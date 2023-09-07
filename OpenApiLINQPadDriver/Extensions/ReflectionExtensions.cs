using System;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenApiLINQPadDriver.Extensions;

internal static class ReflectionExtensions
{
    public static void SetProperty<T, TP>(this T obj, string propertyName, TP value, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        where T : class
    {
        var type = obj.GetType();

        var propertyType = type.GetProperty(propertyName, bindingFlags)?.DeclaringType;
        var property = propertyType?.GetProperty(propertyName, bindingFlags);

        if (property == null)
            throw new InvalidOperationException(
                $"Cannot find property '{propertyName}' in object of type {typeof(T)}");

        if (!property.CanWrite)
        {
            obj.SetField($"<{propertyName}>k__BackingField", value);
            return;
        }

        property.SetValue(obj, value, null);
    }

    public static void SetField<T, TP>(this T obj, string fieldName, TP value, BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        where T : class
    {
        var type = obj.GetType();

        var field = type.GetField(fieldName, bindingFlags);

        if (field == null)
            throw new InvalidOperationException($"Cannot find field '{fieldName}' in object of type {typeof(T)}");

        field.SetValue(obj, value);
    }

    public static void SetProperty<T, TP>(this T obj, Expression<Func<T, TP>> action, TP value, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        where T : class
    {
        var propertyName = ((MemberExpression)action.Body).Member.Name;

        try
        {
            obj.SetProperty(propertyName, value, bindingFlags);
        }
        catch (InvalidOperationException)
        {
            obj.SetPropertyBackingField(propertyName, value);
        }
    }

    public static void SetPropertyBackingField<T, TP>(this T obj, string propertyName, TP value, BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        where T : class
        => obj.SetField($"<{propertyName}>k__BackingField", value, bindingFlags);
}
