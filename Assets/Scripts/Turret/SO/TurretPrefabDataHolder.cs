using SpaceGame.Combat.Authoring;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TurretPrefabDataHolder", menuName = "SpaceGame/ScriptableObjects/TurretPrefabDataHolder")]
public class TurretPrefabDataHolder : ScriptableObject
{
    public List<TurretPrefabData> Data;
}


[System.Serializable]
public class TurretPrefabData
{
    public string Id;
    public TurretAuthoring Prefab;
}
