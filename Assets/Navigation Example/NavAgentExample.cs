using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour {

    #region Public Variables

    public AIWayPointNetwork   WayPointNetwork      = null;
    public int                 CurrentWayPointIndex = 0;
    public bool                HasPath              = false;
    public bool                PathPending          = false;
    public bool                PathStale            = false;
    public NavMeshPathStatus   PathStatus           = NavMeshPathStatus.PathInvalid;

    #endregion
    #region Private Variables

    private NavMeshAgent       NavAgent             = null;

    #endregion
    #region MonoBehaviour Callbacks

    private void Start() {
        NavAgent = GetComponent<NavMeshAgent>();

        if (WayPointNetwork == null)    return;

        SetNextDestination(false);
    }
    private void Update() {
        HasPath     = NavAgent.hasPath;
        PathPending = NavAgent.pathPending;
        PathStale   = NavAgent.isPathStale;
        PathStatus  = NavAgent.pathStatus;

        if ((!HasPath && !PathPending) || PathStatus == NavMeshPathStatus.PathInvalid || PathStatus == NavMeshPathStatus.PathPartial) {
            SetNextDestination(true);
        } else if (NavAgent.isPathStale) {
            SetNextDestination(false);
        }
    }

    #endregion
    #region Private Methods

    private void SetNextDestination(bool increment) {
        if (WayPointNetwork == null)    return;
        
        int         incStep                 = increment ? 1 : 0;
        int         nextWayPointIndex       = (CurrentWayPointIndex + incStep >= WayPointNetwork.WayPoints.Count) ? 0 : CurrentWayPointIndex + incStep;
        Transform   nextWayPointTransform   = WayPointNetwork.WayPoints[nextWayPointIndex];
        
        if (nextWayPointTransform != null) {
            CurrentWayPointIndex = nextWayPointIndex;
            NavAgent.SetDestination(nextWayPointTransform.position);
            return;
        }

        CurrentWayPointIndex += 1;
    }

    #endregion
}
