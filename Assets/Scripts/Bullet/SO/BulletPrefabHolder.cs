using SpaceGame.Combat.Authoring;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BulletPrefabHolder", menuName = "SpaceGame/ScriptableObjects/BulletPrefabHolder")]
public class BulletPrefabHolder : ScriptableObject
{
    public List<BulletPrefabData> Data;
}


[System.Serializable]
public class BulletPrefabData
{
    public string Id;
    public BulletAuthoring Prefab;
}