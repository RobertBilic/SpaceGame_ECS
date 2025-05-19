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
        }
    }
}