namespace Shaddle.Values;

public sealed class KdlNumberValue(double value) : KdlValue<double>(value, nameof(Double));