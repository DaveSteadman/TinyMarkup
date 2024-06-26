
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

    public TMLeaf CreateChildLeafString(string newLeafName, string newData)
    {
        TMLeaf newLeaf = new TMLeafString(newLeafName, newData);
        elementList.Add(newLeaf);
        return newLeaf;
    }

    public TMLeafFloat CreateChildLeafFloat(string newLeafName, float newData, int precision = 2)
    {
        TMLeafFloat newLeaf = new TMLeafFloat(newLeafName, newData);
        elementList.Add(newLeaf);
        return newLeaf;
    }

    public TMLeafDouble CreateChildLeafDouble(string newLeafName, double newData, int precision = 2)
    {
        TMLeafDouble newLeaf = new TMLeafDouble(newLeafName, newData);
        elementList.Add(newLeaf);
        return newLeaf;
    }

    public TMLeafInt CreateChildLeafInt(string newLeafName, int newData)
    {
        TMLeafInt newLeaf = new TMLeafInt(newLeafName, newData);
        elementList.Add(newLeaf);
        return newLeaf;
    }

    public void AddChild(TMElement newChild) { elementList.Add(newChild); }


    public TMLeaf? FindLeaf(string leafName)
    {
        foreach (TMElement element in node.elementList)
        {
            if ((element is TMLeaf leaf) && (leaf.Name == leafName))
                return leaf;
        }
    }
    
    private TMLeaf? FindLeafRecursive(TMNode node, string leafName)
    {
        foreach (TMElement element in node.elementList)
        {
            if ((element is TMLeaf leaf) && (leaf.Name == leafName))
                return leaf;
            else if (element is TMNode childNode)
            {
                TMLeaf? result = FindLeafRecursive(childNode, leafName);
                if (result != null)
                    return result;
            }
        }
        return null;
    }
}

// ---------------------------------------------------------------------------------------------

public abstract class TMLeaf : TMElement
{
    public TMLeaf(string newName) : base(newName) { }
    public abstract bool TryCreate(string name, string data, out TMLeaf leaf);
    public abstract string DataString();
}

public class TMLeafString : TMLeaf
{
    public string Data { get; private set; }

    public TMLeafString(string newName, string newData) : base(newName) => Data = newData;
    public override bool TryCreate(string name, string data, out TMLeaf leaf)
    {
        leaf = new TMLeafString(name, data);
        return true;  // Always succeeds
    }
    public override string DataString() => Data;
}

public class TMLeafInt : TMLeaf
{
    public int NumericData { get; private set; }

    public TMLeafInt(string newName, int newData) : base(newName) => NumericData = newData;

    public override bool TryCreate(string name, string data, out TMLeaf leaf)
    {
        // init out variable
        leaf = null;

        // If either strings are null. Fail.
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(data)) return false;

        // Check if the number part is a valid float number
        if (int.TryParse(data, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int result))
        {
            leaf = new TMLeafInt(name, result);
            return true;
        }
        return false;
    }
    public override string DataString() => NumericData.ToString();
}

public class TMLeafFloat : TMLeaf
{
    public float NumericData { get; private set; }

    public TMLeafFloat(string newName, float newData) : base(newName) => NumericData = newData;

    public override bool TryCreate(string name, string data, out TMLeaf leaf)
    {
        // Initialise the out variable
        leaf = null;

        // If either strings are null. Fail. If the last character is not a "f", fail. 
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(data)) return false;
        if (!data.EndsWith("f")) return false;

        // Remove the last 'f' character to parse the number
        string numberPart = data.Substring(0, data.Length - 1);

        // Check if the number part is a valid float number
        if (float.TryParse(numberPart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
        {
            leaf = new TMLeafFloat(name, result);
            return true;
        }
        return false;
    }

    public override string DataString() => NumericData.ToString() + "f";
}

public class TMLeafDouble : TMLeaf
{
    public double NumericData { get; private set; }

    public TMLeafDouble(string newName, double newData) : base(newName) => NumericData = newData;

    public override bool TryCreate(string name, string data, out TMLeaf leaf)
    {
        // init out variable
        leaf = null;

        // If either strings are null. Fail. 
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(data)) return false;

        // Check if the number part is a valid float number
        if (double.TryParse(data, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double result))
        {
            leaf = new TMLeafDouble(name, result);
            return true;
        }
        return false;
    }

    public override string DataString() => NumericData.ToString();
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
    
        if (reader.Peek() == -1)
            return TMElement.Default();
    
        string name = ReadName(reader);
        if (string.IsNullOrEmpty(name))
            return TMElement.Default();
    
        SkipWhiteSpace(reader);
    
        if (reader.Peek() == TMConsts.openBracketChar)
        {
            reader.Read(); // Consume the '['
            TMNode node = new TMNode(name);
            while (reader.Peek() != TMConsts.closeBracketChar && reader.Peek() != -1)
            {
                TMElement child = ReadTMElement(reader);
                if (child != null && !child.IsDefault())
                    node.AddChild(child);
                SkipWhiteSpace(reader);
            }
            if (reader.Peek() == TMConsts.closeBracketChar)
                reader.Read(); // Consume the closing ']'
            return node;
        }
        else
        {
            string content = ReadContent(reader);
            if (reader.Peek() == TMConsts.closeBracketChar)
                reader.Read(); // Consume the closing ']'
    
            // Try to create a leaf node, falling back through potential types
            List<Type> leafTypes = new List<Type> { typeof(TMLeafInt), typeof(TMLeafFloat), typeof(TMLeafDouble), typeof(TMLeafString) };
            foreach (Type type in leafTypes)
            {
                TMLeaf leaf;
                var instance = Activator.CreateInstance(type, new object[] { name, content }) as TMLeaf;
                if (instance != null && instance.TryCreate(name, content, out leaf))
                {
                    return leaf;
                }
            }
    
            // Fall back to a string leaf if no other types matched
            return new TMLeafString(name, content);
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
            string dataRepresentation = leaf.DataString();
            if (leaf is TMLeafFloat)
                dataRepresentation += "f";

            sb.Append($"{TMConsts.openBracketChar}{TMConsts.openBracketChar}{leaf.Name}{TMConsts.closeBracketChar}{dataRepresentation}{TMConsts.closeBracketChar}");
        }
    }
} // class

