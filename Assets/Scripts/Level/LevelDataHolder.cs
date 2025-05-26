using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="LevelData", menuName = "SpaceGame/ScriptableObjects/LevelData")]
public class LevelDataHolder : ScriptableObject
{
    public string Name;
    public List<LevelShipEntryData> ShipEntries;
    public Vector3 PlayerSpawnPosition;
    public float PlayerSpawnRadius;
}


[System.Serializable]
public class LevelShipEntryData
{
    public string Id;
    public int Team;
    public int Count;
    public Vector3 Position;
    public float SpawnRadius;
}