using SpaceGame;
using SpaceGame.Combat.Defences;
using UnityEditor;
using UnityEngine;
using static ResistanceMatrixAsset;

[CustomEditor(typeof(ResistanceMatrixAsset))]
public class ResistanceMatrixAssetEditor : Editor
{
    private string[] damageTypes = System.Enum.GetNames(typeof(DamageType));
    private string[] layerTypes = System.Enum.GetNames(typeof(DefenceLayerType));

    public override void OnInspectorGUI()
    {
        var asset = (ResistanceMatrixAsset)target;
        var entries = asset.Entries;

        EditorGUILayout.LabelField("Resistance Matrix", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        // Header row
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Damage Type", GUILayout.Width(100));
        foreach (var layer in layerTypes)
            EditorGUILayout.LabelField(layer, GUILayout.Width(70));
        EditorGUILayout.EndHorizontal();

        // Ensure all combinations exist
        foreach (DamageType damage in System.Enum.GetValues(typeof(DamageType)))
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(damage.ToString(), GUILayout.Width(100));

            foreach (DefenceLayerType layer in System.Enum.GetValues(typeof(DefenceLayerType)))
            {
                ResistanceEntryHolder entry = entries.Find(e => e.DamageType == damage && e.LayerType == layer);
                if (entry == null)
                {
                    entry = new ResistanceEntryHolder { DamageType = damage, LayerType = layer, Value = 0f };
                    entries.Add(entry);
                }

                entry.Value = EditorGUILayout.FloatField(entry.Value, GUILayout.Width(70));
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
