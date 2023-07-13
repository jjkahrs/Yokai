using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ExecutionState
{
    public virtual List<Type> GetEnterStates()
    {
        return new List<Type>();
    }

    public virtual List<Type> GetExitStates()
    {
        return new List<Type>();
    }

    // Called when first entering the state
    public virtual void OnEnter()
    {
    }

    // Called last before switching to new state
    public virtual void OnExit()
    {
    }

    // Called every frame
    public virtual void OnUpdate()
    {
    }
}