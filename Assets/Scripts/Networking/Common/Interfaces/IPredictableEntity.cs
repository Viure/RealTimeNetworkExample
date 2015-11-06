using UnityEngine;
using System.Collections;

public interface IPredictableEntity<T> 
{
	void ApplyInput(T input, float deltaTime);
}
