using UnityEngine;
using System.Collections;

public class SphereCamera : MonoBehaviour {
	[SerializeField]
	private float StartHeight = 0;
	[SerializeField]
	private float StartDistance = 10;
	[SerializeField]
	private bool AutoAdjustDistanceFromCollision;

	private Transform _focalTransform = null;
	private Transform _myTransform;

	private void Update()
	{

		if (!AutoAdjustDistanceFromCollision)
		{
			Distance = MaxDistance;
			return;
		}
		Ray cameraFocalRay = new Ray(_focalTransform.position, _myTransform.position - _focalTransform.position);
		RaycastHit hit;
		if (Physics.Raycast(cameraFocalRay, out hit, MaxDistance))
		{
			Distance = Mathf.Lerp(Distance, hit.distance - 0.3f, Time.deltaTime*50);
		}
		else
		{
			Distance = Mathf.Lerp(Distance,MaxDistance, Time.deltaTime*50);;
		}

	}

	private void OnEnable()
	{
		var parent = transform.parent;
		if (parent != null && transform.parent.GetComponent<SphereCameraFocal>() != null)
		{
			_focalTransform = transform.parent;
		}
		if (_focalTransform == null) 
		{
			Debug.LogError("Error: Camera parent has no SphereCameraFocal component");
		}
		_myTransform = transform;
		MaxDistance = StartDistance;
		Height = StartHeight;
		_myTransform.rotation = Quaternion.identity;
	}

	public void Rotate(Vector3 rotation, Space relativeTo)
	{
		if (_focalTransform == null)
		{
			return;
		}
		_focalTransform.Rotate(rotation, relativeTo);
	}

	public float MaxDistance;

	private float Distance
	{
		get 
		{
			return -_myTransform.localPosition.z;
		}
		set
		{
			//_myTransform.rotation = Quaternion.identity;
			_myTransform.localPosition = new Vector3(0,_myTransform.localPosition.y, -value);
		}
	}

	public float Height
	{
		get 
		{
			return -_myTransform.localPosition.y;
		}
		set
		{
			//_myTransform.rotation = Quaternion.identity;
			_myTransform.localPosition = new Vector3(0, -value, _myTransform.localPosition.z);
		}
	}

	public void LookAt(Vector3 point, Vector3 up)
	{
		var deltaVector = point - _focalTransform.position;
		Rotation = Quaternion.LookRotation(deltaVector, up);
	}

	public Quaternion Rotation
	{
		get
		{
			return _focalTransform.rotation;
		}
		set
		{
			_focalTransform.rotation = value;
		}
	}

	public Vector3 Forward
	{
		get
		{
			return _focalTransform.forward;
		}
		set
		{
			_focalTransform.forward = value;
		}
	}

	public Vector3 FocalPosition
	{
		get
		{
			return _focalTransform.position;
		}
		set
		{
			if (_focalTransform == null)
			{
				return;
			}
			_focalTransform.position = value;
		}
	}

	public Vector3 Up
	{
		get
		{
			return _focalTransform.up;
		}
		set
		{
			_focalTransform.up = value;
		}
	}

}
