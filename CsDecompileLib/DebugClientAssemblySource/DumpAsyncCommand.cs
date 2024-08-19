using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Diagnostics.Runtime;

namespace CsDecompileLib.DebugClientAssemblySource;

internal static class Utilities
{
    internal static ClrObject TryGetObjectField(this ClrObject clrObject, string fieldName)
    {
        if (!clrObject.IsNull)
        {
            ClrInstanceField? field = clrObject.Type?.GetFieldByName(fieldName);
            if (field is object && field.IsObjectReference)
            {
                return field.ReadObject(clrObject.Address, interior: false);
            }
        }

        return default(ClrObject);
    }

    internal static ClrValueType? TryGetValueClassField(this ClrObject clrObject, string fieldName)
    {
        if (!clrObject.IsNull)
        {
            ClrInstanceField? field = clrObject.Type?.GetFieldByName(fieldName);
            if (field?.Type is object && field.Type.IsValueType)
            {
                // System.Console.WriteLine("{0} {1:x} Field {2} {3} {4} {5}", clrObject.Type.Name, clrObject.Address, fieldName, field.Type.Name, field.Type.IsValueType, field.Type.IsRuntimeType);
                return clrObject.ReadValueTypeField(fieldName);
            }
        }

        return null;
    }

    internal static ClrObject TryGetObjectField(this ClrValueType? clrObject, string fieldName)
    {
        if (clrObject is object)
        {
            ClrInstanceField? field = clrObject.Value.Type?.GetFieldByName(fieldName);
            if (field is object && field.IsObjectReference)
            {
                return clrObject.Value.ReadObjectField(fieldName);
            }
        }

        return default(ClrObject);
    }

    internal static ClrValueType? TryGetValueClassField(this ClrValueType? clrObject, string fieldName)
    {
        if (clrObject.HasValue)
        {
            ClrInstanceField? field = clrObject.Value.Type?.GetFieldByName(fieldName);
            if (field is object && field.IsValueType)
            {
                return clrObject.Value.ReadValueTypeField(fieldName);
            }
        }

        return null;
    }

    internal static IEnumerable<ClrObject> GetObjectsOfType(ClrHeap heap, string typeName)
    {
        return heap.EnumerateObjects()
            .Where(obj => string.Equals(obj.Type?.Name, typeName, StringComparison.Ordinal));
    }
    
    public static ClrObject GetObject(this ClrHeap _, ulong objRef, ClrType type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return new(objRef, type);
    }
}
