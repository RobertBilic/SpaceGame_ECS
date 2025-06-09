using SpaceGame.Combat.Authoring;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BulletPrefabHolder", menuName = "SpaceGame/ScriptableObjects/BulletPrefabHolder")]
public class ProjectilePrefabHolder : ScriptableObject
{
    public List<ProjectilePrefabData> Data;
}


[System.Serializable]
public class ProjectilePrefabData
{
    public string Id;
    public GameObject Prefab;
}