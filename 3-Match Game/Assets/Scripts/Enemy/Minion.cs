using Const;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Minion : MonoBehaviour
{
    private float health;
    private float speed;
    private int damage;
    private float attackDelay;

    protected Animator refAnimator;
    private Vector3 targetPosition;
    public FollowPath followPath;
    protected Collider2D minionCollider;

    protected Collider2D targetCollider = null;
    protected Minion targetMinion = null;

    private void Awake()
    {
        refAnimator = GetComponentInChildren<Animator>();
        followPath = GetComponent<FollowPath>();
        minionCollider = GetComponent<Collider2D>();
        targetPosition = transform.position; // 초기 위치를 현재 위치로 설정
    }

    public void Initialized(Vector3[] wayPoints, float speed, float waitTime)
    {
        followPath.Initialize(wayPoints, speed, waitTime);
    }

    // 새로운 목적지를 설정하는 함수
    public void SetTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }

    // 들어온 collider가 Enemy라면 멈춤
    // 현재 타겟이 없다면 들어온 콜라이더를 타겟으로 설정.
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.tag == "Enemy")
    //    {
    //        if (targetCollider == null)
    //        {
    //            targetCollider = collision;
    //            targetMinion = targetCollider.GetComponent<Minion>();
    //            attackCoroutine = StartCoroutine(CoAttack());
    //        }
    //    }
    //}

    // 나간 collider가 나와 태그가 다르고, 콜라이더 내부에 같은 태그만 있다면 다시 움직인다.
    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.tag == "Enemy" && IsColliderEmpty())
    //        StateUpdate(MinionState.Move);
    //}

    private void OnTriggerStay2D(Collider2D collision)
    {
        //if (targetCollider == null)
        //{
        //    return;
        //}

        //if (targetCollider != null && targetCollider == collision)
        //{
        //    StartCoroutine(attackCoroutine);
        //}
    }

    //bool IsColliderEmpty()
    //{
    //    Collider2D[] hitColliders = Physics2D.OverlapBoxAll(minionCollider.bounds.center, minionCollider.bounds.size, 0f);
    //    foreach (Collider2D hitCollider in hitColliders)
    //    {
    //        if (hitCollider.tag == "Enemy") // 범위 내에 있는 콜라이더가 나와 다른 태그를 가졌다면 false 리턴
    //        {
    //            return false;
    //        }
    //    }
    //    return true;
    //}

    //private void AnimationTrigger(MinionState state)
    //{
    //    refAnimator.SetTrigger(state.ToString());
    //}

    //private void StateUpdate(MinionState state, bool isActive = true)
    //{
    //    // 똑같은 상태 입력되거나 죽은상태라면 그냥 리턴
    //    if (currentState == state || currentState == MinionState.Death)
    //        return;

    //    currentState = state;

    //    switch (currentState)
    //    {
    //        case MinionState.Idle:
    //            refAnimator.SetBool(MinionState.Move.ToString(), false);
    //            refAnimator.SetBool(MinionState.Death.ToString(), false);
    //            followPath.StopMove();
    //            break;
    //        case MinionState.Move:
    //            refAnimator.SetBool(currentState.ToString(), isActive);
    //            if (isActive)
    //                followPath.MoveStart();
    //            else
    //                followPath.StopMove();
    //            break;
    //        case MinionState.Death:
    //            refAnimator.SetBool(currentState.ToString(), isActive);
    //            break;

    //        case MinionState.Attack:
    //            refAnimator.SetTrigger(currentState.ToString());
    //            break;
    //        case MinionState.Hurt:
    //            refAnimator.SetTrigger(currentState.ToString());
    //            break;
    //    }
    //}

    //public void Move(Vector3 targetPosition)
    //{
    //    refAnimator.SetBool("Move", true);
    //}

    //Coroutine attackCoroutine = null;

    //IEnumerator CoAttack()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(attackDelay);

    //        OnAttacked();
    //    }
    //}

    //public void OnSpawned()
    //{

    //}

    //public void OnAttacked()
    //{
    //    if (targetMinion.GetState() == MinionState.Death)
    //    {
    //        targetMinion = null;
    //        targetCollider = null;
    //        StopCoroutine(attackCoroutine);
    //        attackCoroutine = null;
    //        return;
    //    }

    //    AnimationTrigger(MinionState.Attack);
    //    targetMinion.OnDamaged(damage);
    //}

    //public void OnDamaged(int receivedDamage)
    //{
    //    AnimationTrigger(MinionState.Hurt);

    //    health = (health - receivedDamage) > 0 ? (health - receivedDamage) : 0;

    //    if (health <= 0)
    //        OnDeath();
    //}

    //public void OnDeath()
    //{
    //    Invoke("ReturnObject", 4f);
    //}

    public void ReturnObject(float deadDelay)
    {
        StartCoroutine(CoReturnObject(deadDelay));
    }

    private IEnumerator CoReturnObject(float deadDelay)
    {   
        yield return new WaitForSeconds(deadDelay);

        ObjectPoolingManager.Instance.ReturnObject(this.name, this.gameObject);
    }
}
