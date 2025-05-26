using SpaceGame.Combat.Authoring;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipPrefabDataHolder", menuName = "SpaceGame/ScriptableObjects/ShipPrefabDataHolder")]
public class ShipPrefabDataHolder : ScriptableObject
{
    public List<ShipPrefabData> Data;
}

[System.Serializable]
public class ShipPrefabData
{
    public string Id;
    public ShipAuthoring Prefab;
    public float DefaultScale;
}