using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SharpJson;
using System.Reflection;
using System.Runtime.Serialization;

public class JsonLoader
{

    public object Decode( string json )
    {
        JsonDecoder decoder = new JsonDecoder();
        object decodedObj = decoder.Decode( json );
        
        Dictionary<string,object> dict = (Dictionary<string,object>) decodedObj;
        return LoadObject( dict );
    }

    public object LoadObject( Dictionary<string,object> dict )
    {
        string className = (string) dict["$className"];
        Type t = Assembly.GetExecutingAssembly().GetType( className );
        object obj = FormatterServices.GetUninitializedObject( t );

        foreach( string key in dict.Keys )
        {
            if( key == "$className")
                continue;

            FieldInfo field = t.GetField( key );
            object val = dict[key];

            if( val == null )
            {
                field.SetValue( obj, null );
            }
            else if( val is string )
            {
                field.SetValue( obj, val );
            }
            else if ( typeof(IEnumerable).IsAssignableFrom( val.GetType() ) )
            {
                Type elementType = field.FieldType.GetGenericArguments()[0];
                Type listType = typeof(List<>).MakeGenericType( elementType );
                var genericList = Activator.CreateInstance( listType );

                // Find the Add method for the list
                MethodInfo method = listType.GetMethod("Add");
                IEnumerable iEnum = (IEnumerable) val;
                IEnumerator enumerator = iEnum.GetEnumerator();
                enumerator.MoveNext();
                object enumObj = enumerator.Current;

                if( enumObj is Dictionary<string,object> dItem )
                {
                    foreach( var item in iEnum )
                    {
                        object itemObject = LoadObject( dItem );
                        method.Invoke( genericList, new object[]{ itemObject } );
                    }
                }
                else
                {
                    foreach( var item in iEnum )
                        method.Invoke( genericList, new object[]{ item } );
                }

                field.SetValue( obj, genericList );
            }
            else if ( val is Dictionary< string, object > d )
            {
                 object subVal = LoadObject( d );
                 field.SetValue( obj, subVal );
            }
            else if ( val is Double doubleValue )
            {
                if( field.FieldType == typeof(Double) )
                    field.SetValue( obj, val );
                else if ( field.FieldType == typeof(Int16) )
                    field.SetValue( obj, Convert.ToInt16( val ) );
                else if ( field.FieldType == typeof(Int32) )
                    field.SetValue( obj, Convert.ToInt32( val ) );
                else if ( field.FieldType == typeof(Int64) )
                    field.SetValue( obj, Convert.ToInt64( val ) );
            }
            else
            {
                field.SetValue( obj, val );
            }
        }

        return obj;
    }

}