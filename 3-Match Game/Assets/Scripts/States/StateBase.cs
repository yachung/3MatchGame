using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBase
{
    protected Minion _Minion;
    //protected Transform refTransform;

    protected StateBase(Minion minion)
    {
        _Minion = minion;
        //refTransform = transform;
    }

    public abstract void OnStateEnter();
    public abstract void OnStateUpdate();
    public abstract void OnStateExit();
}
