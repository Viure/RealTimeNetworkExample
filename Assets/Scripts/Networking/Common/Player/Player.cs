using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

// Manages communication and state for players
public class Player : NetworkBehaviour
{
    public const float CAMERA_ROTATION_MOUSE_SENSITIVITY = 200;
    public const uint PACKET_CHECKSUM_BASE = 1241244; // I'm pretty sure Unity auto-checksums the UDP when you choose a "reliable" comm channel, but this is for educational purposes and I implemented a poor-man's checksum

    #region Client-only debugging settings
    [SerializeField] private bool _playerPrediction = true;  // Turns RewindReplay log functionality on and off
    [SerializeField] private bool _objectInterpolation = true; // Turns object interpolation on/off
    [SerializeField] private bool _playInputLocally = true; // Turns local input effects on/off
    [SerializeField] private bool _showDebugIndicators = false; // Turns the RewindReplay and server position debug indicators
    [SerializeField] private float _simulatedPacketDropPct = 0; // In Unity 5.2.2p2, the network simulator has a bug that crashes the network connection
    [SerializeField] private int _simulatedLatency = 0; // In Unity 5.2.2p2, the network simulator has a bug that crashes the network connection
    [SerializeField] private Transform _lastServerPositionIndicator; // Transform indicating the last sent server position
    [SerializeField] private LineRenderer _rewindReplayPath; // LineRenderer to show RewindReplay log in real time.
    #endregion

    #region General prefab settings
    [SerializeField] private Ship _shipComponent; // The actuall object that we're going to move. This class (Player) is not the Avatar object, Ship is. 
    [SerializeField] private Transform _cameraFollow; // The specific transform to lock the camera on, its probably set to the same object as _shipComponent.
    #endregion

    // This client only stuff should probably be in another class between Player and Ship. (LocalPlayerController?)
    #region Client-only privates
    private PlayerInput _lastSentInput;
    private EntityState _lastServerState;
    private RewindReplayLog<PlayerInput> _rewindReplayLog;
    private float _lastDeserializeTime;
    private InputController _inputController; 
    private int _lastServerRtt;
    #endregion

    #region Server-only privates
    private PlayerInput _lastClientInput;
    #endregion

    #region Shared privates
    // Because I completely implemented the protocol serialization myself, I shouldn't need any SyncVars,
    // but it looks like if there isn't at least one SyncVar that gets dirty unity won't send the data
    // maybe unity thinks nothing was changed so it doesn't need to send anything
    // (in the future, maybe find a way to set the dirty bit ourselves in the future and lose the [SyncVar])
    [SyncVar]
    private uint _lastServerFrameForPlayer;
    #endregion

    // Like Start() on the server-side, called when server spawns objects, network prefabs have to go through NetworkServer.Spawn(..), 
    // but luckily we use Unity's NetworkManager helper that does that job for us.
    public void Spawn()
    {		
        
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        // Set up local player input if we're the local player
        _inputController = gameObject.AddComponent<InputController>();
        // Attach camera to the player's ship
        var sphereCamera = FindObjectOfType<SphereCamera>();
        // Find the key debug view to pass to the input controller
        var inputDebugView = FindObjectOfType<InputDebugView>();
		
        _inputController.Initialize(sphereCamera, _cameraFollow, inputDebugView);
        _inputController.SetMouseSensitivity(CAMERA_ROTATION_MOUSE_SENSITIVITY);

        // Instantiate a rewind-replay log for this entity, since its the local player we would like to perform state-prediction
        _rewindReplayLog = new RewindReplayLog<PlayerInput>();
    }


    // Commands are part of the new UNet, called from the client side, they will execute on the same entity on the server
    // So the arguments of the method are actually data unity serialized from the client
    // Commands can only be ran from the NetworkIdentity recognized as the local player
    [Command]
    public void CmdSendInput(uint clientFrame, PlayerInput input)
    {		
        // This section runs only on the server
        _lastClientInput = input;
        _lastServerFrameForPlayer = clientFrame;
    }


    #region Client<->Server protocol
    /* Protocol structure:
     * [SERVER_FRAME_UINT][POSITION_VEC3][ROTATION_QUAT][FORCE_VEC3][CHECKSUM_UINT]
     */

    // Poor-man's checksum
    private uint Checksum(EntityState state)
    {
        int crc = 0;
        crc += (int)state.position.x - (int)state.position.y + (int)state.position.z;
        crc += (int)state.rotation.x - (int)state.rotation.y + (int)state.rotation.z - (int)state.rotation.w;
        crc += (int)state.force.x - (int)state.force.y + (int)state.force.z;
        return PACKET_CHECKSUM_BASE + System.Convert.ToUInt32(System.Math.Abs(crc));
    }

    // OnSerialize is called on the server side when the server sends data to the client
    public override bool OnSerialize(NetworkWriter writer, bool forceAll)
    {
        var state = _shipComponent.GetCurrentState();
        writer.WritePackedUInt32(_lastServerFrameForPlayer);
        writer.Write(state.position);
        writer.Write(state.rotation);
        writer.Write(state.force);
        writer.WritePackedUInt32(Checksum(state));
        return true;
    }

    // OnDeserialize is called on the client-side when data is received from the server
    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {               
        var rtt = NetworkClient.allClients[0].GetRTT();

        var serverFrame = reader.ReadPackedUInt32();
        var serverPosition = reader.ReadVector3();
        var serverRotation = reader.ReadQuaternion();
        var serverForce = reader.ReadVector3();
        var checksum = reader.ReadPackedUInt32();

        // Create an EntityState from the data given by the server
        var serverState = new EntityState
            {
                position = serverPosition,
                rotation = serverRotation,
                force = serverForce
            };

        // Drop the packet if we simulate packet drop
        if (_simulatedPacketDropPct > 0)
        {
            var result = Random.Range(0, 100);
            if (result <= _simulatedPacketDropPct)
            {
                Debug.LogWarning("Simulated packet drop, server frame: " + serverFrame);
                return;
            }
        }

        // Perform checksum and drop bad packets
        if (checksum != Checksum(serverState))
        {
            Debug.LogError("PACKET CRC ERROR");
            return;
        }

        // Discard packets that are out of date.
        if (serverFrame < _lastServerFrameForPlayer)
        {
            Debug.LogError("Discarding old packet");
            return; // discard
        }

        if (_simulatedLatency == 0)
        {
            AcceptUpdateFromServer(rtt, serverFrame, serverState);
        }
        else
        {
            StartCoroutine(AcceptUpdateFromServerCoro(rtt, serverFrame, serverState, _simulatedLatency/1000f));
        }
    }

    private IEnumerator AcceptUpdateFromServerCoro(int rtt, uint serverFrame, EntityState serverState, float delay)
    {
        yield return new WaitForSeconds(delay);
        AcceptUpdateFromServer(rtt, serverFrame, serverState);
    }

    private void AcceptUpdateFromServer(int rtt, uint serverFrame, EntityState serverState)
    {
        // Save the last RTT
        _lastServerRtt = rtt;
        // Register fram last packet received from server
        _lastServerFrameForPlayer = serverFrame;
        // Log time of last deserialization
        _lastDeserializeTime = Time.time;
        // Register last known server state
        _lastServerState = serverState;
        // Adjust the debug server position indicator
        _lastServerPositionIndicator.position = serverState.position;
    }


    #endregion


    // Client-side input processing
    private void ProcessAndSendInput()
    {        
        if (_inputController == null)
        {
            return;
        }

        var currentDeltaTime = Time.deltaTime;
        var currentFrame = (uint)Time.frameCount;
        var currentState = _shipComponent.GetCurrentState();

        // Record the current input from the controller/keyboard/mouse
        PlayerInput currentInput = _inputController.GetPlayerInputFromController();
        // First we create a new entry in the rewind-replay log
        _rewindReplayLog.LogEntry(currentFrame, currentState, currentInput, currentDeltaTime);

        if (_playInputLocally) // For debug and demo purposes, we can disable the effect of the input on the local player from the inspector
        {
            // We play the input on our local ship at the client side, in hope the input will provide a similar result on the server side
            _shipComponent.ApplyInput(currentInput, currentDeltaTime);
        }
        // We don't always need to send the input to the server, the server remembers our last input. 
        // We should only send it if it was modified, or once in a while to avoid flooding the communication-channel every frame
        bool shouldSendUpdateToServer = Time.frameCount % 20 == 0 || _lastSentInput != currentInput;
        if (shouldSendUpdateToServer)
        {
            // Send the input to the server
            CmdSendInput(currentFrame, currentInput);
            // Log last sent input for comparison purposes
            _lastSentInput = currentInput;
        }
    }


    private void LocalPlayerPerformReplayRewindPrediction()
    {        
        var updatedReplayLog = _rewindReplayLog.Replay(_shipComponent, _inputController.GetPlayerInputFromController(), _lastServerFrameForPlayer, _lastServerState, _objectInterpolation);

        // Set the vertex of the line renderer used to debug the replay log
        _rewindReplayPath.SetVertexCount(updatedReplayLog.Count);
        for (int i = 0; i < updatedReplayLog.Count; i++)
        {
            _rewindReplayPath.SetPosition(i, updatedReplayLog[i].State.position);
        }

        // Shout if the log gets too big
        if (_rewindReplayLog.LogSize > 300)
        {
            Debug.LogWarning("Too many replay log entries");
        }
    }

    private void LocalPlayerPerformInterpolationWithoutPrediction()
    {
        if (_objectInterpolation)
        {
            _shipComponent.ApplyState(EntityState.Lerp(_shipComponent.GetCurrentState(), _lastServerState, 5 * Time.deltaTime));
        }
        else
        {
            _shipComponent.ApplyState(_lastServerState);
        }
    }

    // This method performs client-side prediction for players that are not the local player
    private void RemotePlayerPerformExtrapolation()
    {
        var targetState = _lastServerState;
        if (_playerPrediction)
        {
            var serverRttCompensation = Mathf.Max(0, _lastServerRtt/1000); 
            var secondsSinceLastServerState = Time.time - _lastDeserializeTime + serverRttCompensation;
            // Perform dead reckoning
            targetState.position += _lastServerState.force * Time.deltaTime * secondsSinceLastServerState; // + rtt/1000);
        }
        //var newState = EntityState.Lerp(_lastServerState, targetState, Time.deltaTime);
        if (_objectInterpolation)
        {
            _shipComponent.ApplyState(EntityState.Lerp(_shipComponent.GetCurrentState(), targetState, 2 * Time.deltaTime));
        }
        else
        {
            _shipComponent.ApplyState(targetState);
        }
    }

    private void UpdateServerSide()
    {
        _shipComponent.ApplyInput(_lastClientInput, Time.deltaTime);
    }

    private void UpdateClientSide()
    {
        if (isLocalPlayer)
        {
            ProcessAndSendInput();
            bool serverStartedSendingPackets = _lastServerFrameForPlayer > 0;
            if (_playerPrediction && serverStartedSendingPackets)
            {
                LocalPlayerPerformReplayRewindPrediction();
            }
            else
            {
                LocalPlayerPerformInterpolationWithoutPrediction();
            }
        }
        else
        {
            RemotePlayerPerformExtrapolation();
        }            
        SetDebugIndicatorsVisibility(_showDebugIndicators);
    }

    private void Update()
    {        
        if (isServer)
        {			
            UpdateServerSide();
        }
        else
        {
            UpdateClientSide();
        }    
    }

    private bool _lastDebugIndicatorVisibility = true;

    // Lazy setting the debug indicators visibility to avoid calling .gameObject on every frame (We want the parameter controlled at real time from the inspector for demo purposes)
    private void SetDebugIndicatorsVisibility(bool v)
    {
        if (_lastDebugIndicatorVisibility != v)
        {
            _lastDebugIndicatorVisibility = v;
            _lastServerPositionIndicator.gameObject.SetActive(_lastDebugIndicatorVisibility);
            _rewindReplayPath.gameObject.SetActive(_lastDebugIndicatorVisibility);
        }
    }


}
