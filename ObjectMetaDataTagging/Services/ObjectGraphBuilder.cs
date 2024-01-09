using ObjectMetaDataTagging.Models.TagModels;
using System.Collections.Concurrent;

namespace ObjectMetaDataTagging.Services
{
    /// <summary>
    ///  A service to build an object graph structure with a given dictionary, 
    ///  using a depth-first recursive approach it will scan all tag hierarchies and display them.
    /// </summary>
    public class GraphNode
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


                var objectName = rootObject.GetType().Name;

                if (rootObject != null)
                {
                    var rootNode = await BuildSubgraph(rootObject, objectName, concurrentDictionary, visitedIds);
                    if (rootNode != null)
                    {
                        graphNodes.Add(rootNode);
                    }
                }
                else
                {
                    // when the root object doesn't have Id and Name properties
                }
            }

            return graphNodes;
        }

        private static async Task<GraphNode?> BuildSubgraph(object rootObject, string objectName, ConcurrentDictionary<object, Dictionary<Guid, BaseTag>> concurrentDictionary, HashSet<Guid> visitedIds)
        {
            var idProperty = rootObject.GetType().GetProperty("Id");
            if (idProperty == null) return null;
            
            var objectId = idProperty.GetValue(rootObject);
            if (objectId == null) return null;

            if (visitedIds.Contains((Guid)objectId))
            {

                return null;
            }

            visitedIds.Add((Guid)objectId);

            var node = new GraphNode((Guid)objectId, objectName);

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

                            // Iterate through direct child tags
                            foreach (var childTag in tag.ChildTags)
                            {
                                var childTagNode = await BuildSubgraph(childTag, childTag.Name, concurrentDictionary, visitedIds);

                                if (childTagNode != null)
                                {
                                    childNode.Children.Add(childTagNode);

                                    // Check if the childTag has grandchild tags
                                    if (childTag.ChildTags != null)
                                    {
                                        foreach (var grandChildTag in childTag.ChildTags)
                                        {
                                            var grandchildTagNode = await BuildSubgraph(grandChildTag, grandChildTag.Name, concurrentDictionary, visitedIds);

                                            if (grandchildTagNode != null)
                                            {
                                                childTagNode.Children.Add(grandchildTagNode);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // when tags is null
                    //Console.WriteLine($"Tags are null for object with Id '{objectId}'.");
                }
            }
            else
            {
                // when key is not present in the dictionary
                //Console.WriteLine($"Key '{objectId}' not present in the dictionary.");
            }

            return node;
        }


        public static void PrintObjectGraph(List<GraphNode> graphNodes)
        {
            foreach (var node in graphNodes)
            {
                Console.WriteLine($"Root Object: {node.Name}");
                PrintSubgraph(node, 1, true);
            }
        }

        private static void PrintSubgraph(GraphNode node, int depth, bool isRoot = false)
        {
            var indent = new string(' ', depth * 6);

            if (!isRoot)
            {
                Console.WriteLine($"{indent}|___ {node.Name}");
            }

            foreach (var childNode in node.Children)
            {
                PrintSubgraph(childNode, depth + 1);
            }
        }

    }
}
