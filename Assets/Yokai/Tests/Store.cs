using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[StoreData]
[System.Serializable]
public class Store
{
    public int Count = 0;
    public List<Foo> fooList = new List<Foo>() { new Foo(), new Foo() };
    public string blorb = "ZOM";
}

[System.Serializable]
public class Foo
{
    public float value = 3.14f;
}