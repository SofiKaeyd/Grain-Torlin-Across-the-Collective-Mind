using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogGraph", menuName = "Dialog System/Dialog Graph")]
public class DialogGraph : ScriptableObject
{
    public List<DialogRelation> Relations = new List<DialogRelation>();
    public List<DialogNode> Nodes = new List<DialogNode>();
    public int QuestionsCount = 0;

    public void Set(DialogGraph graph)
    {
        Relations = graph.Relations;
        Nodes = graph.Nodes;
        QuestionsCount = graph.QuestionsCount;
    }

    public DialogNode GetNode(int id)
    {
        return Nodes.Count > id ? Nodes[id] : null;
    }

    public List<DialogNode> GetChildren(DialogNode node)
    {
        var children = new List<DialogNode>();
        foreach (var childId in node.ChildrenIds)
        {
            var child = GetNode(childId);
            if (child != null)
                children.Add(child);
        }

        return children;
    }

    public List<DialogNode> GetParents(DialogNode node)
    {
        var parents = new List<DialogNode>();
        foreach (var parentId in node.ParentIds)
        {
            var parent = GetNode(parentId);
            if (parent != null)
                parents.Add(parent);
        }

        return parents;
    }

    public void ClearData()
    {
        Relations.Clear();
        Nodes.Clear();
        QuestionsCount = 0;
    }
}

[Serializable]
public class DialogNode
{
    public int Id;
    public string Speaker;
    public string Text;
    public bool IsPlayer;
    public string Parameters;
    public string Conditions;
    public bool IsCorrect;

    public List<int> ParentIds = new List<int>();
    public List<int> ChildrenIds = new List<int>();

    public string FormattedText
    {
        get
        {
            var paramNames = Parameters.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var args = new List<object>();
            foreach (var name in paramNames)
                args.Add(DynamicParameters.Get(name));

            return string.Format(Text, args.ToArray());
        }
    }

    public bool Available
    {
        get
        {
            var conditionNames = Conditions.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var args = new List<object>();
            foreach (var name in conditionNames)
                args.Add(DynamicParameters.Get(name));

            return args.All(a => a.Equals(true));
        }
    }

    public DialogNode(int nodeId, string speaker, string text, bool isPlayer, string parameters, string conditions, bool isCorrect)
    {
        Id = nodeId;
        Speaker = speaker;
        Text = text;
        IsPlayer = isPlayer;
        Parameters = parameters;
        Conditions = conditions;
        IsCorrect = isCorrect;
    }
}

[Serializable]
public class DialogRelation
{
    public int ParentID;
    public int ChildID;

    public DialogRelation(int parentId, int childId)
    {
        ParentID = parentId;
        ChildID = childId;
    }
}
