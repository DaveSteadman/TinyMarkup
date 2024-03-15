
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TinyMarkup;

/*
    Expected output:

        Serialized tree:

        [[Root]
          [[Child1]
            [[Leaf1] A + b = c]
            [[Leaf2]general statement]
          ]
          [[Child2]
            [[Child3]
              [[Leaf3]'string literal']
            ]
          ]
        ]

        Deserialized tree structure:

        [[Root]
          [[Child1]
            [[Leaf1]A + b = c]
            [[Leaf2]general statement]
          ]
          [[Child2]
            [[Child3]
              [[Leaf3]'string literal']
            ]
          ]
        ]

        Found nested leaf data item: 'string literal'
*/

class Program
{
    static void Main(string[] args)
    {
        // Create a tree structure
        TMNode rootNode = new TMNode("Root");
        TMNode childNode1 = rootNode.CreateChildNode("Child1");
        TMNode childNode2 = rootNode.CreateChildNode("Child2");

        childNode1.CreateChildLeaf("Leaf1", " A + b = c");
        childNode1.CreateChildLeaf("Leaf2", "general statement");
        TMNode childNode3 = childNode2.CreateChildNode("Child3");
        childNode3.CreateChildLeaf("Leaf3", "'string literal'");

        // Serialize the tree structure
        string serializedTree = TMSerializer.Serialize(rootNode);
        Console.WriteLine("Serialized tree:");
        Console.WriteLine(serializedTree);

        // Deserialize the tree structure
        TMElement deserializedTree = TMParser.Parse(serializedTree);
        Console.WriteLine("\nDeserialized tree structure:");

        // Serialize the deserialized tree to verify deserialization
        string reserializedTree = TMSerializer.Serialize(deserializedTree);
        Console.WriteLine(reserializedTree);

        // Find a nested leaf data item
        if (deserializedTree is TMNode deserializedRootNode)
        {
            string leafData = deserializedRootNode.FindLeafData("Leaf3");
            Console.WriteLine($"\nFound nested leaf data item: {leafData}");
        }
        else
        {
            Console.WriteLine("The deserialized tree is not a node.");
        }
    }
}
