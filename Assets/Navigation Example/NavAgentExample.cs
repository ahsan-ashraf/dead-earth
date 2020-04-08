using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour {

    #region Public Variables

    public AIWayPointNetwork    WayPointNetwork      = null;
    public int                  CurrentWayPointIndex = 0;
    public bool                 HasPath              = false;
    public bool                 PathPending          = false;
    public bool                 PathStale            = false;
    public float                RemainingDistance    = 0.0f;
    public NavMeshPathStatus    PathStatus           = NavMeshPathStatus.PathInvalid;
    public AnimationCurve       JumpCurve            = new AnimationCurve();

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
        HasPath             = NavAgent.hasPath;
        PathPending         = NavAgent.pathPending;
        PathStale           = NavAgent.isPathStale;
        PathStatus          = NavAgent.pathStatus;
        RemainingDistance   = NavAgent.remainingDistance;

        if (NavAgent.isOnOffMeshLink) {
            StartCoroutine(Jump(1.0f));
            return;
        }

        if ((RemainingDistance <= NavAgent.stoppingDistance && !PathPending) || PathStatus == NavMeshPathStatus.PathInvalid || PathStatus == NavMeshPathStatus.PathPartial) {
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
    private IEnumerator Jump(float duration) {
        OffMeshLinkData linkData    =   NavAgent.currentOffMeshLinkData;
        Vector3         startpos    =   NavAgent.transform.position;
        Vector3         endPos      =   linkData.endPos + NavAgent.baseOffset * Vector3.up;
        float           time        =   0.0f;

        while (time <= duration) {
            float t = time / duration;
            NavAgent.transform.position = Vector3.Lerp(startpos, endPos, t) + JumpCurve.Evaluate(t) * Vector3.up;
            time += Time.deltaTime;
            yield return null;
        }
        NavAgent.CompleteOffMeshLink();
    }

    #endregion

}
