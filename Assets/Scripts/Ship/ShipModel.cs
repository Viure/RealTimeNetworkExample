using UnityEngine;
using System.Collections;

public class ShipModel : MonoBehaviour {

	[SerializeField]
	private float _moveSpeed = 100f;
	[SerializeField]
	private float _rotationSpeed = 30f;

	[SerializeField]
	private Transform _follow;
	private Transform _myTransform;
	private void Awake()
	{
		_myTransform = GetComponent<Transform>();
	}

	// Update is called once per frame
	void Update () {
		_myTransform.rotation = Quaternion.RotateTowards(_myTransform.rotation, _follow.rotation, _rotationSpeed*Time.deltaTime);
		var deltaVector = _follow.position - _myTransform.position;
		_myTransform.position += Vector3.ClampMagnitude(deltaVector*_moveSpeed*Time.deltaTime, deltaVector.magnitude);
	}
}
