using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

public class JsonPersistor
{
    public string ToJson( object obj )
    {
        if( !IsStoreData( obj ) )
            return "";

        StringBuilder sb = new StringBuilder();
        sb.Append("{").Append( FieldsToJson( obj ) ).Append("}");

        return sb.ToString();
    }

    private string ListToJson( IEnumerable iEnum )
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");

        int count = 0;
        foreach( var item in iEnum )
        {
            if (count > 0 )
                sb.Append(",");
            
            sb.Append( ToJson( item ) );
        }
        
        sb.Append("]");
        return sb.ToString();
    }

    private string FieldsToJson( object obj )
    {
        StringBuilder sb = new StringBuilder();
        Type objectType = obj.GetType();

        sb.Append("\"$className\":\"").Append( obj.GetType().FullName ).Append("\"");

        foreach( FieldInfo field in objectType.GetFields() )
        {
                 sb.Append(",");

            sb.Append("\"").Append( field.Name ).Append("\":");

            if( IsNull( field, obj ) )
            {
                sb.Append("null");
            }
            else if( field.FieldType == typeof(string))
            {
                sb.Append("\"").Append( field.GetValue( obj ) ).Append("\"");
            }
            else if( field.FieldType == typeof(bool))
            {
                string boolString = ((bool)field.GetValue( obj )) ? "true" : "false";
                sb.Append( boolString );
            }
            else if( IsNumeric( field.FieldType ) )
            {
                sb.Append( field.GetValue( obj ) );
            }
            else if ( typeof(IEnumerable).IsAssignableFrom( field.FieldType ) )
            {
                sb.Append( ListToJson( (IEnumerable) field.GetValue( obj ) ) );
            }
            else if ( field.FieldType.IsClass )
            {
                sb.Append( ToJson( field.GetValue( obj ) ) );
            }
        }

        return sb.ToString();
    }

    private bool IsStoreData( object obj )
    {
        Type t = obj.GetType();
        return t.IsDefined( typeof( System.SerializableAttribute ) );
    }

    private bool IsNumeric(Type t )
    {
        switch( Type.GetTypeCode( t ) )
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;

            default:
                return false;
        }
    }

    private bool IsNull( FieldInfo field, object obj )
    {
        return field.GetValue( obj ) == null;
    }
}