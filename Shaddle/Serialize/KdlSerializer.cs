namespace Shaddle.Serialize;

public static class KdlSerializer
{
    public static string SerializeCompact(ISerializable serializable) => serializable.ToKdlString();
}