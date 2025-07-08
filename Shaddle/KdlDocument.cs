using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using Shaddle.Serialize;

namespace Shaddle;

public sealed class KdlDocument(IReadOnlyCollection<KdlNode> nodes) : ISerializable
{
    public IReadOnlyCollection<KdlNode> Nodes { get; } = nodes;

    /// <summary>
    /// Checks for the presence of a node in the document.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    public bool HasNode(string name)
    {
        foreach (var node in Nodes)
            if (node.Name == name)
                return true;

        return false;
    }

    /// <summary>
    /// Retrieves the node from the document.
    /// Throws an exception if the node by the passed name is not found.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    /// <exception cref="KeyNotFoundException"></exception>
    public KdlNode GetNode(string name)
    {
        if (TryGetNode(name, out var node))
            return node;

        throw new KeyNotFoundException($"{name} key is not found");
    }

    /// <summary>
    /// A safer way to get a node than the <seealso cref="GetNode"/> method.
    /// </summary>
    /// <param name="name">The name of the node.</param>
    /// <param name="node">The node.</param>
    public bool TryGetNode(string name, [NotNullWhen(true)] out KdlNode? node)
    {
        foreach (var n in Nodes)
        {
            if (n.Name == name)
            {
                node = n;
                return true;
            }
        }

        node = null;
        return false;
    }

    public void BuildKdlString(StringBuilder builder)
    {
        var nodes = Nodes.ToList();
        for (var i = 0; i < Nodes.Count; i++)
        {
            var node = nodes[i];

            node.BuildKdlString(builder);

            if (i != nodes.Count - 1)
            {
                builder.Append(';');
            }
        }
    }
}