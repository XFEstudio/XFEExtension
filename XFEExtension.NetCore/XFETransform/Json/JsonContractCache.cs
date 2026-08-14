using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace XFEExtension.NetCore.XFETransform.Json;

internal static class JsonContractCache
{
    private static readonly ConcurrentDictionary<ContractKey, JsonObjectContract> Contracts = new();

    public static JsonObjectContract Get(Type type, JsonSettings settings) =>
        Contracts.GetOrAdd(new(type, settings.IncludeFields, settings.PropertyNamingPolicy), static key => Build(key));

    public static string ApplyNamingPolicy(string name, XFEJsonPropertyNamingPolicy policy)
    {
        if (policy != XFEJsonPropertyNamingPolicy.CamelCase || name.Length == 0 || !char.IsUpper(name[0]))
            return name;
        var characters = name.ToCharArray();
        for (var index = 0; index < characters.Length; index++)
        {
            if (index == 1 && !char.IsUpper(characters[index]))
                break;
            var hasNext = index + 1 < characters.Length;
            if (index > 0 && hasNext && !char.IsUpper(characters[index + 1]))
                break;
            characters[index] = char.ToLowerInvariant(characters[index]);
        }
        return new string(characters);
    }

    private static JsonObjectContract Build(ContractKey key)
    {
        var members = new List<JsonMemberContract>();
        var memberNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in key.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.GetIndexParameters().Length != 0 || property.GetMethod is not { IsPublic: true })
                continue;
            var name = ApplyNamingPolicy(property.Name, key.NamingPolicy);
            if (!memberNames.Add(name))
                continue;
            members.Add(new(
                name,
                property.PropertyType,
                CreateGetter(property),
                property.SetMethod is { IsPublic: true } ? CreateSetter(property) : null));
        }

        if (key.IncludeFields)
        {
            foreach (var field in key.Type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var name = ApplyNamingPolicy(field.Name, key.NamingPolicy);
                if (!memberNames.Add(name))
                    continue;
                members.Add(new(
                    name,
                    field.FieldType,
                    CreateGetter(field),
                    field.IsInitOnly ? null : CreateSetter(field)));
            }
        }

        Func<object>? factory = null;
        if (key.Type.IsValueType)
        {
            factory = () => Activator.CreateInstance(key.Type)!;
        }
        else if (!key.Type.IsAbstract && key.Type.GetConstructor(Type.EmptyTypes) is { } constructor)
        {
            factory = CreateFactory(constructor);
        }

        return new(key.Type, members, factory);
    }

    private static Func<object, object?> CreateGetter(PropertyInfo property)
    {
        if (RuntimeFeature.IsDynamicCodeSupported && !property.DeclaringType!.IsValueType)
        {
            try
            {
                var instance = Expression.Parameter(typeof(object), "instance");
                var access = Expression.Property(Expression.Convert(instance, property.DeclaringType), property);
                return Expression.Lambda<Func<object, object?>>(Expression.Convert(access, typeof(object)), instance).Compile();
            }
            catch (Exception)
            {
                // Reflection fallback below.
            }
        }
        return property.GetValue;
    }

    private static Func<object, object?> CreateGetter(FieldInfo field)
    {
        if (RuntimeFeature.IsDynamicCodeSupported && !field.DeclaringType!.IsValueType)
        {
            try
            {
                var instance = Expression.Parameter(typeof(object), "instance");
                var access = Expression.Field(Expression.Convert(instance, field.DeclaringType), field);
                return Expression.Lambda<Func<object, object?>>(Expression.Convert(access, typeof(object)), instance).Compile();
            }
            catch (Exception)
            {
                // Reflection fallback below.
            }
        }
        return field.GetValue;
    }

    private static Action<object, object?> CreateSetter(PropertyInfo property)
    {
        if (RuntimeFeature.IsDynamicCodeSupported && !property.DeclaringType!.IsValueType)
        {
            try
            {
                var instance = Expression.Parameter(typeof(object), "instance");
                var value = Expression.Parameter(typeof(object), "value");
                var assign = Expression.Assign(
                    Expression.Property(Expression.Convert(instance, property.DeclaringType), property),
                    Expression.Convert(value, property.PropertyType));
                return Expression.Lambda<Action<object, object?>>(assign, instance, value).Compile();
            }
            catch (Exception)
            {
                // Reflection fallback below.
            }
        }
        return property.SetValue;
    }

    private static Action<object, object?> CreateSetter(FieldInfo field)
    {
        if (RuntimeFeature.IsDynamicCodeSupported && !field.DeclaringType!.IsValueType)
        {
            try
            {
                var instance = Expression.Parameter(typeof(object), "instance");
                var value = Expression.Parameter(typeof(object), "value");
                var assign = Expression.Assign(
                    Expression.Field(Expression.Convert(instance, field.DeclaringType), field),
                    Expression.Convert(value, field.FieldType));
                return Expression.Lambda<Action<object, object?>>(assign, instance, value).Compile();
            }
            catch (Exception)
            {
                // Reflection fallback below.
            }
        }
        return field.SetValue;
    }

    private static Func<object> CreateFactory(ConstructorInfo constructor)
    {
        if (RuntimeFeature.IsDynamicCodeSupported)
        {
            try
            {
                return Expression.Lambda<Func<object>>(Expression.Convert(Expression.New(constructor), typeof(object))).Compile();
            }
            catch (Exception)
            {
                // Reflection fallback below.
            }
        }
        return () => constructor.Invoke(null);
    }

    private readonly record struct ContractKey(Type Type, bool IncludeFields, XFEJsonPropertyNamingPolicy NamingPolicy);
}

internal sealed class JsonObjectContract
{
    private readonly Dictionary<string, JsonMemberContract> _ordinalMembers;
    private readonly Dictionary<string, JsonMemberContract> _ignoreCaseMembers;

    public JsonObjectContract(Type type, IReadOnlyList<JsonMemberContract> members, Func<object>? factory)
    {
        Type = type;
        Members = members;
        Factory = factory;
        _ordinalMembers = new(StringComparer.Ordinal);
        _ignoreCaseMembers = new(StringComparer.OrdinalIgnoreCase);
        foreach (var member in members)
        {
            _ordinalMembers.TryAdd(member.Name, member);
            _ignoreCaseMembers.TryAdd(member.Name, member);
        }
    }

    public Type Type { get; }
    public IReadOnlyList<JsonMemberContract> Members { get; }
    public Func<object>? Factory { get; }

    public bool TryGetMember(string name, bool ignoreCase, out JsonMemberContract member) =>
        (ignoreCase ? _ignoreCaseMembers : _ordinalMembers).TryGetValue(name, out member!);
}

internal sealed record JsonMemberContract(
    string Name,
    Type MemberType,
    Func<object, object?> Getter,
    Action<object, object?>? Setter);
