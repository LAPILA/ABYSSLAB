using System;
using Unity.Behavior;

[BlackboardEnum]
public enum MonsterCurrentState
{
    Idle,
	Move,
	Die,
	Wander,
	Patrol,
	Chase,
	Attack
}
