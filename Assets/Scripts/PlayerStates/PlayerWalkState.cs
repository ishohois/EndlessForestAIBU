using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = ("PlayerWalkState"))]
public class PlayerWalkState : State
{
    [SerializeField] private float maxSpeed = 10f;
    private Vector3 input;


    public override void Enter()
    {
        base.Enter();
    }

    public override void UpdateState()
    {
        base.UpdateState();

        input = player.GetInput();
        if (input.magnitude > float.Epsilon)
        {
            player.Body.Accelerate(input, maxSpeed);
        }
        else
        {
            player.Body.Decelerate();
        }

        if (player.Body.Grounded && Input.GetKeyDown(KeyCode.Space))
        {
            stateMachine.ChangeState<PlayerJumpState>();
        }

        if (player.Body.Velocity.magnitude <= 0.0005f)
        {
            stateMachine.ChangeState<PlayerIdleState>();
        }
    }

    public override void Exit()
    {
        base.Exit();
       

    }


}
