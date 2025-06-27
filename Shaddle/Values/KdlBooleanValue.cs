namespace Shaddle.Values;

public class KdlBooleanValue(bool value) : KdlValue<bool>(value, nameof(Boolean));