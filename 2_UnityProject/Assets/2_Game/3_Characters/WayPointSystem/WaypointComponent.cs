using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(WaypointComponent))]
public class WaypointSystemEditor : Editor
{
    private SerializedProperty waypoints;
    private SerializedProperty bezierInfluence;
    private int selectedWaypointIndex;
    private bool lockY;
    private float yHeight;
    private bool equalBezierHandles;

    private GameObject targetObject;

    private void OnEnable()
    {
        waypoints = serializedObject.FindProperty("waypoints");
        bezierInfluence = serializedObject.FindProperty("bezierInfluence");
        selectedWaypointIndex = 0;
        SceneView.duringSceneGui += SceneGUI;

        targetObject = ((WaypointComponent)target).gameObject;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= SceneGUI;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Waypoints:");
        for (int w = 0; w < waypoints.arraySize; w++)
        {
            ElementView(w);
        }
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("+ Add Element"))
        {
            AddElement();
        }
        GUILayout.EndHorizontal();

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

        EditorGUILayout.PropertyField(bezierInfluence, true);

        equalBezierHandles = EditorGUILayout.Toggle("Equal Bezier Handles", equalBezierHandles);

        serializedObject.ApplyModifiedProperties();
    }

    private void AddElement()
    {
        Vector3 lastVectorEntry = waypoints.GetArrayElementAtIndex(waypoints.arraySize - 1).vector3Value;
        Vector3 forLastVectorEntry;
        if (waypoints.arraySize > 1)
        {
            forLastVectorEntry = waypoints.GetArrayElementAtIndex(waypoints.arraySize - 2).vector3Value;
        }
        else
        {
            forLastVectorEntry = Vector3.zero;
        }
        ((WaypointComponent)target).waypoints.Add(lastVectorEntry + VectorHelper.FromTo(forLastVectorEntry, lastVectorEntry));
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

    private GUIStyle GetDarkBackgroundStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.box);
        Color[] pix = new Color[2 * 2];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = new Color(0.2f,0.2f,0.2f, 1f);
        }
        Texture2D result = new Texture2D(2, 2);
        result.SetPixels(pix);
        result.Apply();
        style.normal.background = result;
        return style;
    }

    private void ElementView(int index)
    {
        EditorGUILayout.BeginHorizontal(GetDarkBackgroundStyle());

            if (GUILayout.Button("Select", GUILayout.Width(50)))
            {
                selectedWaypointIndex = index;
            }

            waypoints.GetArrayElementAtIndex(index).vector3Value = EditorGUILayout.Vector3Field("Element " + index,waypoints.GetArrayElementAtIndex(index).vector3Value);

        if (index != 0)
        {
            if (GUILayout.Button("<", GUILayout.Width(25)))
            {
                waypoints.MoveArrayElement(index - 1, index);
            }
        }
        else
        {
            GUILayout.Space(28);
        }
        if (index != waypoints.arraySize - 1)
        {
            if (GUILayout.Button(">", GUILayout.Width(25)))
            {
                waypoints.MoveArrayElement(index + 1, index);
            }
        }
        else
        {
            GUILayout.Space(28);
        }

            if (index != 0)
            {
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    waypoints.DeleteArrayElementAtIndex(index);
                }
            }
            else
            {
                GUILayout.Space(28);
            }

        EditorGUILayout.EndHorizontal();
    }

    private void SceneGUI(SceneView sceneView)
    {
        WaypointComponent waypointSystem = (WaypointComponent)target;

        Handles.color = Color.gray;

        if (!Keyboard.current.shiftKey.isPressed)
        {
            for (int i = 0; i < waypoints.arraySize; i++)
            {
                SerializedProperty waypoint = waypoints.GetArrayElementAtIndex(i);
                Vector3 newPos = waypoint.vector3Value + targetObject.transform.position;
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

                    if (i == 0)
                    {
                        targetObject.transform.position = newPos;
                    }
                    else
                    {
                        if (lockY && newPos.y != waypoint.vector3Value.y)
                        {
                            yHeight = newPos.y;
                            UpdateYHeight();
                        }
                        waypoint.vector3Value = newPos - targetObject.transform.position;
                    }
                }
                else if (Handles.Button(newPos, Quaternion.identity, 0.2f, 0.2f, Handles.CubeHandleCap))
                {
                    selectedWaypointIndex = i;
                }

                if (i < waypoints.arraySize - 1)
                {
                    DrawArrow(newPos, waypoints.GetArrayElementAtIndex(i + 1).vector3Value + targetObject.transform.position);
                }

                GUIContent labelContent = new GUIContent(i.ToString());
                Vector2 labelSize = EditorStyles.whiteBoldLabel.CalcSize(labelContent);
                Vector3 labelPosition = newPos + new Vector3(labelSize.x * 0.5f / Screen.width, labelSize.y * 0.5f / Screen.height, 0f);
                Handles.Label(labelPosition, i.ToString(), EditorStyles.whiteBoldLabel);
            }
        }

        DrawBezierCurve(waypoints);

        if (Keyboard.current.deleteKey.isPressed)
        {
            waypoints.DeleteArrayElementAtIndex(selectedWaypointIndex);
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            Debug.Log("SPACE");
            AddElement();
            selectedWaypointIndex = waypoints.arraySize;
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

    private void DrawBezierCurve(SerializedProperty waypoints)
    {
        Handles.color = Color.cyan;

        int numWaypoints = waypoints.arraySize;

        if (numWaypoints < 3)
        {
            // Not enough waypoints to form a Bézier curve
            return;
        }

        Vector3[] points = new Vector3[numWaypoints];

        for (int i = 0; i < numWaypoints; i++)
        {
            points[i] = waypoints.GetArrayElementAtIndex(i).vector3Value + targetObject.transform.position;
        }

        // Draw the first Bézier curve
        Vector3 startTanget = points[0] + VectorHelper.FromTo(points[0], points[1]) * bezierInfluence.floatValue * 0.1f * Vector3.Distance(points[0], points[1]);
        Vector3 endTangent = points[1] - VectorHelper.FromTo(points[0], points[2]) * bezierInfluence.floatValue * 0.1f * Vector3.Distance(points[0], points[1]);
        Handles.DrawBezier(points[0], points[1], startTanget, endTangent, Color.cyan, null, 5f);

        // Draw subsequent Bézier curves
        for (int i = 1; i < numWaypoints - 2; i++)
        {
            startTanget = points[i] + VectorHelper.FromTo(points[i-1], points[i+1]) * bezierInfluence.floatValue * 0.1f * Vector3.Distance(points[i], points[i+1]);
            endTangent = points[i +1] - VectorHelper.FromTo(points[i], points[i+2]) * bezierInfluence.floatValue * 0.1f * Vector3.Distance(points[ equalBezierHandles ? i + 1 : i], points[equalBezierHandles ? i + 2 : i + 1]);
            Handles.DrawBezier(points[i], points[i+1], startTanget, endTangent, Color.cyan, null, 5f);
        }

        // Draw the last Bézier curve
        startTanget = points[numWaypoints - 2] + VectorHelper.FromTo(points[numWaypoints - 3], points[numWaypoints - 1]) * bezierInfluence.floatValue * 0.1f * Vector3.Distance(points[equalBezierHandles ? numWaypoints - 3 : numWaypoints - 2], points[equalBezierHandles ? numWaypoints - 2 : numWaypoints - 1]);
        endTangent = points[numWaypoints - 1] - VectorHelper.FromTo(points[numWaypoints - 2], points[numWaypoints - 1]) * bezierInfluence.floatValue * 0.1f * Vector3.Distance(points[numWaypoints - 2], points[numWaypoints - 1]);
        Handles.DrawBezier(points[numWaypoints-2], points[numWaypoints -1], startTanget, endTangent, Color.cyan, null, 5f);
    }
}

[RequireComponent(typeof(Interactable), typeof(TriggerByCharacter))]
public class WaypointComponent : MonoBehaviour
{
    private Interactable interactable;
    public List<Vector3> waypoints;
    public float bezierInfluence = 3;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();
        interactable.triggerEvent += StartNarrativeWalk;
    }

    private void StartNarrativeWalk(Movement movement)
    {

    }
}
