using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="TrailPrefabHolder", menuName = "SpaceGame/ScriptableObjects/TrailPrefabHolder")]
public class TrailPrefabHolder : ScriptableObject
{
    public List<TrailPrefab> Prefabs;
}

[System.Serializable]
public class TrailPrefab
{
    public string Id;
    public TrailRenderer Renderer;
}
