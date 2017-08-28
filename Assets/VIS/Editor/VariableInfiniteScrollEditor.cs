using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using VIS;

[CustomEditor(typeof(VariableInfiniteScroll))]
public class VariableInfiniteScrollEditor : ScrollRectEditor
{
    public override void OnInspectorGUI()
    {
        VariableInfiniteScroll myTarget = (VariableInfiniteScroll)target;

        myTarget.contentItem = EditorGUILayout.ObjectField("ContentItem", myTarget.contentItem, typeof(RectTransform), true) as RectTransform;

        myTarget.instantateItemCount = (int)EditorGUILayout.Slider("Instantate Item Coun", myTarget.instantateItemCount, 1, 30);
        base.OnInspectorGUI();
    }
}
