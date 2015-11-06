using UnityEngine;
using System.Collections;

public interface IReplayableEntity<T> : IStatefulEntity<EntityState>, IPredictableEntity<T>
{
	
}
