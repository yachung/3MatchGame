using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BringerOfDeath : Minion
{
    public enum State
    {
        None = -1,

        Idle = 0,
        Move,
        Hurt,
        Attack,
        Dead,

        Count
    }

    private StateManager stateManager;
    private State curState;
    private bool isDetectEnemy;

    void Start()
    {
        curState = State.Idle;
        stateManager = new StateManager(new IdleState(this));
        stateManager.OnStateChanged += OnBODAnimationChanged;
    }

    void Update()
    {
        switch (curState)
        {
            case State.Idle:
                if (!followPath.IsEndPoint)
                {
                    if (isDetectEnemy)
                        ChangeState(State.Attack);
                    else
                        ChangeState(State.Move);
                }
                break;
            case State.Move:
                if (!followPath.IsEndPoint)
                {
                    if (isDetectEnemy)
                        ChangeState(State.Attack);
                }
                else
                    ChangeState(State.Idle);
                break;
            case State.Attack:
                if (!isDetectEnemy)
                {
                    if (!followPath.IsEndPoint)
                        ChangeState(State.Move);
                    else
                        ChangeState(State.Idle);
                }
                break;
        }

        stateManager.UpdateState();
    }

    private void ChangeState(State nextState)
    {
        curState = nextState;
        switch (curState)
        {
            case State.Idle:
                stateManager.ChangeState(new IdleState(this));
                break;
            case State.Move:
                stateManager.ChangeState(new MoveState(this));
                break;
            case State.Hurt:
                stateManager.ChangeState(new HurtState(this));
                break;
            case State.Attack:
                stateManager.ChangeState(new AttackState(this));
                break;
            case State.Dead:
                stateManager.ChangeState(new DeadState(this));
                break;
        }

        Debug.Log($"curState : {nextState}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            if (targetMinion == null)
            {
                TryGetComponent(out targetMinion);
            }

            if (targetMinion != null)
            {
                Debug.Log($"Detect Target : {targetMinion.name}");
                isDetectEnemy = true;
            }
        }
    }

    private void OnBODAnimationChanged(StateBase newState)
    {
        refAnimator.SetInteger("State", (int)curState);
    }
}
