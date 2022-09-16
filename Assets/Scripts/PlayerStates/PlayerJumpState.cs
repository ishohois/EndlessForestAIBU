using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("PlayerJumpState"))]
public class PlayerJumpState : State
{
    [SerializeField] private float jumpFactor = 5f;
    public override void Enter()
    {
        base.Enter();
        Vector3 jumpForce = Vector3.up * jumpFactor;
        player.Body.AddForce(jumpForce);

    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (player.Body.Grounded)
        {
            if (player.Body.Velocity.z <= 0.0005f)
            {

                stateMachine.ChangeState<PlayerIdleState>();
            }
            else
            {
                stateMachine.ChangeState<PlayerWalkState>();
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
