using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("PlayerIdleState"))]
public class PlayerIdleState : State
{
    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if(player.GetInput().magnitude > float.Epsilon)
        {
            stateMachine.ChangeState<PlayerWalkState>();
        }

        if (player.Body.Grounded && Input.GetKeyDown(KeyCode.Space))
        {
            stateMachine.ChangeState<PlayerJumpState>();
        }
    }
}
