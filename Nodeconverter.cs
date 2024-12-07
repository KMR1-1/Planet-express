using System;
using System.Collections.Generic;

public class NiNode
{
    public string Name { get; set; }
    public List<NiNode> Children { get; set; } = new List<NiNode>();
}

public class TreeNodeConverter
{
    private int idCounter = 0; // Pour générer des IDs uniques
    private Dictionary<NiNode, int> nodeToIdMap = new Dictionary<NiNode, int>();

    public List<Dictionary<string, object>> ConvertTreeToList(NiNode root)
    {
        var result = new List<Dictionary<string, object>>();
        ProcessNode(root, result);
        return result;
    }

    private int ProcessNode(NiNode node, List<Dictionary<string, object>> result)
    {
        if (nodeToIdMap.ContainsKey(node))
        {
            return nodeToIdMap[node]; // Retourne l'ID si déjà traité
        }

        int nodeId = idCounter++;
        nodeToIdMap[node] = nodeId;

        var childrenIds = new List<int>();
        foreach (var child in node.Children)
        {
            childrenIds.Add(ProcessNode(child, result));
        }

        var nodeDict = new Dictionary<string, object>
        {
            { "name", node.Name },
            { "id", nodeId },
            { "childrens", childrenIds }
        };

        result.Add(nodeDict);

        return nodeId;
    }
}
