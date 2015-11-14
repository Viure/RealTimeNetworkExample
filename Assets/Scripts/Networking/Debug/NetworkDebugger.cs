using UnityEngine;
using System.Collections;


// Controls NetworkDebugSettings
public class NetworkDebugger : MonoBehaviour 
{    
    #region Client-only debugging settings
    [SerializeField] private bool _localPlayerPrediction = true;  // Turns RewindReplay log functionality on and off for the local player
    [SerializeField] private bool _remotePlayerExtrapolation = false; // Turns extrapolation for remote players on and off
    [SerializeField] private bool _objectInterpolation = true; // Turns object interpolation on/off
    [SerializeField] private bool _playInputLocally = true; // Turns local input effects on/off
    [SerializeField] private bool _showDebugIndicators = false; // Turns the RewindReplay and server position debug indicators
    [SerializeField] private float _simulatedPacketDropPct = 0; // In Unity 5.2.2p2, the network simulator has a bug that crashes the network connection
    #endregion
    [SerializeField] private int _simulatedLatency = 0; // In Unity 5.2.2p2, the network simulator has a bug that crashes the network connection



	// Update is called once per frame
	private void Update () 
    {
        NetworkDebugSettings.LocalPlayerPrediction = _localPlayerPrediction;
        NetworkDebugSettings.RemotePlayerExtrapolation = _remotePlayerExtrapolation;
        NetworkDebugSettings.ObjectInterpolation = _objectInterpolation;
        NetworkDebugSettings.PlayInputLocally = _playInputLocally;
        NetworkDebugSettings.ShowDebugIndicators = _showDebugIndicators;
        NetworkDebugSettings.SimulatedPacketDropPct = _simulatedPacketDropPct;
        NetworkDebugSettings.SimulatedLatency = _simulatedLatency;
	}
}
