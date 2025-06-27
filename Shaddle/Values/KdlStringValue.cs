namespace Shaddle.Values;

public class KdlStringValue(string value) : KdlValue<string>(value, nameof(String));