using System.Text;
using Shaddle.Serialize;

namespace Shaddle;

public sealed class KdlDocument(IReadOnlyCollection<KdlNode> nodes) : ISerializable
{
    public IReadOnlyCollection<KdlNode> Nodes { get; } = nodes;
    
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