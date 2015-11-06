using UnityEngine;
using System;

public struct PlayerInput
{
	public Ship.SideThrustType SideThrust;
	public Ship.ThrustType Thrust;
	public Ship.VerticalThrustType VerticalThrust;
	public Vector3 TargetUp;
	public Vector3 TargetForward;
	public bool HasInput;

	public static bool operator== (PlayerInput a, PlayerInput b)
	{
		return PlayerInput._Equals(a,b);
	}

	public static bool operator!= (PlayerInput a, PlayerInput b)
	{
		return !PlayerInput._Equals(a,b);
	}

	private static bool _Equals(PlayerInput a, PlayerInput b)
	{
		return a.Thrust == b.Thrust && a.TargetUp == b.TargetUp && a.SideThrust == b.SideThrust && a.VerticalThrust == b.VerticalThrust && a.TargetForward == b.TargetForward && a.HasInput == b.HasInput;
	}
}
