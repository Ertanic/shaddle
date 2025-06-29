namespace Shaddle;

public class KdlDocument(IReadOnlyCollection<KdlNode> nodes)
{
    public IReadOnlyCollection<KdlNode> Nodes { get; } = nodes;
}