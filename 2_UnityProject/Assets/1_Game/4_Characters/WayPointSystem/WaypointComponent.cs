using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(WaypointComponent))]
public class WaypointSystemEditor : Editor
{
    private SerializedProperty waypoints;
    private int selectedWaypointIndex;
    private bool lockY;
    private float yHeight;

    private void OnEnable()
    {
        waypoints = serializedObject.FindProperty("waypoints");
        selectedWaypointIndex = 0;
        SceneView.duringSceneGui += SceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= SceneGUI;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(waypoints, true);

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        lockY = EditorGUILayout.Toggle("Lock Y",lockY);
        if (EditorGUI.EndChangeCheck())
        {
            UpdateYHeight();
        }

        if (lockY)
        {
            EditorGUI.BeginChangeCheck();
            yHeight = EditorGUILayout.FloatField("Y Height", yHeight);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateYHeight();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void UpdateYHeight()
    {
        if (!lockY)
        {
            return;
        }

        for (int i = 0; i < waypoints.arraySize; i++)
        {
            Vector3 newPos = waypoints.GetArrayElementAtIndex(i).vector3Value;
            newPos.y = yHeight;
            waypoints.GetArrayElementAtIndex(i).vector3Value = newPos;
        }
    }

    private void SceneGUI(SceneView sceneView)
    {
        WaypointComponent waypointSystem = (WaypointComponent)target;

        Handles.color = Color.gray;

        for (int i = 0; i < waypoints.arraySize; i++)
        {
            SerializedProperty waypoint = waypoints.GetArrayElementAtIndex(i);
            Vector3 newPos = waypoint.vector3Value;
            Handles.color = Color.gray;

            if (selectedWaypointIndex == i - 1)
                Handles.color = Color.green;

            if (selectedWaypointIndex == i + 1)
                Handles.color = Color.red;

            if (selectedWaypointIndex == i)
            {
                Handles.color = Color.yellow;
                Handles.DrawWireCube(newPos, Vector3.one / 10);
                newPos = Handles.PositionHandle(newPos, Quaternion.identity);
                if (lockY)
                {
                    yHeight = newPos.y;
                    UpdateYHeight();
                    //newPos.y = yHight;
                }
                waypoint.vector3Value = newPos;
            }
            else if (Handles.Button(newPos, Quaternion.identity, 0.1f, 0.1f, Handles.CubeHandleCap))
            {
                selectedWaypointIndex = i;
            }

            if (i < waypoints.arraySize - 1)
            {
                DrawArrow(newPos, waypoints.GetArrayElementAtIndex(i+1).vector3Value);
            }

            GUIContent labelContent = new GUIContent(i.ToString());
            Vector2 labelSize = EditorStyles.whiteBoldLabel.CalcSize(labelContent);
            Vector3 labelPosition = newPos + new Vector3(labelSize.x * 0.5f / Screen.width, labelSize.y * 0.5f/ Screen.height, 0f);
            Handles.Label(labelPosition, i.ToString(), EditorStyles.whiteBoldLabel);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawArrow(Vector3 startPos, Vector3 endPos)
    {
        Vector3 direction = endPos - startPos;
        float arrowSize = 0.2f;
        float arrowAngle = 20f;

        Handles.DrawLine(startPos, endPos);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowAngle, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowAngle, 0) * Vector3.forward;

        Handles.DrawLine(endPos, endPos + right * arrowSize);
        Handles.DrawLine(endPos, endPos + left * arrowSize);
    }
}
#endif

[RequireComponent(typeof(Interactable), typeof(TriggerByCharacter))]
public class WaypointComponent : MonoBehaviour
{
    private Interactable interactable;
    public List<Vector3> waypoints;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();
        interactable.triggerEvent += StartNarrativeWalk;
    }

    private void StartNarrativeWalk(Movement movement)
    {

    }
}
