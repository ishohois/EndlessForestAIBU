using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StateMachine
{

    private Dictionary<System.Type, State> playerStates = new Dictionary<System.Type, State>();
    [SerializeField] private State currentState;
    [SerializeField] private State queuedState;

    public StateMachine(State[] states, Controller3D controller)
    {
        if (states != null)
        {
            foreach (State s in states)
            {
                State stateInstance = Object.Instantiate(s);
                stateInstance.InitState(controller, this);
                playerStates.Add(stateInstance.GetType(), stateInstance);

                currentState ??= stateInstance;
            }
        }
        queuedState = currentState;
        currentState.Enter();
    }

    public void UpdateStates()
    {
        if (currentState != queuedState)
        {
            currentState.Exit();
            currentState = queuedState;
            currentState.Enter();
        }
        currentState.UpdateState();
    }

    public void ChangeState<T>() where T : State
    {
        if (playerStates.ContainsKey(typeof(T)))
        {
            queuedState = playerStates[typeof(T)];
        }
    }

}
