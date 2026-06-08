using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Peg))]
[CanEditMultipleObjects]
public class PegEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(8);

        var label = targets.Length > 1
            ? $"Connect Neighbors (선택된 {targets.Length}개)"
            : "Connect Neighbors";

        if (GUILayout.Button(label, GUILayout.Height(28)))
            ConnectNeighbors();
    }

    private void ConnectNeighbors()
    {
        var allPegs = FindObjectsByType<Peg>(FindObjectsSortMode.None);

        foreach (var t in targets)
        {
            var so = new SerializedObject(t);
            so.Update();

            var rangeProp = so.FindProperty("_neighborRange");
            var neighborsProp = so.FindProperty("_neighbors");
            var peg = (Peg)t;

            neighborsProp.ClearArray();

            foreach (var other in allPegs)
            {
                if (other == peg) continue;

                var dist = Vector2.Distance(peg.transform.position, other.transform.position);
                if (dist > rangeProp.floatValue) continue;

                int idx = neighborsProp.arraySize;
                neighborsProp.InsertArrayElementAtIndex(idx);
                neighborsProp.GetArrayElementAtIndex(idx).objectReferenceValue = other;
            }

            so.ApplyModifiedProperties();
        }
    }
}
