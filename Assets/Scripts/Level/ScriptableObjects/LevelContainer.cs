using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelContainer", menuName = "SpaceGame/ScriptableObjects/LevelContainer")]
public class LevelContainer : ScriptableObject
{
    public List<LevelDataHolder> Levels;
    public int PlayerTeam;
}