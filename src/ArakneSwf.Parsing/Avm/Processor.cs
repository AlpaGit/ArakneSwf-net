using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using ArakneSwf.Parsing.Parser.Structure.Action;
using ValueType = ArakneSwf.Parsing.Parser.Structure.Action.ValueType;

namespace ArakneSwf.Parsing.Avm;

/// <summary>
/// Execute the parsed AVM bytecode.
/// This class is stateless, so the state must be passed as argument.
/// </summary>
public sealed class Processor
{
    private readonly bool _allowFunctionCall;

    /// <param name="allowFunctionCall">
    /// Allow to call methods or functions. If false, the processor will always
    /// push null for method/function calls, without executing them.
    /// </param>
    public Processor(bool allowFunctionCall = true)
    {
        _allowFunctionCall = allowFunctionCall;
    }

    /// <summary>
    /// Run the given actions and return the final state.
    /// </summary>
    public State Run(IList<ActionRecord> actions, State? state = null)
    {
        state ??= new State();

        foreach (var action in actions)
        {
            Execute(state, action);
        }

        return state;
    }

    /// <summary>
    /// Execute a single instruction.
    /// </summary>
    public void Execute(State state, ActionRecord action)
    {
        switch (action.Opcode)
        {
            case Opcode.ActionConstantPool:
            {
                var pool = GetConstantPool(action.Data);
                state.Constants.Clear();
                state.Constants.AddRange(pool);
                break;
            }

            case Opcode.ActionPush:
            {
                foreach (var v in ToDotNetValues(state, GetValues(action.Data)))
                    state.Stack.Add(v);
                break;
            }

            case Opcode.ActionSetVariable:
                SetVariable(state);
                break;

            case Opcode.ActionGetVariable:
                GetVariable(state);
                break;

            case Opcode.ActionGetMember:
                GetMember(state);
                break;

            case Opcode.ActionCallMethod:
                CallMethod(state);
                break;

            case Opcode.ActionPop:
                _ = Pop(state.Stack);
                break;

            case Opcode.ActionNewObject:
                NewObject(state);
                break;

            case Opcode.ActionInitObject:
                InitObject(state);
                break;

            case Opcode.ActionInitArray:
                InitArray(state);
                break;

            case Opcode.ActionSetMember:
                SetMember(state);
                break;

            case Opcode.ActionToString:
                ToStringTop(state);
                break;

            case Opcode.ActionToNumber:
                ToNumberTop(state);
                break;

            case Opcode.ActionCallFunction:
                CallFunction(state);
                break;

            case Opcode.Null:
                break;

            default:
                throw new Exception(
                    $"Unknown action: {action.Opcode} | ActionData={SafeJson(action.Data)} | Stack={SafeJson(state.Stack)}");
        }
    }

    // ---------- Helpers mirroring PHP behavior ----------

    private static IEnumerable<string> GetConstantPool(object? data)
    {
        if (data is IEnumerable<string> s) return s;
        if (data is string one) return [one];
        return [];
    }

    private static IEnumerable<Value> GetValues(object? data)
    {
        if (data is IEnumerable<Value> vs) return vs;
        if (data is Value v) return [v];
        return [];
    }

    /// <summary>
    /// Convert ActionScript Value -> .NET object, using state's constant pool when needed.
    /// </summary>
    private static IEnumerable<object?> ToDotNetValues(State state, IEnumerable<Value> values)
    {
        foreach (var value in values)
        {
            switch (value.Type)
            {
                case ValueType.Constant8:
                case ValueType.Constant16:
                    yield return state.Constants[(int)(value.Data ?? 0)];
                    break;

                // Register not implemented in PHP either ("@todo")
                default:
                    yield return value.Data;
                    break;
            }
        }
    }

    private static object? Pop(List<object?> list)
    {
        if (list.Count == 0) throw new InvalidOperationException("Stack underflow.");
        var idx = list.Count - 1;
        var v = list[idx];
        list.RemoveAt(idx);
        return v;
    }

    private static string SafeJson(object? o)
    {
        try
        {
            return JsonSerializer.Serialize(o);
        }
        catch
        {
            return o?.ToString() ?? "null";
        }
    }

    // ---------- Opcode handlers ----------

    private void SetVariable(State state)
    {
        var value = Pop(state.Stack);
        var name = Pop(state.Stack);
        state.Variables[Convert.ToString(name, CultureInfo.InvariantCulture) ?? ""] = value;
    }

    private void GetVariable(State state)
    {
        if (state.Stack.Count == 0) throw new InvalidOperationException("Stack underflow.");
        var idx = state.Stack.Count - 1;
        var rawKey = state.Stack[idx];
        var key = Convert.ToString(rawKey, CultureInfo.InvariantCulture) ?? "";
        state.Variables.TryGetValue(key, out var val);
        state.Stack[idx] = val;
    }

    private void GetMember(State state)
    {
        var propertyName = Pop(state.Stack);
        var scriptObject = Pop(state.Stack);

        if (scriptObject is null)
        {
            state.Stack.Add(null);
            return;
        }

        // Integer index?
        if (TryAsInt(propertyName, out var index))
        {
            if (scriptObject is IList list && index >= 0 && index < list.Count)
            {
                state.Stack.Add(list[index]);
                return;
            }
        }

        // String key?
        var name = Convert.ToString(propertyName, CultureInfo.InvariantCulture);

        if (name is not null)
        {
            // IDictionary<string, object?>
            if (scriptObject is IDictionary<string, object?> dict)
            {
                dict.TryGetValue(name, out var val);
                state.Stack.Add(val);
                return;
            }

            // Reflection property/field
            var t = scriptObject.GetType();
            var prop = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (prop != null)
            {
                state.Stack.Add(prop.GetValue(scriptObject));
                return;
            }

            var field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field != null)
            {
                state.Stack.Add(field.GetValue(scriptObject));
                return;
            }
        }

        state.Stack.Add(null);
    }

    private void CallMethod(State state)
    {
        var methodName = Convert.ToString(Pop(state.Stack), CultureInfo.InvariantCulture) ?? "";
        var scriptObject = Pop(state.Stack);
        var argumentCount = Convert.ToInt32(Pop(state.Stack), CultureInfo.InvariantCulture);
        var args = argumentCount > 0
            ? state.Stack.GetRange(state.Stack.Count - argumentCount, argumentCount)
            : [];

        if (argumentCount > 0)
            state.Stack.RemoveRange(state.Stack.Count - argumentCount, argumentCount);

        // Arguments passed in reverse order (stack), restore call order
        args.Reverse();

        if (!_allowFunctionCall || scriptObject is null)
        {
            state.Stack.Add(null);
            return;
        }

        // Reflection call
        var t = scriptObject.GetType();
        var candidate = t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                         .FirstOrDefault(m => string.Equals(m.Name, methodName, StringComparison.Ordinal) &&
                                              m.GetParameters().Length == args.Count);

        if (candidate != null)
        {
            var result = candidate.Invoke(scriptObject, args.ToArray());
            state.Stack.Add(result);
            return;
        }

        // If the object behaves like a dictionary with a Delegate value
        if (scriptObject is IDictionary<string, object?> dict &&
            dict.TryGetValue(methodName, out var maybeDelegate) &&
            maybeDelegate is Delegate del)
        {
            var result = del.DynamicInvoke(args.ToArray());
            state.Stack.Add(result);
            return;
        }

        state.Stack.Add(null);
    }

    private void NewObject(State state)
    {
        var type = Convert.ToString(Pop(state.Stack), CultureInfo.InvariantCulture) ?? "";
        var argumentCount = Convert.ToInt32(Pop(state.Stack), CultureInfo.InvariantCulture);

        var args = argumentCount > 0
            ? state.Stack.GetRange(state.Stack.Count - argumentCount, argumentCount)
            : [];

        if (argumentCount > 0)
            state.Stack.RemoveRange(state.Stack.Count - argumentCount, argumentCount);

        // In PHP they reverse before constructing the array obj
        args.Reverse();

        object created = type switch
        {
            "Object" => new Dictionary<string, object?>(StringComparer.Ordinal),
            "Array"  => (object)new List<object?>(args),
            _        => throw new Exception("Unknown object type: " + type),
        };

        state.Stack.Add(created);
    }

    private void InitObject(State state)
    {
        var propertiesCount = Convert.ToInt32(Pop(state.Stack), CultureInfo.InvariantCulture);
        var raw = propertiesCount > 0
            ? state.Stack.GetRange(state.Stack.Count - propertiesCount * 2, propertiesCount * 2)
            : [];

        if (propertiesCount > 0)
            state.Stack.RemoveRange(state.Stack.Count - propertiesCount * 2, propertiesCount * 2);

        var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
        for (int i = 2 * propertiesCount - 2; i >= 0; i -= 2)
        {
            var key = Convert.ToString(raw[i], CultureInfo.InvariantCulture) ?? "";
            var val = raw[i + 1];
            dict[key] = val;
        }

        state.Stack.Add(dict);
    }

    private void InitArray(State state)
    {
        var size = Convert.ToInt32(Pop(state.Stack), CultureInfo.InvariantCulture);
        var values = size > 0
            ? state.Stack.GetRange(state.Stack.Count - size, size)
            : [];

        if (size > 0)
            state.Stack.RemoveRange(state.Stack.Count - size, size);

        // PHP returns a plain array. We use List<object?> for similar semantics.
        state.Stack.Add(values);
    }

    private void SetMember(State state)
    {
        var value = Pop(state.Stack);
        var propertyName = Pop(state.Stack);
        var scriptObject = Pop(state.Stack);

        if (scriptObject is null) return;

        if (TryAsInt(propertyName, out var idx) && scriptObject is IList list)
        {
            EnsureListCapacity(list, idx);
            list[idx] = value;
            return;
        }

        var name = Convert.ToString(propertyName, CultureInfo.InvariantCulture) ?? "";

        if (scriptObject is IDictionary<string, object?> dict)
        {
            dict[name] = value;
            return;
        }

        var t = scriptObject.GetType();
        var prop = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(scriptObject, value);
            return;
        }

        var field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public);
        if (field != null)
        {
            field.SetValue(scriptObject, value);
            return;
        }

        // If nothing matched, do nothing (PHP silently sets dynamic property).
    }

    private void ToStringTop(State state)
    {
        if (state.Stack.Count == 0) throw new InvalidOperationException("Stack underflow.");
        var idx = state.Stack.Count - 1;
        state.Stack[idx] = (state.Stack[idx] is null)
            ? ""
            : Convert.ToString(state.Stack[idx], CultureInfo.InvariantCulture);
    }

    private void ToNumberTop(State state)
    {
        if (state.Stack.Count == 0) throw new InvalidOperationException("Stack underflow.");
        var idx = state.Stack.Count - 1;
        state.Stack[idx] = ToDoubleLikePhp(state.Stack[idx]);
    }

    private void CallFunction(State state)
    {
        var functionName = Convert.ToString(Pop(state.Stack), CultureInfo.InvariantCulture) ?? "";
        var argumentCount = Convert.ToInt32(Pop(state.Stack), CultureInfo.InvariantCulture);

        var args = argumentCount > 0
            ? state.Stack.GetRange(state.Stack.Count - argumentCount, argumentCount)
            : [];

        if (argumentCount > 0)
            state.Stack.RemoveRange(state.Stack.Count - argumentCount, argumentCount);

        // Restore call order
        args.Reverse();

        object? result = functionName switch
        {
            "Boolean" => ToBooleanLikePhp(args.ElementAtOrDefault(0)),
            "String" => (args.ElementAtOrDefault(0) is null)
                ? ""
                : Convert.ToString(args[0], CultureInfo.InvariantCulture),
            "Number" => ToDoubleLikePhp(args.ElementAtOrDefault(0)),
            _        => CallCustomFunction(state, functionName, args),
        };

        state.Stack.Add(result);
    }

    private object? CallCustomFunction(State state, string functionName, IReadOnlyList<object?> args)
    {
        if (!_allowFunctionCall) return null;

        if (!state.Functions.TryGetValue(functionName, out var del))
            throw new Exception("Unknown function: " + functionName);

        return del.DynamicInvoke(args.ToArray());
    }

    // ---------- Small utilities ----------

    private static bool TryAsInt(object? o, out int value)
    {
        switch (o)
        {
            case int i:
                value = i;
                return true;
            case long l when l >= int.MinValue && l <= int.MaxValue:
                value = (int)l;
                return true;
            case float f when Math.Abs(f - Math.Truncate(f)) < float.Epsilon:
                value = (int)f;
                return true;
            case double d when Math.Abs(d - Math.Truncate(d)) < double.Epsilon:
                value = (int)d;
                return true;
            case string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i2):
                value = i2;
                return true;
            default:
                value = default;
                return false;
        }
    }

    private static void EnsureListCapacity(IList list, int index)
    {
        if (list is List<object?> generic)
        {
            while (generic.Count <= index) generic.Add(null);
        }
        else
        {
            // Fallback for non-generic IList: append nulls until index is valid
            while (list.Count <= index) list.Add(null);
        }
    }

    private static bool ToBooleanLikePhp(object? x)
    {
        return x switch
        {
            null       => false,
            bool b     => b,
            string s   => s.Length != 0 && s != "0",
            sbyte sb   => sb != 0,
            byte b     => b != 0,
            short sh   => sh != 0,
            ushort ush => ush != 0,
            int i      => i != 0,
            uint ui    => ui != 0,
            long l     => l != 0,
            ulong ul   => ul != 0,
            float f    => Math.Abs(f) > float.Epsilon,
            double d   => Math.Abs(d) > double.Epsilon,
            decimal m  => m != 0m,
            _          => true
        };
    }

    private static double ToDoubleLikePhp(object? x)
    {
        try
        {
            return x switch
            {
                null => 0.0,
                bool b => b ? 1.0 : 0.0,
                string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) => d,
                string _ => 0.0,
                IConvertible c => c.ToDouble(CultureInfo.InvariantCulture),
                _ => 0.0
            };
        }
        catch
        {
            return 0.0;
        }
    }
}