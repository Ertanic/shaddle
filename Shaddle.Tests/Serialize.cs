using Shaddle.Serialize;
using Shaddle.Values;

namespace Shaddle.Tests;

public class Serialize
{
    [Fact]
    public void Serialize_DocumentWithoutNodes()
    {
        var val = (new KdlDocument([]) as ISerializable).ToKdlString();
        var expected = string.Empty;

        Assert.Equal(val, expected);
    }

    [Fact]
    public void Serialize_Document()
    {
        var val = (new KdlDocument([new KdlNode("node1"), new KdlNode("node2")]) as ISerializable).ToKdlString();
        var expected = "\"node1\";\"node2\"";

        Assert.Equal(val, expected);
    }

    [Fact]
    public void Serialize_DocumentWithNestedNodes()
    {
        var val = (new KdlDocument([new KdlNode("node1") {
            Children = new KdlDocument([new KdlNode("node2")])
        }, new KdlNode("node3")]) as ISerializable).ToKdlString();
        var expected = "\"node1\"{\"node2\"};\"node3\"";

        Assert.Equal(val, expected);
    }

    [Fact]
    public void Serialize_DocumentWithArguments()
    {
        var val = (new KdlDocument([new KdlNode("node1") { Arguments = [
                new KdlBooleanValue(true), new KdlNullValue(),
                new KdlNumberValue(2.333), new KdlStringValue("str")]
            }]) as ISerializable).ToKdlString();
        var expected = "\"node1\" #true #null 2.333 \"str\"";

        Assert.Equal(val, expected);
    }

    [Fact]
    public void Serialize_DocumentWithProps()
    {
        var val = (new KdlDocument([new KdlNode("node1") {
            Properties = new Dictionary<string, KdlValue>() {
                { "hello", new KdlStringValue("world") },
                { "number", new KdlNumberValue(3.432) },
                { "bool", new KdlBooleanValue(true) },
                { "null", new KdlNullValue() },
                { "custom", new KdlValue<string>(".*", "regex") }
            }
        }]) as ISerializable).ToKdlString();
        var expected = "\"node1\" hello=\"world\" number=3.432 bool=#true null=#null custom=(regex)\".*\"";

        Assert.Equal(val, expected);
    }

    [Fact]
    public void SerializeRoundtripCompact()
    {
        var val = """
                  node1 {
                    node2 hello=world 2333
                  }
                  node3 #null optional=#true {
                    node4 1 2 \
                          3 4
                  }
                  """;

        var parsed = KdlParser.Parse(val);
        var serialized = KdlSerializer.SerializeCompact(parsed);

        var expected = @"""node1""{""node2"" hello=""world"" 2333};""node3"" optional=#true #null{""node4"" 1 2 3 4}";
        
        Assert.Equal(serialized, expected);
    }
}