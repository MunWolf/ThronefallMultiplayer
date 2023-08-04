﻿using System.Collections.Generic;
using System.Diagnostics.Contracts;
using HarmonyLib;
using UnityEngine;

namespace ThronefallMP.Components;

public enum IdentifierType
{
    Invalid,
    Player,
    Building,
    Ally,
    Enemy
}

public struct IdentifierData
{
    public IdentifierType Type;
    public int Id;

    public IdentifierData(Identifier identity)
    {
        if (identity != null)
        {
            Type = identity.Type;
            Id = identity.Id;
        }
    }

    [Pure]
    public GameObject Get()
    {
        return Identifier.GetGameObject(Type, Id);
    }
}

public class Identifier : MonoBehaviour
{
    private static readonly Dictionary<IdentifierType, Dictionary<int, GameObject>> Repository = new()
    {
        { IdentifierType.Player, new Dictionary<int, GameObject>() },
        { IdentifierType.Building, new Dictionary<int, GameObject>() },
        { IdentifierType.Ally, new Dictionary<int, GameObject>() },
        { IdentifierType.Enemy, new Dictionary<int, GameObject>() }
    };

    public IdentifierType Type { get; private set; }

    public int Id { get; private set; }

    public void SetIdentity(IdentifierType type, int id)
    {
        Type = type;
        Id = id;
        Repository[Type][Id] = gameObject;
        Plugin.Log.LogInfo($"Added {type}:{id} to identifier repository.");
    }

    public static void Clear(IdentifierType type)
    {
        Repository[type].Clear();
    }
    
    public static GameObject GetGameObject(IdentifierType type, int id)
    {
        return type == IdentifierType.Invalid ? null : Repository[type].GetValueSafe(id);
    }
}