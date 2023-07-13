using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SubscriberAttribute : System.Attribute
{
    private string Name;

    public SubscriberAttribute( string name )
    {
        Name = name;
    }

    public string GetName() => Name;
}