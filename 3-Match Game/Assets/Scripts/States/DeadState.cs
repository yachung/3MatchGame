using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : StateBase
{
    private float deadDelay = 4f;

    public DeadState(Minion minion) : base(minion) { }

    public override void OnStateEnter()
    {
        ObjectPoolingManager.Instance.DelayReturnObject(_Minion.name, _Minion.gameObject, deadDelay);
    }

    public override void OnStateExit()
    {

    }

    public override void OnStateUpdate()
    {

    }
}
