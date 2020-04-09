using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentNoRootMotionExample : MonoBehaviour {

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

    private NavMeshAgent    NavAgent            = null;
    private Animator        Animator            = null;
    private float           OriginalMaxSpeed    = 5.6f;

    #endregion
    #region MonoBehaviour Callbacks

    private void Start() {
        NavAgent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        if (NavAgent != null)
            OriginalMaxSpeed = NavAgent.speed;

        /*
        NavAgent.updatePosition = false;
        NavAgent.updateRotation = false;
        */

        if (WayPointNetwork == null)    return;

        SetNextDestination(false);
    }
    private void Update() {

        int turnOnSpot;

        HasPath             = NavAgent.hasPath;
        PathPending         = NavAgent.pathPending;
        PathStale           = NavAgent.isPathStale;
        PathStatus          = NavAgent.pathStatus;
        RemainingDistance   = NavAgent.remainingDistance;
        
        Vector3 cross = Vector3.Cross(transform.forward, NavAgent.desiredVelocity.normalized);
        float horizontal = cross.y < 0 ? -cross.magnitude : cross.magnitude;
        horizontal = Mathf.Clamp(horizontal * 2.32f, -2.32f, +2.32f);

        //float vertical = NavAgent.desiredVelocity.magnitude;
        Debug.Log((NavAgent.desiredVelocity.magnitude < 3.0f) + ": " + NavAgent.desiredVelocity.magnitude + " && "+ (Vector3.Angle(transform.forward, NavAgent.desiredVelocity) > 2.0f) + ": " + (Vector3.Angle(transform.forward, NavAgent.desiredVelocity)));
        if (NavAgent.desiredVelocity.magnitude < 3.0f && Vector3.Angle(transform.forward, NavAgent.desiredVelocity) > 2.0f) {
            NavAgent.speed  = 0.1f;
            turnOnSpot      = (int)Mathf.Sign(horizontal);
            Debug.Log(Vector3.Angle(transform.forward, NavAgent.desiredVelocity));
        } else {
            NavAgent.speed = OriginalMaxSpeed;
            turnOnSpot = 0;
            Debug.Log("Resetted because of: " + (NavAgent.desiredVelocity.magnitude < 3.0f) + ": " + NavAgent.desiredVelocity.magnitude + " && " + (Vector3.Angle(transform.forward, NavAgent.desiredVelocity) > 2.0f) + ": " + (Vector3.Angle(transform.forward, NavAgent.desiredVelocity)));
        }

        Animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        Animator.SetFloat("Vertical", /*vertical*/NavAgent.desiredVelocity.magnitude, 0.1f, Time.deltaTime);
        Animator.SetInteger("TurnOnSpot", turnOnSpot);

        /*
        if (NavAgent.isOnOffMeshLink) {
            StartCoroutine(Jump(1.0f));
            return;
        }
        */

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
