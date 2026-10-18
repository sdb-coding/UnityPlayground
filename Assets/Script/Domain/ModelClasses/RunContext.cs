using UnityEngine;
using System.Collections.Generic;
using System;

public static class RunContext
{
    public static event Action<GameModel> onRunContextBound;
    public static event Action OnRunContextUnbound;
    public static GameModel Current { get; set; }
    public static void Bind(GameModel gameModel)
    {
        Debug.Log($"Request to bind. Current is null: {Current == null}");
        Current = gameModel;
        onRunContextBound?.Invoke(Current);
    }
    public static void Unbind(GameModel gameModel)
    {
        if (gameModel == Current)
        {
            Current = null;
            OnRunContextUnbound?.Invoke();
        }
        else
            Debug.Log($"Request to unbind, but gamemodel not current. Is it null? {Current == null}");
    }
}
