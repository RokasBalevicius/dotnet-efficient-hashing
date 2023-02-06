using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Hashing.HashCalculator;

public sealed class CustomHashCalculatorBuilder
{
    public const long HashSeed = 17; // must be a primary number

    private const int MaxDepth = 5;

    private readonly ConstantExpression _hashSeed = Expression.Constant(HashSeed, typeof(long));

    public Func<T, long> BuildHashCalculator<T>()
    {
        var searchFormParam = Expression.Parameter(typeof(T));

        var thisConstant = Expression.Constant(this);
        var combineMethod = GetType().GetMethod(nameof(CombineHash))!;

        var mainAccumulator = Expression.Variable(typeof(long), "mainAccumulator");

        var setUpAccumulatorExpression = Expression.Assign(mainAccumulator, _hashSeed); // var accumulator = _hashSeed
        var hashExpressionTree = GetExpressionForObject(mainAccumulator, searchFormParam, typeof(T), thisConstant, combineMethod, 0);

        // last expression is effectively - return accumulator;
        var mainBlock = Expression.Block(new[] {mainAccumulator}, setUpAccumulatorExpression, hashExpressionTree, mainAccumulator);

        return Expression.Lambda<Func<T, long>>(mainBlock, searchFormParam).Compile();
    }

    private Expression GetExpressionForObject(
        ParameterExpression accumulator,
        Expression obj,
        Type objType,
        ConstantExpression thisConstant,
        MethodInfo combineMethod,
        int depth)
    {
        if (depth > MaxDepth)
        {
            return Expression.Throw(Expression.Constant(new Exception("object is nested to deep"), typeof(Exception)));
        }
        
        var nullExpression = Expression.Assign(accumulator, Expression.Call(thisConstant, combineMethod, accumulator, Expression.Constant((long)0, typeof(long))));

        if (IsDictionary(objType))
        {
            var expressionForDictionary = GetExpressionForDictionary(accumulator, obj, objType, thisConstant, combineMethod, depth);

            return WrapWithNullCheck(obj, objType, expressionForDictionary, nullExpression);
        }

        if (IsKvListType(objType))
        {
            var expressionForDictionary = GetExpressionForKvList(accumulator, obj, objType, thisConstant, combineMethod, depth);

            return WrapWithNullCheck(obj, objType, expressionForDictionary, nullExpression);
        }

        if (IsIteratable(objType))
        {
            var expressionForList = GetExpressionForList(accumulator, obj, objType, thisConstant, combineMethod, depth);

            return WrapWithNullCheck(obj, objType, expressionForList, nullExpression);
        }

        if (IsSimpleType(objType))
        {
            var expressionForPrimitiveType = AccumulateHashCode(accumulator, objType, obj, thisConstant, combineMethod);

            return WrapWithNullCheck(obj, objType, expressionForPrimitiveType, nullExpression);
        }

        var notNullExpression = GetExpressionForComplexObject(accumulator, obj, objType, thisConstant, combineMethod, depth);

        return WrapWithNullCheck(obj, objType, notNullExpression, nullExpression);
    }

    private Expression GetExpressionForComplexObject(
        ParameterExpression accumulator,
        Expression obj,
        Type objType,
        ConstantExpression thisConstant,
        MethodInfo combineMethod,
        int depth)
    {
        var exp = new List<Expression>();

        var objAccumulator = Expression.Variable(typeof(long), "complexAccumulator" + depth); // var complexAccumulator;
        var objectAccumulatorInit = Expression.Assign(objAccumulator, _hashSeed); // complexAccumulator = _hashSeed;
        exp.Add(objectAccumulatorInit);

        var properties = objType.GetProperties().Where(p => p.CanRead && p.CanWrite);
        foreach (var propertyInfo in properties)
        {
            if (propertyInfo.GetCustomAttribute(typeof(ExcludeFromHash)) is not null)
            {
                continue;
            }

            var getterMethodInfo = propertyInfo.GetGetMethod()!;
            var propertyType = getterMethodInfo.ReturnType;
            var getterCall = Expression.Call(obj, getterMethodInfo); //property.get()

            exp.Add(GetExpressionForObject(objAccumulator, getterCall, propertyType, thisConstant, combineMethod, depth + 1));
        }

        //accumulate into main
        var accumulateToMain = Expression.Call(thisConstant, combineMethod, accumulator, objAccumulator); // .CombineHash(accumulator, objAccumulator)
        var assignToMain = Expression.Assign(accumulator, accumulateToMain); // mainAccumulator = .CombineHash(accumulator, objAccumulator)

        exp.Add(assignToMain);

        return Expression.Block(new[] {objAccumulator}, exp);
    }

    private Expression GetExpressionForKvList(
        ParameterExpression mainAccumulator,
        Expression list,
        Type listType,
        ConstantExpression thisConstant,
        MethodInfo combineMethod,
        int depth)
    {
        var listKvType = listType.GetGenericArguments()[0];

        return GetExpressionForEnumerableKv(mainAccumulator, list, listKvType, thisConstant, combineMethod, depth);
    }

    private Expression GetExpressionForDictionary(
        ParameterExpression mainAccumulator,
        Expression list,
        Type listType,
        ConstantExpression thisConstant,
        MethodInfo combineMethod,
        int depth)
    {
        return GetExpressionForEnumerableKv(mainAccumulator, list, listType, thisConstant, combineMethod, depth);
    }

    private Expression GetExpressionForEnumerableKv(
        ParameterExpression mainAccumulator,
        Expression list,
        Type listKvType,
        ConstantExpression thisConstant,
        MethodInfo combineMethod,
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

        // load current into var
        // accumulate hash
        var loopAccumulator = Expression.Variable(typeof(long), "loopAccumulator");

        var loopCode = CalculateHashCodeForKeyValuePair(loopAccumulator, currentKvItem, kvType, thisConstant, combineMethod, depth);
        var loopInnerBlock = Expression.Block(new[] {currentKvItem}, assignToCurrent, loopCode);

        return BuildLoopBlock(moveNextCall, loopInnerBlock, thisConstant, combineMethod, mainAccumulator, loopAccumulator, enumerator, enumeratorAssign);
    }

    private Expression CalculateHashCodeForKeyValuePair(
        ParameterExpression accumulator,
        Expression kv,
        Type kvType,
        ConstantExpression thisConstant,
        MethodInfo combineMethod,
        int depth)
    {
        var keyGetterMethod = kvType.GetProperty("Key")!.GetGetMethod()!;
        var keyType = keyGetterMethod.ReturnType;
        var keyGetter = Expression.Call(kv, keyGetterMethod); // kv.Key.get();

        var valueGetterMethod = kvType.GetProperty("Value")!.GetGetMethod()!;
        var valueType = valueGetterMethod.ReturnType;
        var valueGetter = Expression.Call(kv, valueGetterMethod); // kv.Value.get();

        var accumulateKey = GetExpressionForObject(accumulator, keyGetter, keyType, thisConstant, combineMethod, depth + 1);
        var accumulateValue = GetExpressionForObject(accumulator, valueGetter, valueType, thisConstant, combineMethod, depth + 1);

        return Expression.Block(accumulateKey, accumulateValue);
    }

    private Expression GetExpressionForList(
        ParameterExpression mainAccumulator,
        Expression list,
        Type listType,
        ConstantExpression thisConstant,
        MethodInfo combineMethod,
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
        var loopAccumulator = Expression.Variable(typeof(long), "loopAccumulator");
        var loopCode = GetExpressionForObject(loopAccumulator, currentItem, listItemType, thisConstant, combineMethod, depth + 1);
        var loopInnerBlock = Expression.Block(new[] {currentItem}, assignToCurrent, loopCode);

        return BuildLoopBlock(moveNextCall, loopInnerBlock, thisConstant, combineMethod, mainAccumulator, loopAccumulator, enumerator, enumeratorAssign);
    }

    private Expression BuildLoopBlock(
        Expression moveNextCall,
        Expression loopInnerBlock,
        ConstantExpression thisConstant,
        MethodInfo combineMethod,
        ParameterExpression mainAccumulator,
        ParameterExpression loopAccumulator,
        ParameterExpression enumerator,
        BinaryExpression enumeratorAssign)
    {
        // add code to exit loop after enumeration is done.
        var breakLabel = Expression.Label("LoopBreak");
        var loopBreakExpression = Expression.IfThenElse(Expression.Equal(moveNextCall, Expression.Constant(true)), loopInnerBlock, Expression.Break(breakLabel));
        var loop = Expression.Loop(loopBreakExpression, breakLabel);

        // init loop accumulator
        var loopAccumulatorInit = Expression.Assign(loopAccumulator, _hashSeed);

        // accumulate into main
        var accumulateToMain = Expression.Call(thisConstant, combineMethod, mainAccumulator, loopAccumulator); // .CombineHash(mainAccumulator, loopAccumulator)
        var assignToMain = Expression.Assign(mainAccumulator, accumulateToMain); // mainAccumulator = .CombineHash(mainAccumulator, loopAccumulator)

        return Expression.Block(new[] {enumerator, loopAccumulator}, loopAccumulatorInit, enumeratorAssign, loop, assignToMain);
    }

    private Expression WrapWithNullCheck(Expression getterCall, Type returnType, Expression expressionToRunIfNotNull, Expression expressionToRunIfNull)
    {
        var isNullable = Nullable.GetUnderlyingType(returnType) is not null || !returnType.IsValueType;
        if (isNullable)
        {
            var checkIfNull = Expression.Equal(getterCall, Expression.Constant(null, typeof(object)));
            var ifThenElse = Expression.IfThenElse(checkIfNull, expressionToRunIfNull, expressionToRunIfNotNull); // if is not null ? X1 : X2

            return ifThenElse;
        }

        return expressionToRunIfNotNull;
    }

    private Expression AccumulateHashCode(ParameterExpression accumulator, Type returnType, Expression getterCall, ConstantExpression thisConstant, MethodInfo combineMethod)
    {
        Expression hashMethodCall;
        if (returnType == typeof(string))
        {
            var hashMethod = typeof(StringExtensions).GetMethod("GetDeterministicHashCode", BindingFlags.Static | BindingFlags.Public)!;
            hashMethodCall = Expression.Call(null, hashMethod, getterCall); // .GetDeterministicHashCode(.get())
        }
        else
        {
            var hashMethod = returnType.GetMethod(nameof(GetHashCode), Type.EmptyTypes)!;
            hashMethodCall = Expression.Call(getterCall, hashMethod); // .GetHashCode(.get())
        }

        var cast = Expression.Convert(hashMethodCall, typeof(long)); // cast to long

        var combineIfNotNull =
            Expression.Assign(accumulator, Expression.Call(thisConstant, combineMethod, accumulator, cast)); // acc = .CombineHash(accumulator, .GetHashCode(.get()))

        return combineIfNotNull;
    }

    private bool IsIteratable(Type objectType)
    {
        return objectType != typeof(string)
            && objectType.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>) && x.GenericTypeArguments.Length == 1);
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

    private bool IsKvListType(Type objectType)
    {
        if (!(IsIteratable(objectType) && objectType.IsGenericType))
        {
            return false;
        }

        var arg1Type = objectType.GetGenericArguments()[0];

        if (arg1Type.IsGenericType && arg1Type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// returns true if GetHashCode can be used on object directly and not on its components.
    /// </summary>
    /// <param name="objectType"></param>
    /// <returns></returns>
    private bool IsSimpleType(Type objectType)
    {
        return objectType.IsPrimitive || objectType.IsEnum || objectType.IsValueType || objectType == typeof(string);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public long CombineHash(long accumulator, long hash)
    {
        unchecked
        {
            accumulator = (accumulator * 397) ^ hash;
        }

        return accumulator;
    }
}