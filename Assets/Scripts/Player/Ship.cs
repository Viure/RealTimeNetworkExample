using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Ship : MonoBehaviour, IReplayableEntity<PlayerInput>
{	
	public enum ThrustType
	{
		None, Forward, Reverse
	}

	public enum SideThrustType
	{
		None, Left, Right
	}

	public enum VerticalThrustType
	{
		None, Up, Down
	}

	[SerializeField]
	private float _maxForceMagnitude;

	[SerializeField]
	private float _rotationSpeed = 10;

	[SerializeField]
	private float _thrustForce = 10;

	[SerializeField]
	private float _decceleration = 2;

	[SerializeField]
	private CharacterController _charController;



	private Transform _myTransform;

	private Vector3 Force = Vector3.zero;


	private Vector3 _targetUp = Vector3.up;



	private Vector3 TargetForward { get; set;}

	private Vector3 TargetUp {
		get 
		{
			return _targetUp;
		}
		set
		{
			if (value.magnitude == 0)
			{
				return;
			}
			_targetUp = value.normalized;
		}
	}



	private Vector3 Position
	{
		get
		{
			return _myTransform.position;
		}
		set
		{
			_myTransform.position = value;
		}
	}


	private Quaternion Rotation
	{
		get 
		{
			return _myTransform.rotation;
		}
		set
		{			
			_myTransform.rotation = value;
		}
	}

	// Use this for initialization
	private void Awake () 
	{
		_myTransform = transform;
		//_myRigidBody = GetComponent<Rigidbody>();
		//_charController = GetComponent<CharacterController>();
	}
		
	private void AddForce(Vector3 f)
	{
		Force = Vector3.ClampMagnitude(Force + f, _maxForceMagnitude);
	}

	private void AddFriction(float deltaTime)	
	{
		Force -= Force.normalized * Mathf.Min(_decceleration * deltaTime, Force.magnitude);
	}

	private Vector3 GetThrusterForce(PlayerInput input)
	{
		Vector3 thrustForce;
		switch (input.Thrust)
		{
		case (ThrustType.Forward):
			{
				thrustForce = Time.deltaTime * _thrustForce * _myTransform.forward.normalized;
				break;
			}
		case (ThrustType.Reverse):
			{
				thrustForce = Time.deltaTime * _thrustForce * -_myTransform.forward.normalized;
				break;
			}
		default:
			{
				thrustForce = Vector3.zero;
				break;
			}
		}
		switch (input.SideThrust)
		{
		case (SideThrustType.Left):
			{
				thrustForce += Time.deltaTime * _thrustForce * -_myTransform.right.normalized;
				break;
			}
		case (SideThrustType.Right):
			{
				thrustForce += Time.deltaTime * _thrustForce * _myTransform.right.normalized;
				break;
			}
		}
		switch (input.VerticalThrust)
		{

		case (VerticalThrustType.Down):
			{
				thrustForce += Time.deltaTime * _thrustForce * -_myTransform.up.normalized;
				break;
			}
		case (VerticalThrustType.Up):
			{
				thrustForce += Time.deltaTime * _thrustForce * _myTransform.up.normalized;
				break;
			}
		}
		return thrustForce;
	}

	public void ApplyState(EntityState state)
	{		
		Rotation = state.rotation;
		Position = state.position;
		Force = state.force;
	}

	public EntityState GetCurrentState()
	{
		
		return new EntityState
		{
			position = Position,
			rotation = Rotation,
			force = Force
		};
	}

	public void ApplyInput(PlayerInput input, float deltaTime)
	{
		AddFriction(deltaTime);
		var inputForce = GetThrusterForce(input);

		if (input.HasInput)
		{
			if (input.TargetForward.magnitude > 0)
			{
				Quaternion targetRotation = Quaternion.LookRotation(input.TargetForward, input.TargetUp);
				var currentRotation = Quaternion.RotateTowards(Rotation, targetRotation, deltaTime*_rotationSpeed);	
				Rotation = currentRotation;
			}
			AddForce(inputForce);				
		}

		var movementVector = Force * deltaTime;
		_charController.Move(movementVector);
	}


}
