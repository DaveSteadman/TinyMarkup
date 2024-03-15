
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace TinyMarkup;

// ---------------------------------------------------------------------------------------------

public class TMConsts
{
    public const char openBracketChar = '[';
    public const char closeBracketChar = ']';
}

// ---------------------------------------------------------------------------------------------

public class TMElement
{
    public string Name { get; set; }
    public TMElement(string initName) => Name = initName;

    // Create a default blank element to return in case of error, to avoid passing nulls around
    public static TMElement Default() => new TMElement("_Default_");

    // Check if the element is a default
    public bool IsDefault() => Name == "_Default_";
}

// ---------------------------------------------------------------------------------------------

public class TMLeaf : TMElement
{
    public string Data { get; set; }

    public TMLeaf(string newName, string newData) : base(newName) => Data = newData;
}

// ---------------------------------------------------------------------------------------------

public class TMNode : TMElement
{
    private List<TMElement> elementList;

    public TMNode(string newName) : base(newName) => elementList = new List<TMElement>();

    public int ElementCount() { return elementList.Count; }

    public IReadOnlyList<TMElement> ElementList() { return elementList.AsReadOnly(); }

    public TMNode CreateChildNode(string newNodeName)
    {
        TMNode newNode = new TMNode(newNodeName);
        elementList.Add(newNode);
        return newNode;
    }

    public TMLeaf CreateChildLeaf(string newLeafName, string newData)
    {
        TMLeaf newLeaf = new TMLeaf(newLeafName, newData);
        elementList.Add(newLeaf);
        return newLeaf;
    }

    public void AddChild(TMElement newChild) { elementList.Add(newChild); }

    public string FindLeafData(string leafName)
    {
        return FindLeafDataRecursive(this, leafName);
    }

    private string FindLeafDataRecursive(TMNode node, string leafName)
    {
        foreach (TMElement element in node.elementList)
        {
            if ((element is TMLeaf leaf) && (leaf.Name == leafName))
                return leaf.Data;
            else if (element is TMNode childNode)
            {
                string result = FindLeafDataRecursive(childNode, leafName);
                if (result != string.Empty)
                    return result;
            }
        }
        return string.Empty;
    }
}

// ---------------------------------------------------------------------------------------------

public class TMParser
{
    public static TMElement Parse(string input)
    {
        using StringReader reader = new StringReader(input);
        return ReadTMElement(reader);
    }

    private static TMElement ReadTMElement(StringReader reader)
    {
        SkipWhiteSpace(reader);

        // Check for valid input and return the default if we don't see it.
        if (reader.Peek() == -1)
            return TMElement.Default();

        string name = ReadName(reader);

        SkipWhiteSpace(reader);
        if (reader.Peek() == TMConsts.openBracketChar)
        {
            TMNode node = new TMNode(name);
            while (reader.Peek() != TMConsts.closeBracketChar)
            {
                TMElement child = ReadTMElement(reader);
                if (child != null)
                    node.AddChild(child);
                SkipWhiteSpace(reader); // Add this line to skip white spaces between elements
            }

            reader.Read(); // Consume '}'
            return node;
        }
        else
        {
            string content = ReadContent(reader);
            reader.Read(); // Consume '}'
            return new TMLeaf(name, content);
        }
    }

    private static void SkipWhiteSpace(StringReader reader)
    {
        while (char.IsWhiteSpace((char)reader.Peek()))
        {
            reader.Read();
        }
    }

    private static string ReadName(StringReader reader)
    {
        SkipWhiteSpace(reader);
        if (reader.Peek() != TMConsts.openBracketChar) return string.Empty;
        reader.Read(); // Consume first '{'
        SkipWhiteSpace(reader);
        if (reader.Peek() != TMConsts.openBracketChar) return string.Empty;
        reader.Read(); // Consume second '{'

        SkipWhiteSpace(reader);
        string name = ReadUntil(reader, TMConsts.closeBracketChar);
        reader.Read(); // Consume '}'

        return name;
    }

    private static string ReadContent(StringReader reader)
    {
        string newContent = ReadUntil(reader, TMConsts.closeBracketChar);
        return newContent;
    }

    private static string ReadUntil(StringReader reader, char endChar)
    {
        StringBuilder result = new StringBuilder();
        while (reader.Peek() != endChar && reader.Peek() != -1)
        {
            result.Append((char)reader.Read());
        }
        return result.ToString();
    }
}

// ---------------------------------------------------------------------------------------------

public class TMSerializer
{
    public static string Serialize(TMElement element)
    {
        StringBuilder sb = new();
        SerializeCore(sb, element, 0);
        return sb.ToString();
    }

    private static void SerializeCore(StringBuilder sb, TMElement element, int indent)
    {
        if (indent > 0) sb.Append('\n'); // avoid newline on first output
        sb.Append(' ', indent * 2);      // 2 spaces per indent level

        if (element is TMNode node)
        {
            sb.Append($"{TMConsts.openBracketChar}{TMConsts.openBracketChar}{node.Name}{TMConsts.closeBracketChar}");
            foreach (TMElement child in node.ElementList())
            {
                SerializeCore(sb, child, indent + 1);
            }
            sb.Append('\n');
            sb.Append(' ', indent * 2); // 2 spaces per indent level
            sb.Append(TMConsts.closeBracketChar);
        }
        else if (element is TMLeaf leaf)
        {
            sb.Append($"{TMConsts.openBracketChar}{TMConsts.openBracketChar}{leaf.Name}{TMConsts.closeBracketChar}{leaf.Data}{TMConsts.closeBracketChar}");
        }
    }
} // class

