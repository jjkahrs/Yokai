# Yokai

Yokai is a framework for Unity that encapsulates:
- Dependency Injection
- Event Publish/Subscribe Messaging
- Execution State Management
- Data Binding
- Data Persistence

## Setup
Add the Yokai component to it's own GameObject in your first scene. This component will keep this object from being destroyed between scenes.

## Execution States
The current state may always be found at the Yokai.CurrentState property.

```
// To create a state, have a class extend ExecutionState
// If that State should be the initial state, then
// Add the StartingState attribute

[StartingState]
public class TestState : ExecutionState
{
    // This should return a list of valid state types that
    // this state is permmited to exit to

    public override List<Type> GetExitStates()
    {
        return new List<Type>() { typeof(TestState2)};
    }

    // Called when the State is entered
    public override void OnEnter()
    {
        // This call registers this state so any event listeners will
        // be properly added

        Yokai.Register( this );
    }

    // The last thing called before leaving this state
    // and entering the new one

    public override void OnExit()
    {
        // This will clean up and remove any events this
        // state was subscribed to.

        Yokai.Unregister( this );
    }

}
```

## Events
```
[Subscriber("TestEvent")]
public void OnTestEvent( AnyClassHere data )
{
    // Tells the framework to call this method
    // whenever a TestEvent message is broadcast
}

```

```
Yokai.Dispatch( "TestEvent", new AnyClassHere() )
// This dispatches an event to all subscribed methods

```

## Dependency Injection
```
[Injectable]
public class TestInjectable
{
    // Your class you want available for injection
}
```

```
public class Test : MonoBehaviour
{
    [Inject] TestInjectable test;
}
```

## Data Store and Binding

```
[StoreData]
[System.Serializable]
public class Store
{
    // All classes with the StoreData attribute will be instantiated when Awaken() is called
    // This only happens once.

    // Any public fields here but insure all types are Serializable
    public int Count = 0;
    public List<Foo> fooList = new List<Foo>() { new Foo(), new Foo() };
    public string blorb = "ZOM";
}

[System.Serializable]
public class Foo
{
    public float value = 3.14f;
}
```

```
[Subscriber("Store.Count")]
public void OnCountUpdate( int Count )
{
    // This will bind this method to be called when the value of Store.Count is updated
}
```

## Data Persistence
```
// This will save the managed data state to a JSON file
// Or load it from a previous save

string path = System.IO.Path.Combine( Application.persistentDataPath, "saved_data.json");
Yokai.LoadDataStore( path );
Yokai.SaveDataStore( path);

```