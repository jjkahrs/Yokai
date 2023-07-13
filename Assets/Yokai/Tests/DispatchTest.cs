using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispatchTest : MonoBehaviour
{
    public void OnClick()
    {
        Yokai.Dispatch( "TestEvent", new YokaiEvent() );
    }
}
