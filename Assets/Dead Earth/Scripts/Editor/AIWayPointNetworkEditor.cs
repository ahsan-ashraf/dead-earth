using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[CustomEditor(typeof(AIWayPointNetwork))]
public class AIWayPointNetworkEditor : Editor {

    #region Private Variables

    private AIWayPointNetwork Network;

    #endregion
    #region Editor Callbacks

    private void OnSceneGUI() {
        Network = (AIWayPointNetwork)target;
        DisplayLabels();
        switch (Network.DisplayMode) {
            case PathDisplayMode.Connections:
                DisplayConnections();
                break;
            case PathDisplayMode.Paths:
                DisplayPath();
                break;
        }
        
    }
    public override void OnInspectorGUI() {
        Network.DisplayMode = (PathDisplayMode)EditorGUILayout.EnumPopup("Display Mode", Network.DisplayMode);
        if (Network.DisplayMode == PathDisplayMode.Paths) {
            Network.UIStart = EditorGUILayout.IntSlider("UIStart", Network.UIStart, 0, Network.WayPoints.Count - 1);
            Network.UIEnd   = EditorGUILayout.IntSlider("UIEnd", Network.UIEnd, 0, Network.WayPoints.Count - 1);
        }
        DrawDefaultInspector();
    }

    #endregion
    #region Private Methods

    private void DisplayLabels() {
        GUIStyle style          = new GUIStyle();
        style.normal.textColor  = Color.white;
        for (int i = 0; i < Network.WayPoints.Count; i++) {
            if (Network.WayPoints[i] != null) {
                Handles.Label(Network.WayPoints[i].position, "WayPoint-" + i.ToString(), style);
            }
        }
    }
    private void DisplayConnections() {
        Vector3[] linePoints = new Vector3[Network.WayPoints.Count + 1];
        for (int i = 0; i <= Network.WayPoints.Count; i++) {
            int index = i != Network.WayPoints.Count ? i : 0;
            if (Network.WayPoints[index] != null) {
                linePoints[i] = Network.WayPoints[index].position;
            } else {
                linePoints[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            }
        }
        Handles.color = Color.cyan;
        Handles.DrawPolyLine(linePoints);
    }
    private void DisplayPath() {
        NavMeshPath path            = new NavMeshPath();
        Vector3     sourcePosition  = Network.WayPoints[Network.UIStart].position;
        Vector3     targetPosition  = Network.WayPoints[Network.UIEnd].position;
        NavMesh.CalculatePath(sourcePosition, targetPosition, NavMesh.AllAreas, path);
        Handles.color = Color.yellow;
        Handles.DrawPolyLine(path.corners);
    }

    #endregion

}
