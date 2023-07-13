using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[StartingState]
public class TestState : ExecutionState
{

    public override List<Type> GetExitStates()
    {
        return new List<Type>() { typeof(TestState2)};
    }

    [Subscriber("TestEvent")]
    public void OnTestEvent( YokaiEvent yokaiEvent )
    {
        Debug.Log("TestState.OnTestEvent()");
        Yokai.ChangeState( new TestState2());
    }

    public override void OnEnter()
    {
        Debug.Log($"TestState.OnEnter()");
        Yokai.Register( this );
    }

    public override void OnExit()
    {
        Debug.Log($"TestState.OnExit()");
        Yokai.Unregister( this );
    }

}