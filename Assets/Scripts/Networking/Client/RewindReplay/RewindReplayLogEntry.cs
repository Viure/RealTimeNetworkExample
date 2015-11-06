using UnityEngine;

public class RewindReplayLogEntry<T>
{
	public readonly uint ClientFrame;
	public readonly float DeltaTime;
	public EntityState State;
	public readonly T Input;

	public RewindReplayLogEntry(uint clientFrame, T playerInput, EntityState state, float deltaTime)
	{
		ClientFrame = clientFrame;
		Input = playerInput;
		State = state;
		DeltaTime = deltaTime;
	}
}
