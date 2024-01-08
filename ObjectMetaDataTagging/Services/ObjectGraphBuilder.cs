using ObjectMetaDataTagging.Models.TagModels;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Xml.Linq;

namespace ObjectMetaDataTagging.Services
{
    public class GraphNode  // Change the class name to GraphNode
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<GraphNode> Children { get; } = new List<GraphNode>();

        public GraphNode(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
    public class ObjectGraphBuilder
    {
        public static async Task<List<GraphNode>> BuildObjectGraph(ConcurrentDictionary<object, Dictionary<Guid, BaseTag>> concurrentDictionary)
        {
            var graphNodes = new List<GraphNode>();
            var visitedIds = new HashSet<Guid>();

            foreach (var kvp in concurrentDictionary)
            {
                var rootObject = kvp.Key;

                // Assuming you want to get the Id and Name properties
                var idProperty = rootObject.GetType().GetProperty("Id");
                var objectName = rootObject.GetType().Name;

                if (idProperty != null)
                {
                   // var objectId = idProperty.GetValue(rootObject);                  

                    var rootNode = await BuildSubgraph(rootObject, objectName, concurrentDictionary, visitedIds);
                    graphNodes.Add(rootNode);
                }
                else
                {
                    // Handle the case where the root object doesn't have Id and Name properties
                    // You might want to log a warning or throw an exception based on your requirements.
                }
            }

            return graphNodes;
        }

        private static async Task<GraphNode> BuildSubgraph(object rootObject, string objectName, ConcurrentDictionary<object, Dictionary<Guid, BaseTag>> concurrentDictionary, HashSet<Guid> visitedIds)
        {
            var idProperty = rootObject.GetType().GetProperty("Id");

            if (idProperty == null)
            {
                // Handle the case where the root object doesn't have an 'Id' property
                // You might want to log a warning or throw an exception based on your requirements.
                Console.WriteLine($"Id property not found on object of type '{rootObject.GetType().Name}'.");
                return null;
            }

            var objectId = idProperty.GetValue(rootObject);

            if (objectId == null)
            {
                // Handle the case where the 'Id' property is null
                // You might want to log a warning or throw an exception based on your requirements.
                Console.WriteLine($"Id property is null on object of type '{rootObject.GetType().Name}'.");
                return null;
            }

            var node = new GraphNode((Guid)objectId, objectName);

            // Check if objectId is present in the dictionary
            if (concurrentDictionary.TryGetValue(rootObject, out var tags))
            {
                if (tags != null)
                {
                    foreach (var tag in tags.Values)
                    {
                        var childNode = await BuildSubgraph(tag, tag.Name, concurrentDictionary, visitedIds);

                        if (childNode != null)
                        {
                            node.Children.Add(childNode);
                        }

                        if(tag.ChildTags != null)
                        {
                            foreach(var ct in tag.ChildTags)
                            {
                                var childTagNod = await BuildSubgraph(ct, ct.Name, concurrentDictionary, visitedIds);

                                if (childTagNod != null)
                                {
                                    node.Children.Add(childTagNod);
                                }

                            }
                        }
                    }
                }
                else
                {
                    // Handle the case where 'tags' is null
                    Console.WriteLine($"Tags are null for object with Id '{objectId}'.");
                }
            }
            else
            {
                // Handle the case where the key is not present in the dictionary
                Console.WriteLine($"Key '{objectId}' not present in the dictionary.");
            }

            return node;
        }




        public static void PrintObjectGraph(List<GraphNode> graphNodes)
        {
            foreach (var node in graphNodes)
            {
                Console.WriteLine($"Object: {node.Name}");
                PrintSubgraph(node, 1);
            }
        }

        private static void PrintSubgraph(GraphNode node, int depth)
        {
            var indent = new string(' ', depth * 2);
            foreach (var childNode in node.Children)
            {
                Console.WriteLine($"{indent}- {childNode.Name}");
                PrintSubgraph(childNode, depth + 1);
            }

            if (node.Children.Count == 0)
            {
                Console.WriteLine($"{indent}- (No Children)");
            }
        }
    }
}
