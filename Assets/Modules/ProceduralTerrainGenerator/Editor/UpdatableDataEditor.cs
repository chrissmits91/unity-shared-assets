using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        UpdatableData data = (UpdatableData)target;

        if (GUILayout.Button("Update"))
        {
            data.NotifyOfUpdatedValues();
        }
    }
}
