using System;
using System.Collections.Generic;
using UnityEngine;

public class DynamicParameters
{
    private static readonly Dictionary<string, Func<object>> _parameterGetters = new()
    {
        //["UserName"] = () => Gameplay.UserName,
        //["Win"] = () => Gameplay.IsAllWin,
        //["Fall"] = () => !Gameplay.IsAllWin,

    };

    public static object Get(string key)
    {
        if (_parameterGetters.TryGetValue(key, out var getter))
            return getter();

        Debug.Log($"Parameter '{key}' not found");
        return "";
    }
}
