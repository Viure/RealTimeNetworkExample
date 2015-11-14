using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour
{    
    public const KeyCode KEYCODE_THRUST_FORWARD = KeyCode.W;
    public const KeyCode KEYCODE_THRUST_BACKWARD = KeyCode.S;
    public const KeyCode KEYCODE_THRUST_RIGHT = KeyCode.D;
    public const KeyCode KEYCODE_THRUST_LEFT = KeyCode.A;
    public const KeyCode KEYCODE_THRUST_UP = KeyCode.Space;
    public const KeyCode KEYCODE_THRUST_DOWN = KeyCode.LeftControl;
    public const KeyCode KEYCODE_VIEW_ROTATE_CLOCKWISE = KeyCode.E;
    public const KeyCode KEYCODE_VIEW_ROTATE_COUNTER_CLOCKWISE = KeyCode.Q;
    public const int MOUSE_BUTTON_ADJUST_ROTATION = 1;

    private InputDebugView _inputDebugView; // A canvas view showing the status of keys for educational purposes
    private SphereCamera _sphereCamera; // Sphere camera of which rotation controls the input when the rotation button is pressed
    private Transform _cameraFollow;

    [SerializeField]
    float _mouseSensitivity = 30;  

    public void Initialize(SphereCamera sphereCamera, Transform cameraFollow, InputDebugView inputDebugView)
    {
        _sphereCamera = sphereCamera;
        _cameraFollow = cameraFollow;
        _inputDebugView = inputDebugView;
        enabled = true;
    }

    public void SetMouseSensitivity(float v)
    {
        _mouseSensitivity = v;
    }

    public PlayerInput GetPlayerInputFromController()
    {        
        _inputDebugView.Reset(); // reset the indication on the key debug view

        var result = new PlayerInput();
        if (_sphereCamera == null)
        {
            return result;
        }

        // Main thrust
        if (Input.GetKey(KEYCODE_THRUST_FORWARD))
        {
            result.Thrust = Ship.ThrustType.Forward;
            result.HasInput = true;
            _inputDebugView.MarkPressed(KEYCODE_THRUST_FORWARD);
        }
        else if (Input.GetKey(KEYCODE_THRUST_BACKWARD))
        {
            result.Thrust = Ship.ThrustType.Reverse;
            result.HasInput = true;
            _inputDebugView.MarkPressed(KEYCODE_THRUST_BACKWARD);
        }
        else
        {
            result.Thrust = Ship.ThrustType.None;
        }

        // Sideways thrust
        if (Input.GetKey(KEYCODE_THRUST_LEFT))
        {
            result.SideThrust = Ship.SideThrustType.Left;
            result.HasInput = true;
            _inputDebugView.MarkPressed(KEYCODE_THRUST_LEFT);
        }
        else if (Input.GetKey(KEYCODE_THRUST_RIGHT))
        {
            result.SideThrust = Ship.SideThrustType.Right;
            result.HasInput = true;
            _inputDebugView.MarkPressed(KEYCODE_THRUST_RIGHT);
        }
        else
        {
            result.SideThrust = Ship.SideThrustType.None;
        }

        // Vertical thrust
        if (Input.GetKey(KEYCODE_THRUST_UP))
        {
            result.VerticalThrust = Ship.VerticalThrustType.Up;
            result.HasInput = true;
        }
        else if (Input.GetKey(KEYCODE_THRUST_DOWN))
        {
            result.VerticalThrust = Ship.VerticalThrustType.Down;
            result.HasInput = true;
        }
        else
        {
            result.VerticalThrust = Ship.VerticalThrustType.None;
        }

        // Rotation: if the button is pressed, target rotation will align with camera rotation
        if (Input.GetMouseButton(MOUSE_BUTTON_ADJUST_ROTATION))
        {
            result.TargetForward = _sphereCamera.Forward;
            result.TargetUp = _sphereCamera.Up;
            result.HasInput = true;
        }
        return result;
    }

    void LateUpdate()
    {
        if (_sphereCamera == null)
        {
            return;
        }
        _sphereCamera.FocalPosition = _cameraFollow.position;
        {
            _sphereCamera.Rotate(Input.GetAxisRaw("Mouse X") * Vector3.up * Time.deltaTime * _mouseSensitivity, Space.Self);
            _sphereCamera.Rotate(-Input.GetAxisRaw("Mouse Y") * Vector3.left * Time.deltaTime * _mouseSensitivity, Space.Self);
        }
        if (Input.GetKey(KEYCODE_VIEW_ROTATE_CLOCKWISE))
        {
            _sphereCamera.Rotate(-1 * Vector3.forward * Time.deltaTime * _mouseSensitivity, Space.Self);
        }
        else if (Input.GetKey(KEYCODE_VIEW_ROTATE_COUNTER_CLOCKWISE))
        {
            _sphereCamera.Rotate(1 * Vector3.forward * Time.deltaTime * _mouseSensitivity, Space.Self);
        }
        if (Input.mouseScrollDelta.y != 0)
        {
            _sphereCamera.MaxDistance -= Input.mouseScrollDelta.y;
        }
    }
}
