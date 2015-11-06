using UnityEngine;

public struct EntityState
{
	public Vector3 position;
	public Vector3 force;
	public Quaternion rotation;

	public static EntityState Lerp(EntityState sourceState, EntityState targetState, float t)
	{
		return new EntityState
		{
			position = Vector3.Lerp(sourceState.position, targetState.position, t),
			force = Vector3.Lerp(sourceState.force, targetState.force, t),
			rotation = Quaternion.Lerp(sourceState.rotation, targetState.rotation, t)
		};
	}

	public static EntityState MoveTowards(EntityState sourceState, EntityState targetState, float maxDistance, float maxAngle)
	{
		return new EntityState
		{
			position = Vector3.MoveTowards(sourceState.position, targetState.position, maxDistance),
			rotation = Quaternion.RotateTowards(sourceState.rotation, targetState.rotation, maxAngle),
			force = Vector3.MoveTowards(sourceState.force, targetState.force, maxDistance),
		};
	}
}
