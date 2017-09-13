using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using Antibody;

namespace AntibodyEditor
{
    [CustomEditor(typeof(InfiniteScroll))]
    public class InfiniteScrollEditor : ScrollRectEditor
    {
        private bool isButton;
        public override void OnInspectorGUI()
        {
            InfiniteScroll myTarget = (InfiniteScroll)target;
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_itemBase"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_autoStart"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_instantateItemCount"));
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();
            if (GUILayout.Button("ScrollRect Inspector", GUILayout.ExpandWidth(true), GUILayout.Height(30f)))
            {
                isButton = !isButton;
            }
            if (isButton)
            {
                EditorGUILayout.Space();
                base.OnInspectorGUI();
            }
        }
    }
}