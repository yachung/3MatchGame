using System;
using UnityEngine;

public class StateManager
{
    public Action<StateBase> OnStateChanged;

    public StateManager(StateBase initState)
    {
        _curState = initState;
        ChangeState(_curState);
    }

    private StateBase _curState;

    public void ChangeState(StateBase nextState)
    {
        if (nextState == _curState)
            return;

        if (_curState != null)
            _curState.OnStateExit();

        _curState = nextState;
        _curState.OnStateEnter();

        OnStateChanged?.Invoke(nextState);
    }

    public void UpdateState()
    {
        if (_curState != null)
            _curState.OnStateUpdate();
    }
}