using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MonoTest : MonoBehaviour
{
    [Inject] TestInjectable test;
    [SerializeField] private TMP_Text counterText;

    void Start()
    {
        Debug.Log($"test={test.GetHashCode()}");
        Yokai.Dispatch( "TestEvent", new YokaiEvent() );
        Yokai.PrintAllStores();
        string path = System.IO.Path.Combine( Application.persistentDataPath, "saved_data.txt");
        Yokai.LoadDataStore( path );
        Yokai.PrintAllStores();
        int count = Yokai.GetStoreData<int>("Count");
        Debug.Log($"Loaded Count {count}");
        count++;
        Yokai.SetStoreData("Count", count);
        
        Debug.Log($"DataStore count is {count}");
        Yokai.SetStoreData("fooList", new List<Foo>(){ new Foo() } );
        Yokai.PrintAllSubs();
        
        Yokai.SaveDataStore( path);
    }

    [Subscriber("TestEvent")]
    public void OnTestEvent( YokaiEvent yokaiEvent )
    {
        Debug.Log($"OnTestEvent yokaiEvent={yokaiEvent}");
        //Yokai.Unregister( this );
    }

    [Subscriber("Store.Count")]
    public void OnCountUpdate( int Count )
    {
        Debug.Log($"MonoTest.OnCountUpdate {Count}");
    }

}
