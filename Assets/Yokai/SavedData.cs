using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SavedData
{
    public int version = 0;
    public string author = "Nobody";
    public bool flag = true;
    public List<object> stores = new List<object>();
}