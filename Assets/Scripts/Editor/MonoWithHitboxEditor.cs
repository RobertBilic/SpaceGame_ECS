using UnityEditor;
using UnityEngine;

namespace SpaceGame.Editor
{
    [CustomEditor(typeof(MonoWithHitbox), true)]
    public class MonoWithHitboxEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            MonoWithHitbox authoring = (MonoWithHitbox)target;

            if (GUILayout.Button("Add Hitbox"))
            {
                authoring.Hitboxes.Add(new Hitbox() { HalfExtents = new Vector3(0.0f, 0.0f, 1.0f) });
            }

            for (int i = 0; i < authoring.Hitboxes.Count; i++)
            {
                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.LabelField($"Hitbox {i + 1}", EditorStyles.boldLabel);

                var hitbox = authoring.Hitboxes[i];

                hitbox.LocalCenter = EditorGUILayout.Vector3Field("Local Center", hitbox.LocalCenter);
                hitbox.HalfExtents = EditorGUILayout.Vector3Field("Half Extents", hitbox.HalfExtents);
                hitbox.LocalRotationEuler = EditorGUILayout.Vector3Field("Local Rotation", hitbox.LocalRotationEuler);

                authoring.Hitboxes[i] = hitbox;

                if (GUILayout.Button("Remove Hitbox"))
                {
                    authoring.Hitboxes.RemoveAt(i);
                }

                EditorGUILayout.EndVertical();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(authoring);
            }
        }
    }
}