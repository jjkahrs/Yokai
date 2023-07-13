using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using UnityEngine.SceneManagement;


public sealed class Yokai : MonoBehaviour
{
    private bool isLoaded = false;
    static private Dictionary<Type, System.Object> injectablesTable = new Dictionary<Type, System.Object>();
    static private Dictionary<string, List<Subscription>> subscriberTable = new Dictionary<string, List<Subscription>>();
    static private Dictionary<string, FieldBinding> storeDataFieldTable = new Dictionary<string, FieldBinding>();
    static private List<object> storeDataObjectList = new List<object>();
    static private ExecutionState currentState;
    static public Type CurrentState {
        get { return currentState.GetType();}
        private set {}
    }

    struct Subscription
    {
        public Delegate del;
        public object subscriber;

        public Subscription( Delegate d, object obj )
        {
            del = d;
            subscriber = obj;
        }

    }

    struct FieldBinding
    {
        public object obj;
        public FieldInfo field;

        public FieldBinding( object o, FieldInfo field )
        {
            obj = o;
            this.field = field;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad( transform.gameObject );

        if( !isLoaded )
            LoadInjectablesAndData();

        if( isLoaded )
        {
            ScanRegisterAndInject();
            currentState.OnEnter();
        }
    }

    private void LoadInjectablesAndData()
    {
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();

        foreach( Type t in types )
        {
            if(t.IsDefined( typeof( InjectableAttribute ) ) )
            {
                object obj = FormatterServices.GetUninitializedObject( t );
                injectablesTable.Add( t, obj );
            }

            if(t.IsDefined( typeof( StoreDataAttribute ) ) )
            {
                object obj = FormatterServices.GetUninitializedObject( t );
                storeDataObjectList.Add( obj );

                foreach( FieldInfo info in t.GetFields())
                    storeDataFieldTable.Add( info.Name, new FieldBinding(obj,info));
            }

            if(t.IsDefined( typeof( StartingStateAttribute ) ) )
            {
                if( t.BaseType != typeof(ExecutionState) )
                {
                    Debug.LogError($"ERROR: Invalid starting state {t.Name} does not extend {typeof( ExecutionState ).Name}");
                    continue;
                }

                if( currentState == null )
                {
                    currentState = (ExecutionState) Activator.CreateInstance( t );
                }
                else
                {
                    Debug.LogError($"ERROR: More than one starting state attributed. Using first one foudn.");
                }
            }
        }

        if( currentState == null )
        {
            Debug.LogError($"ERROR: No starting state found");
        }

        isLoaded = true;
    }

    // Method for debugging
    public static void PrintAllSubs()
    {
        foreach (  string key in subscriberTable.Keys)
        {
            Debug.Log($"@@ {key} {subscriberTable[key].Count}");
        }
    }

    public static void PrintAllStores()
    {
        foreach ( object obj in storeDataObjectList )
        {
            Debug.Log($"==> {obj.GetType().Name} {obj}");
            if (obj is Store store )
            {
                Debug.Log($"#####> Store.Count is {store.Count}");
            }
        }
    }

    public static void SetStoreData( string dataName, object value )
    {
        FieldBinding binding = storeDataFieldTable[dataName];
        binding.field.SetValue(binding.obj, value);
        Dispatch("Store." + dataName, value );
    }

    public static T GetStoreData<T>( string dataName )
    {
        FieldBinding binding = storeDataFieldTable[dataName];
        return (T)binding.field.GetValue(binding.obj);
    }

    public static void SaveDataStore( string path )
    {
        SavedData data = new SavedData();
        data.stores = storeDataObjectList;

        string json = new JsonPersistor().ToJson( data );
        System.IO.File.WriteAllText( path, json );
    }

    public static void LoadDataStore( string path )
    {
        if ( !System.IO.File.Exists( path ) )
            return;

        string json = System.IO.File.ReadAllText( path );

        JsonLoader loader = new JsonLoader();
        SavedData newData = (SavedData) loader.Decode( json );
        storeDataObjectList = newData.stores;

        storeDataFieldTable.Clear();

        // Reload the data bindings
        foreach( object storeData in storeDataObjectList )
        {
            foreach( FieldInfo info in storeData.GetType().GetFields())
                storeDataFieldTable.Add( info.Name, new FieldBinding(storeData,info));
        }
    }


    private void ScanRegisterAndInject()
    {
        // Scan object heirarchy for Inject requests
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach( GameObject rootObject in rootObjects )
        {
            Component[] rootComponents = rootObject.GetComponents<Component>();
            Component[] childComponents =rootObject.GetComponentsInChildren<Component>();

            ScanAndInjectComponents( rootComponents );
            ScanAndInjectComponents( childComponents );

            RegisterComponents( rootComponents );
            RegisterComponents( childComponents );
        }
    }

    public static void Dispatch(string eventName, object message )
    {        
        if ( !subscriberTable.ContainsKey( eventName) )
            return;

        Debug.Log($"Dispatch() event={eventName} subs={subscriberTable[ eventName ].Count}");        
        object[] args = { message };

        foreach( Subscription sub in subscriberTable[ eventName ] )
        {
            try 
            {
                sub.del.DynamicInvoke( args );
            }
            catch( ArgumentException ex )
            {
                Debug.LogError($"{ex.ParamName}: Dispatched message of type {message.GetType().Name} incorrect");
            }
        }
    }

    public static void Register( object obj )
    {
        // Scan object for subscribers
        Type t = obj.GetType();
        foreach( MethodInfo info in t.GetMethods() )
        {
            
            if( !info.IsDefined( typeof( SubscriberAttribute ) ) )
                continue;

            SubscriberAttribute attr = info.GetCustomAttribute<SubscriberAttribute>();

            if( !subscriberTable.ContainsKey( attr.GetName() ) )
            {
                Debug.Log("Adding empty list to table");
                subscriberTable.Add( attr.GetName(), new List<Subscription>());
            }
            
            Delegate del = CreateDelegate( obj, info );
            subscriberTable[attr.GetName()].Add( new Subscription( del, obj) );
        }
    }

    public static void Unregister( object obj )
    {
        List<string> keys = subscriberTable.Keys.ToList();
        foreach( string key in keys )
        {
            subscriberTable[key] = subscriberTable[key].Where( sub => sub.subscriber != obj ).ToList();
        }
    }

    private static Delegate CreateDelegate( object obj, MethodInfo info )
    {
        ParameterExpression[] parameters = info.GetParameters().Select( p => Expression.Parameter(p.ParameterType, p.Name)).ToArray();
        MethodCallExpression call = Expression.Call( Expression.Constant( obj ), info, parameters );
        return Expression.Lambda( call, parameters ).Compile();
    }

    public static T GetInstance<T>()
    {
        Type t = typeof( T );

        if(!injectablesTable.ContainsKey( t) )
            throw new Exception($"Yokai: Unknown Injectable {t}");

        return (T) Convert.ChangeType(injectablesTable[ t ], t );
    }

    void RegisterComponents( Component[] components )
    {
        foreach( Component component in components )
            Register( component );
    }

    void ScanAndInjectComponents( Component[] components )
    {        
        foreach( Component component in components )
        {
            Type t = component.GetType();
            FieldInfo[] fields = t.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );

            foreach( FieldInfo info in fields )
            {
                if( !info.IsDefined( typeof( InjectAttribute) ) )
                    continue;

                if( info.GetValue( component ) == null )
                {
                    object injectable = injectablesTable[info.FieldType];                    
                    info.SetValue( component, injectable );
                }
            }

        }
    }

    public static void ChangeState( ExecutionState newState )
    {
        if( !newState.GetEnterStates().Contains( currentState.GetType() ) )
        {
            Debug.LogError($"ERROR: {currentState.GetType().Name} is not a valid enter state for {newState.GetType().Name}");
            return;
        }

        if( !currentState.GetExitStates().Contains( newState.GetType() ) )
        {
            Debug.LogError($"ERROR: {newState.GetType().Name} is not a valid exit state for {currentState.GetType().Name}");
            return;
        }

        currentState.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }
}
