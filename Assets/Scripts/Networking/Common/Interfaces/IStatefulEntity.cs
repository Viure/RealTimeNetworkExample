using UnityEngine;
using System.Collections;

public interface IStatefulEntity<T>
{
	T GetCurrentState();
	void ApplyState(T state);
}
