using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : StateBase
{
    public MoveState(Minion minion) : base(minion) { }

    public override void OnStateEnter()
    {

        _Minion.followPath.MoveStart();
    }

    public override void OnStateExit()
    {
        _Minion.followPath.StopMove();
    }

    public override void OnStateUpdate()
    {
        MoveDirectionChange();
    }

    // + : 오른쪽, - : 왼쪽
    // 이동할 목표 따라서 방향전환
    private void MoveDirectionChange()
    {
        int direction = _Minion.followPath.CurrentWayPointPosition().x - _Minion.transform.position.x > 0 ? 1 : -1;

        Vector3 dirScale = new Vector3(direction, 1, 1);
        _Minion.transform.localScale = dirScale;
    }
}
