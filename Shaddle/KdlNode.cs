using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Shaddle.Serialize;
using Shaddle.Values;

namespace Shaddle;

public sealed class KdlNode(string name, string? type = null) : ISerializable
{
    public string Name { get; } = name;

    public string? Type { get; } = type;

    public IReadOnlyDictionary<string, KdlValue> Properties { get; init; } = new Dictionary<string, KdlValue>();

    public IReadOnlyCollection<KdlValue> Arguments { get; init; } = new List<KdlValue>();

    public KdlDocument? Children { get; init; }

    /// <summary>
    /// Checks for the presence of the property.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <returns><c>true</c> if a property with that name exists.</returns>
    public bool HasProperty(string name) => Properties.ContainsKey(name);

    /// <summary>
    /// Returns the property value if it was found, otherwise throws an exception. 
    /// If you need a more safe behavior, then look at <seealso cref="TryGetProperty"/>.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <returns>The <seealso cref="KdlValue"/> of the property.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public KdlValue GetProperty(string name)
    {
        if (TryGetProperty(name, out var prop))
            return prop;

        throw new KeyNotFoundException($"{name} key is not found");
    }

    /// <summary>
    /// Returns the property value if it was found.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">If the method returns <c>true</c>, then the output parameter has a property value; otherwise, it contains the value <c>null</c>.</param>
    /// <returns>If it returns <c>true</c>, then the property is found; otherwise it returns <c>false</c>.</returns>
    public bool TryGetProperty(string name, [NotNullWhen(true)] out KdlValue? value)
    {
        if (Properties.TryGetValue(name, out var v))
        {
            value = v;
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Checks for an argument based on the passed index.
    /// </summary>
    /// <param name="index">The index of the argument. It starts from 0.</param>
    /// <returns></returns>
    public bool HasArgument(int index)
    {
        if (CheckArgumentIndex(index))
            return false;

        var argument = Arguments.ElementAtOrDefault(index);
        if (argument is not null)
            return true;

        return false;
    }

    /// <summary>
    /// If the search for the argument by the passed index is completed successfully, 
    /// the value of the argument is returned. If the index goes beyond the array of 
    /// arguments, an <c>IndexOutOfRangeException</c> is thrown. A safer option is <seealso cref="TryGetArgument"/>.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>The value of the argument.</returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public KdlValue GetArgument(int index)
    {
        if (CheckArgumentIndex(index))
            throw new IndexOutOfRangeException($"value at index {index} is not found");

        return Arguments.ElementAt(index);
    }

    /// <summary>
    /// If it returns <c>true</c>, then the output parameter <paramref name="value"/> has the value of the argument; 
    /// otherwise it contains null.
    /// </summary>
    /// <param name="index">The index of the argument.</param>
    /// <param name="value">The value of the argument.</param>
    /// <returns><c>true</c> if the argument is successfully searched by the passed index.</returns>
    public bool TryGetArgument(int index, [NotNullWhen(true)] out KdlValue? value)
    {
        if (!CheckArgumentIndex(index))
        {
            value = Arguments.ElementAtOrDefault(index);
            return value is not null;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Checks if a node has child nodes and a specific child node by the specified name.
    /// </summary>
    /// <param name="name">The name of the child node.</param>
    public bool HasChild(string name) => Children is not null && Children.HasNode(name);

    /// <summary>
    /// Returns the child node by the passed name. 
    /// If the node has <c>null</c> instead of <seealso cref="KdlDocument"/>, 
    /// an exception is thrown <c>NullReferenceException</c>.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    /// <returns>The child node with the passed name.</returns>
    /// <exception cref="NullReferenceException"></exception>
    public KdlNode GetChild(string name)
    {
        if (Children is null)
            throw new NullReferenceException($"Children of {Name} node is null");
        else
            return Children.GetNode(name);
    }

    /// <summary>
    /// A more safe method of getting a child node by its name than <see cref="GetChild"/>.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    /// <param name="node">Child node.</param>
    public bool TryGetChild(string name, [NotNullWhen(true)] out KdlNode? node)
    {
        if (Children is not null && Children.TryGetNode(name, out var n))
        {
            node = n;
            return true;
        }

        node = null;
        return false;
    }


    private bool CheckArgumentIndex(int index) => index >= Arguments.Count || index < 0;

    public void BuildKdlString(StringBuilder builder)
    {
        if (Type is not null)
        {
            builder.Append($"({type})");
        }

        builder.Append($"\"{Name}\"");

        foreach (var prop in Properties)
        {
            builder.Append($" {prop.Key}=");
            ValueToString(builder, prop.Value);
        }

        foreach (var arg in Arguments)
        {
            builder.Append(' ');
            ValueToString(builder, arg);
        }

        if (Children is not null)
        {
            builder.Append('{');

            Children.BuildKdlString(builder);

            builder.Append('}');
        }
    }

    private void ValueToString(StringBuilder builder, KdlValue kdlValue)
    {
        switch (kdlValue)
        {
            case KdlBooleanValue value:
                value.BuildKdlString(builder);
                break;
            case KdlStringValue value:
                value.BuildKdlString(builder);
                break;
            case KdlNullValue value:
                value.BuildKdlString(builder);
                break;
            case KdlNumberValue value:
                value.BuildKdlString(builder);
                break;
            case KdlValue<string> value:
                value.BuildKdlString(builder);
                break;
            default:
                throw new UnreachableException();
        }
    }
}