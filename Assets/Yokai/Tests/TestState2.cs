using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestState2 : ExecutionState
{
    public override List<Type> GetEnterStates()
    {
        return new List<Type>() { typeof(TestState)};
    }

    public override void OnExit()
    {
        Debug.Log($"TestState2.OnExit()");
    }

    public override void OnEnter()
    {
        Debug.Log($"TestState2.OnEnter()");
        Yokai.Register( this );
        Debug.Log($"Current state is {Yokai.CurrentState.Name}");
    }
}