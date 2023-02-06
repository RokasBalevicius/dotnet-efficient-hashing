using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using K4os.Hash.xxHash;
using ProtoBuf;
using Exception = System.Exception;

namespace Hashing.HashCalculator;

public class MethodsBag
{
    public MethodInfo SerializeString { get; set; }
    public MethodInfo SerializeInt { get; set; }
    public MethodInfo SerializeNullableInt { get; set; }
    public MethodInfo SerializeBool { get; set; }
    public MethodInfo SerializeNullableBool { get; set; }
    public MethodInfo AddTagToStream { get; set; }
}

public sealed class XXHash64HasherBuilder
{
    private const int MaxDepth = 5;

    public Func<T, XXH64.State, ulong> BuildSerialisationFunction<T>()
    {
        var objectParameter = Expression.Parameter(typeof(T));
        var streamParameter = Expression.Parameter(typeof(XXH64.State));

        var thisConstant = Expression.Constant(this);

        var methods = new MethodsBag
        {
            SerializeString = GetType().GetMethod(nameof(SerialiseString))!,
            SerializeBool = GetType().GetMethod(nameof(SerialiseBool))!,
            SerializeNullableBool = GetType().GetMethod(nameof(SerialiseNullableBool))!,
            SerializeInt = GetType().GetMethod(nameof(SerialiseInt))!,
            SerializeNullableInt = GetType().GetMethod(nameof(SerialiseNullableInt))!,
            AddTagToStream = GetType().GetMethod(nameof(AddTagToStream))!
        };

        var hashExpressionTree = GetExpressionForComplexObject(streamParameter, objectParameter, typeof(T), thisConstant, methods, 0);

        var returnHash = Expression.Call(thisConstant, GetType().GetMethod(nameof(ReturnHash))!, streamParameter);

        var mainBlock = Expression.Block(hashExpressionTree, returnHash);

        return Expression
            .Lambda<Func<T, XXH64.State, ulong>>(mainBlock, objectParameter, streamParameter)
            .Compile();
    }

    private Expression GetExpressionForObject(
        ParameterExpression stream,
        Expression obj,
        Type objType,
        ConstantExpression thisConstant,
        MethodsBag methods,
        int depth)
    {
        if (depth > MaxDepth)
        {
            return Expression.Throw(Expression.Constant(new Exception("object is nested to deep"), typeof(Exception)));
        }

        if (IsDictionary(objType))
        {
            var expressionForDictionary = GetExpressionForDictionary(stream, obj, objType, thisConstant, methods, depth);

            return DoIfNotNull(obj, objType, expressionForDictionary);
        }

        if (IsIteratable(objType))
        {
            var expressionForList = GetExpressionForList(stream, obj, objType, thisConstant, methods, depth);

            return DoIfNotNull(obj, objType, expressionForList);
        }

        if (IsSimpleType(objType))
        {
            var ex = AddToStream(stream, objType, obj, thisConstant, methods);

            return DoIfNotNull(obj, objType, ex);
        }

        var notNullExpression = GetExpressionForComplexObject(stream, obj, objType, thisConstant, methods, depth);

        return DoIfNotNull(obj, objType, notNullExpression);
    }

    private Expression GetExpressionForComplexObject(
        ParameterExpression stream,
        Expression obj,
        Type objType,
        ConstantExpression thisConstant,
        MethodsBag methods,
        int depth)
    {
        var exp = new List<Expression>();

        var properties = objType.GetProperties().Where(p => p.CanRead && p.CanWrite);
        foreach (var propertyInfo in properties)
        {
            var attr = propertyInfo.GetCustomAttribute(typeof(ProtoMemberAttribute));
            if (attr is null)
            {
                continue;
            }

            var tagByte = (byte) ((ProtoMemberAttribute) attr).Tag;
            var addTagExpression = GetAddTagToStream(stream, tagByte, methods, thisConstant);
            exp.Add(addTagExpression);

            var getterMethodInfo = propertyInfo.GetGetMethod()!;
            var propertyType = getterMethodInfo.ReturnType;
            var getterCall = Expression.Call(obj, getterMethodInfo); //property.get()

            exp.Add(GetExpressionForObject(stream, getterCall, propertyType, thisConstant, methods, depth + 1));
        }

        return Expression.Block(exp);
    }

    private Expression GetAddTagToStream(
        ParameterExpression stream, 
        byte tag, 
        MethodsBag bag,
        ConstantExpression thisConstant)
    {
        return Expression.Call(thisConstant, bag.AddTagToStream, stream, Expression.Constant(tag, typeof(byte)));
    }

    private Expression GetExpressionForDictionary(
        ParameterExpression stream,
        Expression list,
        Type listType,
        ConstantExpression thisConstant,
        MethodsBag methods,
        int depth)
    {
        return GetExpressionForEnumerableKv(stream, list, listType, thisConstant, methods, depth);
    }

    private Expression GetExpressionForEnumerableKv(
        ParameterExpression stream,
        Expression list,
        Type listKvType,
        ConstantExpression thisConstant,
        MethodsBag methods,
        int depth)
    {
        var keyItemType = listKvType.GetGenericArguments()[0];
        var valueItemType = listKvType.GetGenericArguments()[1];

        var kvType = typeof(KeyValuePair<,>).MakeGenericType(keyItemType, valueItemType);

        // build enumerator
        var enumerableType = typeof(IEnumerable<>).MakeGenericType(kvType);
        var enumeratorType = typeof(IEnumerator<>).MakeGenericType(kvType);
        var enumerator = Expression.Variable(enumeratorType, "enumerator");
        var getEnumeratorCall = Expression.Call(list, enumerableType.GetMethod("GetEnumerator")!);
        var enumeratorAssign = Expression.Assign(enumerator, getEnumeratorCall);

        // current item in loop.
        var currentKvItem = Expression.Variable(kvType, "current");

        // prepare enumeration methods
        var moveNextCall = Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext")!); // enumerator.MoveNext();
        var assignToCurrent = Expression.Assign(currentKvItem, Expression.Property(enumerator, "Current")); // current = enumerator.Current;

        var loopCode = CalculateHashCodeForKeyValuePair(stream, currentKvItem, kvType, thisConstant, methods, depth);
        var loopInnerBlock = Expression.Block(new[] {currentKvItem}, assignToCurrent, loopCode);
        var loopBlock = BuildLoopBlock(moveNextCall, loopInnerBlock, enumerator, enumeratorAssign);

        // will not work for every type, length vs count vs pure enumerable
        var count = listKvType.GetProperty("Count")!.GetGetMethod()!;
        var getCount = Expression.Call(list, count);
        var addCountToStream = AddToStream(stream, count.ReturnType, getCount, thisConstant, methods);

        return Expression.Block(new List<Expression> {addCountToStream, loopBlock});
    }

    private Expression CalculateHashCodeForKeyValuePair(
        ParameterExpression stream,
        Expression kv,
        Type kvType,
        ConstantExpression thisConstant,
        MethodsBag methods,
        int depth)
    {
        var keyGetterMethod = kvType.GetProperty("Key")!.GetGetMethod()!;
        var keyType = keyGetterMethod.ReturnType;
        var keyGetter = Expression.Call(kv, keyGetterMethod); // kv.Key.get();

        var valueGetterMethod = kvType.GetProperty("Value")!.GetGetMethod()!;
        var valueType = valueGetterMethod.ReturnType;
        var valueGetter = Expression.Call(kv, valueGetterMethod); // kv.Value.get();

        var accumulateKey = GetExpressionForObject(stream, keyGetter, keyType, thisConstant, methods, depth + 1);
        var accumulateValue = GetExpressionForObject(stream, valueGetter, valueType, thisConstant, methods, depth + 1);

        return Expression.Block(accumulateKey, accumulateValue);
    }

    private Expression GetExpressionForList(
        ParameterExpression stream,
        Expression list,
        Type listType,
        ConstantExpression thisConstant,
        MethodsBag methods,
        int depth)
    {
        var listItemType = (listType.IsGenericType ? listType.GetGenericArguments()[0] : listType.GetElementType())!;

        // build enumerator
        var enumerableType = typeof(IEnumerable<>).MakeGenericType(listItemType);
        var enumeratorType = typeof(IEnumerator<>).MakeGenericType(listItemType);
        var enumerator = Expression.Variable(enumeratorType, "enumerator");
        var getEnumeratorCall = Expression.Call(list, enumerableType.GetMethod("GetEnumerator")!);
        var enumeratorAssign = Expression.Assign(enumerator, getEnumeratorCall);

        // current item in loop.
        var currentItem = Expression.Variable(listItemType, "current");

        // prepare enumeration methods
        var moveNextCall = Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext")!); // enumerator.MoveNext();
        var assignToCurrent = Expression.Assign(currentItem, Expression.Property(enumerator, "Current")); // current = enumerator.Current;

        // load current into var
        // accumulate hash
        var loopCode = GetExpressionForObject(stream, currentItem, listItemType, thisConstant, methods, depth + 1);
        var loopInnerBlock = Expression.Block(new[] {currentItem}, assignToCurrent, loopCode);

        var loopBlock = BuildLoopBlock(moveNextCall, loopInnerBlock, enumerator, enumeratorAssign);

        // will not work for every type
        var count = listType.GetProperty("Count")!.GetGetMethod()!;
        var getCount = Expression.Call(list, count);
        var addCountToStream = AddToStream(stream, count.ReturnType, getCount, thisConstant, methods);

        return Expression.Block(new List<Expression> {addCountToStream, loopBlock});
    }

    private Expression BuildLoopBlock(
        Expression moveNextCall,
        Expression loopInnerBlock,
        ParameterExpression enumerator,
        BinaryExpression enumeratorAssign)
    {
        // add code to exit loop after enumeration is done.
        var breakLabel = Expression.Label("LoopBreak");

        var loopBreakExpression = Expression.IfThenElse(
            Expression.Equal(moveNextCall, Expression.Constant(true)),
            loopInnerBlock,
            Expression.Break(breakLabel));

        var loop = Expression.Loop(loopBreakExpression, breakLabel);

        return Expression.Block(new[] {enumerator}, enumeratorAssign, loop);
    }

    private Expression DoIfNotNull(Expression getterCall, Type returnType, Expression expressionToRunIfNotNull)
    {
        var isNullable = Nullable.GetUnderlyingType(returnType) is not null || !returnType.IsValueType;
        if (isNullable)
        {
            var checkIfNull = Expression.NotEqual(getterCall, Expression.Constant(null, typeof(object)));
            var ifThen = Expression.IfThen(checkIfNull, expressionToRunIfNotNull);

            return ifThen;
        }

        return expressionToRunIfNotNull;
    }

    private Expression AddToStream(
        ParameterExpression stream,
        Type returnType,
        Expression getterCall,
        ConstantExpression thisConstant,
        MethodsBag bag)
    {
        if (returnType == typeof(int))
        {
            return Expression.Call(thisConstant, bag.SerializeInt, stream, getterCall);
        }

        if (returnType == typeof(int?))
        {
            return Expression.Call(thisConstant, bag.SerializeNullableInt, stream, getterCall);
        }

        if (returnType == typeof(string))
        {
            return Expression.Call(thisConstant, bag.SerializeString, stream, getterCall);
        }

        if (returnType == typeof(bool))
        {
            return Expression.Call(thisConstant, bag.SerializeBool, stream, getterCall);
        }

        if (returnType == typeof(bool?))
        {
            return Expression.Call(thisConstant, bag.SerializeNullableBool, stream, getterCall);
        }

        throw new Exception($"Unsupported type {returnType.Name}");
    }

    private bool IsIteratable(Type objectType)
    {
        return objectType != typeof(string)
               && objectType.GetInterfaces().Any(x =>
                   x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                   x.GenericTypeArguments.Length == 1);
    }

    private bool IsDictionary(Type objectType)
    {
        if (!(objectType.IsGenericType && objectType.GetGenericArguments().Length == 2))
        {
            return false;
        }

        var keyItemType = objectType.GetGenericArguments()[0];
        var valueItemType = objectType.GetGenericArguments()[1];

        var kvType = typeof(KeyValuePair<,>).MakeGenericType(keyItemType, valueItemType);
        var enumerableType = typeof(IEnumerable<>).MakeGenericType(kvType);

        var isDictionary = objectType.GetInterfaces().Any(i => i == enumerableType);

        return isDictionary;
    }

    /// <summary>
    /// returns true if GetHashCode can be used on object directly and not on its components.
    /// </summary>
    /// <param name="objectType"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private bool IsSimpleType(Type objectType)
    {
        return objectType.IsPrimitive || objectType.IsEnum || objectType.IsValueType || objectType == typeof(string);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public unsafe void SerialiseInt(XXH64.State stream, int value)
    {
        XXH64.Update(ref stream, new ReadOnlySpan<byte>(&value, sizeof(int)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public unsafe void SerialiseNullableInt(XXH64.State stream, int? value)
    {
        if (value is not null)
        {
            var v = value.Value;
            XXH64.Update(ref stream, new ReadOnlySpan<byte>(&v, sizeof(int)));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public unsafe void SerialiseBool(XXH64.State stream, bool value)
    {
        var byteArray = stackalloc byte[] {value ? (byte) 0x1 : (byte) 0x0};
        XXH64.Update(ref stream, byteArray, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public unsafe void SerialiseNullableBool(XXH64.State stream, bool? value)
    {
        if (value is not null)
        {
            var byteArray = stackalloc byte[] {value.Value ? (byte) 0x1 : (byte) 0x0};
            XXH64.Update(ref stream, byteArray, 1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public unsafe void SerialiseString(XXH64.State stream, string value)
    {
        if (value.Length == 0)
        {
            return;
        }

        var l = value.Length;
        XXH64.Update(ref stream, new ReadOnlySpan<byte>(&l, sizeof(int)));

        fixed (char* str = value)
        {
            XXH64.Update(ref stream, str, sizeof(char) * value.Length);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public unsafe void AddTagToStream(XXH64.State stream, byte tag)
    {
        var byteArray = stackalloc byte[] {tag};

        XXH64.Update(ref stream, byteArray, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public ulong ReturnHash(XXH64.State state)
    {
        return XXH64.Digest(state);
    }
}