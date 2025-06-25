using SpaceGame;
using SpaceGame.Combat.Defences;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewResistanceMatrix", menuName = "Combat/Resistance Matrix")]
public class ResistanceMatrixAsset : ScriptableObject
{
    [System.Serializable]
    public class ResistanceEntryHolder
    {
        public DamageType DamageType;
        public DefenceLayerType LayerType;
        public float Value;
    }

    public List<ResistanceEntryHolder> Entries;
    public float GetResistance(DamageType damageType, DefenceLayerType layer)
    {
        foreach (var entry in Entries)
        {
            if (entry.DamageType == damageType && entry.LayerType == layer)
                return entry.Value;
        }

        Debug.LogWarning($"No resistance found for {damageType} on {layer}, returning 0.");
        return 0f;
    }
}