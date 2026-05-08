using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public static class DialogParser
{
    private static string _spreadsheetID = "1Eoa2zYwYxPNovr95rMHB42f2RN0NAWGSuhB8QlSF01g";
    private static string _gid = "0";
    //private string DownloadUrl => $"https://docs.google.com/spreadsheets/d/{_spreadsheetID}/export?format=csv&gid={_gid}";
    //private string DownloadUrl => $"https://docs.google.com/spreadsheets/d/{_spreadsheetID}/edit?usp=sharing";

    private static string DownloadUrl => $"https://docs.google.com/spreadsheets/d/{_spreadsheetID}/gviz/tq?tqx=out:csv&sheet={_gid}";

    public static readonly DialogGraph DialogGraph = new DialogGraph();

    public static bool LoadAndParseFromGoogleSheets()
    {
        Debug.Log($"Loading Google Sheets: {DownloadUrl}");
        try
        {
            using (WebClient client = new WebClient())
            {
                var csvData = client.DownloadString(DownloadUrl);
                ParseCSVAndBuildTree(csvData);
                Debug.Log($"Successfully loaded and parsed {DialogGraph.Nodes.Count} nodes");
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading Google Sheets: {e.Message}");
            return false;
        }
    }

    public static void ParseCSVAndBuildTree(string csvData)
    {
        DialogGraph.ClearData();
        if (csvData == null)
        {
            Debug.LogError("CSV is empty");
            return;
        }

        var lines = csvData.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (var i = 1; i < Mathf.Min(lines.Length, 325); i++)
        {
            var fields = ParseCSVLine(lines[i]);

            if (fields.Length >= 2)
            {
                var parentId = int.Parse(fields[0]);
                var childId = int.Parse(fields[1]);

                var newRelation = new DialogRelation(parentId, childId);
                DialogGraph.Relations.Add(newRelation);
            }

            if (!string.IsNullOrEmpty(fields[3]))
            {
                var nodeId = i - 1;
                var speaker = fields[4];
                var text = fields[5];
                var isPlayer = speaker == "Question" ? false : bool.Parse(fields[6]);
                var parameters = fields[7];
                var conditions = fields[8];
                var isCorrect = bool.Parse(fields[9]);

                var newNode = new DialogNode(nodeId, speaker, text, isPlayer, parameters, conditions, isCorrect);
                DialogGraph.Nodes.Add(newNode);
                if (speaker == "Question")
                    DialogGraph.QuestionsCount++;
            }
        }

        BuildTree();
        return;
    }

    private static string[] ParseCSVLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var currentField = "";
        foreach (char c in line)
        {
            if (c == '"')
                inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
                currentField += c;
        }

        result.Add(currentField);
        return result.ToArray();
    }

    private static void BuildTree()
    {
        if (DialogGraph.Nodes.Count == 0)
            return;

        for (var i = 0; i < DialogGraph.Nodes.Count; i++)
        {
            var id = DialogGraph.Nodes[i].Id;
            foreach (var relation in DialogGraph.Relations)
            {
                if (id == relation.ChildID)
                    DialogGraph.Nodes[i].ParentIds.Add(relation.ParentID);
                else if (id == relation.ParentID)
                    DialogGraph.Nodes[i].ChildrenIds.Add(relation.ChildID);
            }
        }
    }
}