using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class State : ScriptableObject
{
    protected StateMachine stateMachine;
    protected Controller3D player;

    public virtual void InitState(Controller3D player, StateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    // initialization of the state and needed variables
    public virtual void Enter() { }

    // update method run once per frame
    public virtual void UpdateState() { }

    // leaving state, any logic that is needed is executed here
    public virtual void Exit() { }

}
