using Const;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Minion : MonoBehaviour
{
    [SerializeField] private float health;
    [SerializeField] private float speed;
    [SerializeField] private float damage;

    private MinionState currentState = MinionState.Idle;

    private Animator minionAnimator;
    private Vector3 targetPosition;

    private void Awake()
    {
        minionAnimator = GetComponent<Animator>();
        targetPosition = transform.position; // 초기 위치를 현재 위치로 설정
    }

    void Update()
    {
        // 목적지로 이동
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        // 이동 중인지 확인하여 애니메이션 상태 설정
        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            minionAnimator.SetBool("isWalking", true);
        }
        else
        {
            minionAnimator.SetBool("isWalking", false);
        }

    }

    // 새로운 목적지를 설정하는 함수
    public void SetTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }

    private void StateUpdate(MinionState state)
    {
        currentState = state;

        switch (currentState)
        {
            case MinionState.Idle:
                minionAnimator.SetBool(state.ToString(), true);
                break;
            case MinionState.Attack:
                minionAnimator.SetTrigger(state.ToString());
                break;
            case MinionState.Move:
                break;
            case MinionState.Hurt:
                minionAnimator.SetTrigger(state.ToString());
                break;
            case MinionState.Death:
                minionAnimator.SetBool(state.ToString(), true);
                break;
        }
    }

    public void Move(Vector3 targetPosition)
    {
        minionAnimator.SetBool("Move", true);
    }

    public void OnAttacked()
    {

    }

    public void OnDamaged(float receivedDamage)
    {
        health = (health - receivedDamage) > 0 ? (health - receivedDamage) : 0;

        if (health <= 0)
            OnDeath();
    }

    public void OnDeath()
    {

    }
}
