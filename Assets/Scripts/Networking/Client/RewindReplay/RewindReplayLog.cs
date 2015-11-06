using UnityEngine;
using System.Collections.Generic;


public class RewindReplayLog<T>
{

	private readonly List<RewindReplayLogEntry<T>> _replayRewindQueue;


	public RewindReplayLog()
	{
		_replayRewindQueue = new List<RewindReplayLogEntry<T>>();
	}

	public void LogEntry(uint clientFrame, EntityState state, T input, float deltaTime)
	{
		var entry = new RewindReplayLogEntry<T>(clientFrame, input, state, deltaTime);
		_replayRewindQueue.Add(entry);
	}

	private void CleanUpLog(uint originFrame)
	{
		while (_replayRewindQueue.Count > 0 && _replayRewindQueue[0].ClientFrame < originFrame)
		{
			_replayRewindQueue.RemoveAt(0);
		}
	}

	public int LogSize
	{
		get
		{
			return _replayRewindQueue.Count;
		}
	}		

	public List<RewindReplayLogEntry<T>> Replay(IReplayableEntity<T> targetObject, T currentInput, uint serverFrame, EntityState baseServerState, bool interpolate)
	{
		var originalClientState = targetObject.GetCurrentState();
		var estimated = new List<T>();
		CleanUpLog(serverFrame);

		if (_replayRewindQueue.Count == 0)
		{			
			return _replayRewindQueue;
		}

		var firstClientLogEntry =  _replayRewindQueue[0];
		if (firstClientLogEntry.ClientFrame != serverFrame)
		{
			Debug.LogError("Replay rewind queue logical error! Client Frame: " + firstClientLogEntry.ClientFrame + " Server Frame: " + serverFrame);
			return _replayRewindQueue;
		}

		var baseDelta = Vector3.Distance(firstClientLogEntry.State.position, baseServerState.position);

		targetObject.ApplyState(baseServerState);
		for (int i = 0 ; i < _replayRewindQueue.Count ; i ++)
		{
			var logEntry = _replayRewindQueue[i];
			_replayRewindQueue[i].State = targetObject.GetCurrentState();
			targetObject.ApplyInput(logEntry.Input, logEntry.DeltaTime);
		}

		var replayedClientState = targetObject.GetCurrentState();
		//targetObject.ApplyState(originalClientState);

		var tooFar = Vector3.Distance(originalClientState.position, replayedClientState.position) > 0.05f;


		if (interpolate && tooFar)
		{
			Interpolate(targetObject, originalClientState, replayedClientState);
		}
		return _replayRewindQueue;

	}

	private void Interpolate(IReplayableEntity<T> targetObject, EntityState originalClientState, EntityState predictedState)
	{
		
		// Return the client to its original state so we can interpolate it
		if (Vector3.Distance(originalClientState.position, predictedState.position) < 20f)
		{									
			targetObject.ApplyState(EntityState.MoveTowards(originalClientState, predictedState, 5*Time.deltaTime, 90*Time.deltaTime));
		}
		else
		{
			Debug.LogError("Predicted delta too large, Snapping...");
			targetObject.ApplyState(EntityState.Lerp(originalClientState, predictedState, 5*Time.deltaTime));		
		}
	}
}