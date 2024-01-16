﻿using ObjectMetaDataTagging.Models.TagModels;
using System.Collections.Concurrent;

namespace ObjectMetaDataTagging.Utilities
{
    /// <summary>
    ///  A service to build an object graph structure with a given dictionary, 
    ///  using a depth-first recursive approach it will scan all tag hierarchies and form a graph.
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
                    Console.WriteLine("Root object doesn't have a Id or Name properties.");
                }
            }

            return graphNodes;
        }

        private static async Task<GraphNode?> BuildSubgraph(object rootObject, string objectName, ConcurrentDictionary<object, Dictionary<Guid, BaseTag>> concurrentDictionary, HashSet<Guid> visitedIds)
        {
            try
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

                                // Recursively process all child tag depths
                                await ProcessChildTags(tag.ChildTags, childNode, concurrentDictionary, visitedIds);
                            }
                        }
                    }
                }
                return node;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error building subgraph: {ex.Message}");
                return null;
            }
        }

        private static async Task ProcessChildTags(IEnumerable<BaseTag> childTags, GraphNode parentNode, ConcurrentDictionary<object, Dictionary<Guid, BaseTag>> concurrentDictionary, HashSet<Guid> visitedIds)
        {
            foreach (var childTag in childTags)
            {
                var childNode = await BuildSubgraph(childTag, childTag.Name, concurrentDictionary, visitedIds);

                if (childNode != null)
                {
                    parentNode.Children.Add(childNode);

                    await ProcessChildTags(childTag.ChildTags, childNode, concurrentDictionary, visitedIds);
                }
            }
        }

        public static void PrintObjectGraph(List<GraphNode> graphNodes)
        {
            foreach (var node in graphNodes)
            {
                Console.WriteLine($"\nRoot Object: {node.Name}");
                PrintSubgraph(node, 1, true);
            }
        }

        private static void PrintSubgraph(GraphNode node, int depth, bool isRoot = false)
        {
            var indent = new string(' ', depth * 5);

            if (!isRoot)
            {
                Console.WriteLine($"{indent}└──── {node.Name}");
            }

            foreach (var childNode in node.Children)
            {
                PrintSubgraph(childNode, depth + 1);
            }
        }


    }
}
