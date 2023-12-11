using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        Vector3 lastVectorEntry = (Vector3)waypoints.GetArrayElementAtIndex(waypoints.arraySize - 1).vector4Value;
        Vector3 forLastVectorEntry;
        if (waypoints.arraySize > 1)
        {
            forLastVectorEntry = (Vector3)waypoints.GetArrayElementAtIndex(waypoints.arraySize - 2).vector4Value;
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
            Vector3 newPos = (Vector3)waypoints.GetArrayElementAtIndex(i).vector4Value;
            newPos.y = yHeight;
            waypoints.GetArrayElementAtIndex(i).vector4Value = VectorHelper.Convert3To4(newPos, waypoints.GetArrayElementAtIndex(i).vector4Value.w);
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

            waypoints.GetArrayElementAtIndex(index).vector4Value = VectorHelper.Convert3To4(EditorGUILayout.Vector3Field("Element " + index, (Vector3)waypoints.GetArrayElementAtIndex(index).vector4Value), waypoints.GetArrayElementAtIndex(index).vector4Value.w);

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
                Vector3 newPos = (Vector3)waypoint.vector4Value + targetObject.transform.position;
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
                        if (lockY && newPos.y != waypoint.vector4Value.y)
                        {
                            yHeight = newPos.y;
                            UpdateYHeight();
                        }
                        waypoint.vector4Value = VectorHelper.Convert3To4(newPos - targetObject.transform.position, waypoint.vector4Value.w);
                    }
                }
                else if (Handles.Button(newPos, Quaternion.identity, 0.2f, 0.2f, Handles.CubeHandleCap))
                {
                    selectedWaypointIndex = i;
                }

                if (i < waypoints.arraySize - 1)
                {
                    DrawArrow(newPos, (Vector3)waypoints.GetArrayElementAtIndex(i + 1).vector4Value + targetObject.transform.position);
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

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
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
            points[i] = (Vector3)waypoints.GetArrayElementAtIndex(i).vector4Value + targetObject.transform.position;
        }

        // Draw the first Bézier curve
        Vector3 startTanget = points[0] + VectorHelper.FromTo(points[0], points[1]) * bezierInfluence.floatValue * 0.1f * Vector3.Distance(points[0], points[1]);
        Vector3 endTangent = points[1] - VectorHelper.FromTo(points[0], points[2]) * bezierInfluence.floatValue * 0.1f * Vector3.Distance(points[0], points[1]);
        //Handles.DrawBezier(points[0], points[1], startTanget, endTangent, Color.cyan, null, 5f);

        // Draw subsequent Bézier curves
        for (int i = 0; i < numWaypoints - 2; i++)
        {
            startTanget = points[i] + VectorHelper.FromTo(points[Mathf.Max(i-1,0)], points[i+1]) * bezierInfluence.floatValue * 0.1f * Vector3.Distance(points[i], points[i+1]);
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
    public List<Vector4> waypoints;
    public float bezierInfluence = 3;
    public float bezierLength;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();
        interactable.triggerEvent += StartNarrativeWalk;

        CalculateLength();
        Debug.Log(bezierLength);
    }

    private void StartNarrativeWalk(Movement movement)
    {

    }

    private void CalculateLength()
    {
        bezierLength = 0;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector3 start = waypoints[i];
            Vector3 end = waypoints[i + 1];
            Vector3 startTangent = GetTangentVector(i, true);
            Vector3 endTangent = GetTangentVector(i + 1, false);

            Vector4 temp = waypoints[i];
            temp.w = ApproximateBezierCurveLength(start, startTangent, endTangent, end);
            bezierLength += temp.w;
            waypoints[i] = temp;
        }
    }

    private float ApproximateBezierCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Use a simple linear approximation for the curve length
        int numSegments = 10; // Increase for higher accuracy
        float segmentLength = 1f / numSegments;
        float length = 0f;

        for (int i = 0; i < numSegments; i++)
        {
            float t0 = i * segmentLength;
            float t1 = (i + 1) * segmentLength;

            Vector3 point0 = BezierCurvePoint(t0, p0, p1, p2, p3);
            Vector3 point1 = BezierCurvePoint(t1, p0, p1, p2, p3);

            length += Vector3.Distance(point0, point1);
        }

        return length;
    }

    private Vector3 GetTangentVector(int index, bool startPoint)
    {
        Vector3 baseVector = (Vector3)waypoints[index];
        Vector3 tangentNormalized = VectorHelper.FromTo(waypoints[Mathf.Max(index - 1, 0)], waypoints[index + 1 >= waypoints.Count ? index : index + 1]);
        Vector3 addition = tangentNormalized * bezierInfluence * 0.1f * Vector3.Distance(waypoints[index], waypoints[index + (startPoint ? 1 : -1)]);
        return baseVector + addition * (startPoint ? 1 : -1);
    }

    private int GetStartIndex(float t, out float remainingLength)
    {
        float targetLength = bezierLength * t;
        float length = 0;
        int index = 0;

        while (index < waypoints.Count - 1)
        {
            if (length + waypoints[index].w > targetLength)
            {
                remainingLength = targetLength - length;
                return index;
            }
            length += waypoints[index].w;
            index++;
        }

        Debug.LogError("Failed to get targetLength");
        remainingLength = 0;
        return 0;
    }

    public Vector3 GetPointOnBezierCurve(float t)
    {
        t = t % 1;

        int numWaypoints = waypoints.Count;

        if (numWaypoints < 2)
        {
            Debug.LogError("Not enough waypoints to form a Bézier curve.");
            return Vector3.zero;
        }

        int startIndex = GetStartIndex(t, out float remainingLength);

        t = remainingLength / waypoints[startIndex].w;

        Vector3 pStart = waypoints[startIndex];
        Vector3 pEnd = waypoints[startIndex + 1];
        Vector3 pStartTangent = GetTangentVector(startIndex, true);
        Vector3 pEndTangent = GetTangentVector(startIndex + 1, false);

        return BezierCurvePoint(t, pStart, pStartTangent, pEndTangent, pEnd) + transform.position;
    }

    private Vector3 BezierCurvePoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // (1-t)^3 * P0
        p += 3f * uu * t * p1; // 3 * (1-t)^2 * t * P1
        p += 3f * u * tt * p2; // 3 * (1-t) * t^2 * P2
        p += ttt * p3; // t^3 * P3

        return p;
    }

    private Vector3 pos;
    private Vector3 posd;

    private void OnDrawGizmos()
    {
        if (pos != null)
        {
            Gizmos.DrawWireSphere(pos, 0.5f);
        }
        if (posd != null)
        {
            Gizmos.DrawWireSphere(posd, 0.5f);
        }
    }

    private float ti = 0;

    private void Update()
    {
        pos = GetPointOnBezierCurve(ti / 10);
        posd = new Vector3(ti % 10,0, 0);
        ti += Time.deltaTime;
    }
}
