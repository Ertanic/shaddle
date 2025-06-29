using Pidgin;
using Shaddle.Values;

namespace Shaddle.Tests;

public class Nodes
{
    [Fact]
    public void Parse_NullValue()
    {
        const string val = "#null";
        var expected = new KdlNullValue();
        var actual = KdlParser.Value.ParseOrThrow(val) as KdlNullValue;
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Parse_Number()
    {
        const string val = "12.12";
        var expected = new KdlNumberValue(12.12);
        var actual = KdlParser.Value.ParseOrThrow(val) as KdlNumberValue;
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Parse_NumberKeyword()
    {
        const string val = "#nan";
        var expected = new KdlNumberValue(double.NaN);
        var actual = KdlParser.Value.ParseOrThrow(val) as KdlNumberValue;
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Parse_RawString()
    {
        const string val = "#\"\"Hello, World!\"\"#";
        var expected = new KdlStringValue("\"Hello, World!\"");
        var actual = KdlParser.Value.ParseOrThrow(val) as KdlStringValue;
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Parse_NodeEntryArgument()
    {
        var val = "value2";
        var expected = new KeyValuePair<string?, KdlValue>(null, new KdlStringValue("value2"));

        var actual = KdlParser.NodeEntry.ParseOrThrow(val);

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Parse_NodeEntryProperty()
    {
        var val = "key1=value1";
        var expected = new KeyValuePair<string?, KdlValue>("key1", new KdlStringValue("value1"));

        var actual = KdlParser.NodeEntry.ParseOrThrow(val);

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Parse_EmptyNode()
    {
        var val = "node1";
        var expected = new KdlNode("node1");

        var actual = KdlParser.Node.ParseOrThrow(val);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Parse_NodeEntries()
    {
        var val = "node1 arg1 prop1=hello-world arg2 prop2=\"goodbye world\"";
        var expected = new KdlNode("node1")
        {
            Arguments = [new KdlStringValue("arg1"), new KdlStringValue("arg2")],
            Properties = new Dictionary<string, KdlValue>()
            {
                { "prop1", new KdlStringValue("hello-world") },
                { "prop2", new KdlStringValue("goodbye world") }
            }
        };

        var actual = KdlParser.Node.ParseOrThrow(val);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public void Parse_DocumentWithSeparatedNodes()
    {
        var val = "node1; node2";

        List<KdlNode> nodes = [new KdlNode("node1"), new KdlNode("node2")];
        var expected = new KdlDocument(nodes);

        var actual = KdlParser.Document.ParseOrThrow(val);
        Assert.Equivalent(expected, actual, true);
    }

    [Fact]
    public void Parse_DocumentWithSeparatedValues()
    {
        var val = "node1 prop1=\"Hello, World!\"; node2 arg";

        List<KdlNode> nodes =
        [
            new KdlNode("node1")
            {
                Properties = new Dictionary<string, KdlValue>()
                    { { "prop1", new KdlStringValue("Hello, World!") } }
            },
            new KdlNode("node2")
                { Arguments = [new KdlStringValue("arg")] }
        ];
        var expected = new KdlDocument(nodes);

        var actual = KdlParser.Document.ParseOrThrow(val);
        Assert.Equivalent(expected, actual, true);
    }

    [Fact]
    public void Parse_DocumentWithSeparatedNodesByNewlines()
    {
        var val = "node1 prop1=\"Hello, World!\"\nnode2 arg1";
        List<KdlNode> nodes =
        [
            new KdlNode("node1")
            {
                Properties = new Dictionary<string, KdlValue>()
                    { { "prop1", new KdlStringValue("Hello, World!") } }
            },
            new KdlNode("node2")
            {
                Arguments = [new KdlStringValue("arg1")]
            }
        ];
        var expected = new KdlDocument(nodes.OrderBy(n => n.Name).ToList());
        var actual = KdlParser.Document.ParseOrThrow(val);
        Assert.Equivalent(expected.Nodes.ToList()[0], actual.Nodes.ToList()[0], true);
    }

    [Fact]
    public void Parse_NodeWithChildren()
    {
        var val = "node1 { node2; node3\nNode4\n};node5";
        KdlNode[] nodes =
        [
            new KdlNode("node1")
                { Children = new KdlDocument([new KdlNode("node2"), new KdlNode("node3"), new KdlNode("Node4")]) },
            new KdlNode("node5")
        ];
        var expected = new KdlDocument(nodes);

        var actual = KdlParser.Document.ParseOrThrow(val);
        Assert.Equivalent(expected, actual, true);
    }

    [Fact]
    public void Parse_NodeQuotedStringName()
    {
        var val = "\"Node 1\" {\n \"Node 2\" \n}";
        List<KdlNode> nodes = [new KdlNode("Node 1") { Children = new KdlDocument([new KdlNode("Node 2")]) }];
        var expected = new KdlDocument(nodes);

        var actual = KdlParser.Document.ParseOrThrow(val);
        Assert.Equivalent(expected, actual, true);
    }

    [Fact]
    public void Parse_NodeWithoutChildren()
    {
        var val = "node1\n{\n\n}";
        var expected = new KdlDocument([new KdlNode("node1")]);

        var actual = KdlParser.Document.ParseOrThrow(val);
        Assert.Equivalent(expected, actual, true);
    }

    [Fact]
    public void Parse_RealExample()
    {
        var val = """
                  entity Foobar {
                    parent Baz
                    name Real?
                    
                    components {
                      DoesTheThing
                      OnemoreThing property="value" /* comment 1
                      */
                      
                      // or alternatively
                      AnotherThing {
                        property "value" // comment 2
                      }
                    }
                  }
                  """;

        var expected = new KdlDocument([
            new KdlNode("entity")
            {
                Arguments = [new KdlStringValue("Foobar")],
                Children = new KdlDocument([
                    new KdlNode("parent")
                    {
                        Arguments = [new KdlStringValue("Baz")]
                    },
                    new KdlNode("name")
                    {
                        Arguments = [new KdlStringValue("Real?")]
                    },
                    new KdlNode("components")
                    {
                        Children = new KdlDocument([
                            new KdlNode("DoesTheThing"),
                            new KdlNode("OnemoreThing")
                            {
                                Properties = new Dictionary<string, KdlValue>()
                                    { { "property", new KdlStringValue("value") } }
                            },
                            new KdlNode("AnotherThing")
                            {
                                Children = new KdlDocument([
                                    new KdlNode("property")
                                    {
                                        Arguments = [new KdlStringValue("value")]
                                    }
                                ])
                            }
                        ])
                    }
                ]),
            }
        ]);

        var actual = KdlParser.Document.ParseOrThrow(val);
        Assert.Equivalent(expected, actual, true);
    }
}