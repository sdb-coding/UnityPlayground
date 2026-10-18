using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public static class CardRegistryModel
{
    static GameModel Current { get; set; }

    public static void Bind(GameModel gameModel)
    {
        Debug.Log($"Request to bind. Current is null: {Current == null}");
        Current = gameModel;
    }

    public static void Unbind(GameModel gameModel)
    {
        if (gameModel == Current)
            Current = null;
        else
            Debug.Log($"Request to unbind, but gamemodel not current. Is it null? {Current == null}");
    }
    public static T[] GetModels<T>(List<string> ids) where T : class, ICardModel
    {
        if (Current == null || ids == null)
            return new T[0];

        T[] results = new T[ids.Count];
        for (int i = 0; i < ids.Count; i++)
        {
            results[i] = Current.GetModel<T>(ids[i]);
        }

        return results;
    }
    public static T GetModel<T>(string id) where T : class, ICardModel
    {
        return Current != null ? Current.GetModel<T>(id) : null;
    }
    public static void RegisterModel(ICardModel cardModel)
    {
        if (Current != null)
            Current.RegisterModel(cardModel);
    }
    public static void UnRegisterModel(ICardModel cardModel)
    {
        if (Current != null)
            Current.UnRegisterModel(cardModel);
    }
}