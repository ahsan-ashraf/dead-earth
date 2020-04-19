using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AIStateType     { None, Idle, Alerted, Patrol, Attack, Feeding, Pursuit, Dead } // Crawl
public enum AITargetType    { None, WayPoint, Visual_Player, Visual_Light, Visual_Food, Audio }
public enum AITriggerEventType  { Enter, Stay, Exit }

public struct AITarget {
    private AITargetType    Type;
    private Collider        Collider;
    private Vector3         Position;
    private float           Distance;
    private float           Time;

    public AITargetType type        { get { return Type; } }
    public Collider     collider    { get { return Collider; } }
    public Vector3      position    { get { return Position; } }
    public float        distance    { get { return Distance; } set { Distance = value; } }
    public float        time        { get { return Time; } }

    public void Set(AITargetType t, Collider c, Vector3 p, float d) {
        Type        = t;
        Collider    = c;
        Position    = p;
        Distance    = d;
        Time        = UnityEngine.Time.deltaTime;
    }
    public void Clear() {
        Type        = AITargetType.None;
        Collider    = null;
        Position    = Vector3.zero;
        Distance    = Mathf.Infinity;
        Time        = 0.0f;
    }
}

public abstract class AIStateMachine : MonoBehaviour {

    public AITarget VisualThreat    = new AITarget();
    public AITarget AudioThreat     = new AITarget();

    protected   Dictionary<AIStateType, AIState>    States                  = new Dictionary<AIStateType, AIState>();
    protected   AIState                             CurrentState            = null;
    protected   AITarget                            Target                  = new AITarget();
    protected   int                                 RootPositionRefCount    = 0;
    protected   int                                 RootRotationRefCount    = 0;
    protected   bool                                TargetReached           = false;

    [SerializeField] protected  AIStateType         CurrentStateType        = AIStateType.Idle;
    [SerializeField] protected  SphereCollider      TargetTrigger           = null;
    [SerializeField] protected  SphereCollider      SensorTrigger           = null;
    [SerializeField] protected  AIWayPointNetwork   WayPointNetWrok         = null;
    [SerializeField] protected  bool                RandomPatrol            = false;
    [SerializeField] protected  int                 CurrentWayPointIndex    = -1;

    [SerializeField] [Range(0.0f, 15f)] protected float     StoppingDistance = 1.0f;

    private   Animator        Animator    = null;
    private   NavMeshAgent    NavAgent    = null;
    private   Collider        Collider    = null;
    //protected   Transform       Transform   = null;

    public Animator animator {
        get {
            return Animator;
        }
        private set {
            Animator = value;
        }
    }
    public NavMeshAgent navAgent {
        get {
            return NavAgent;
        }
        private set {
            NavAgent = value;
        }
    }
    public Collider _collider {
        get {
            return Collider;
        }
        private set {
            Collider = value;
        }
    }
    public bool useRootPosition {
        get {
            return (RootPositionRefCount > 0);
        }
    }
    public bool useRootRotation {
        get {
            return (RootRotationRefCount > 0);
        }
    }
    public AITargetType targetType {
        get {
            return (Target.type);
        }
    }
    public Vector3 targetPosition {
        get {
            return Target.position;
        }
    }
    public int targetColliderId {
        get {
            if (Target.collider != null) {
                return (Target.collider.GetInstanceID());
            } else {
                return (-1);
            }
        }
    }
    public bool targetReached {
        get {
            return (TargetReached);
        }
        private set {
            TargetReached = value;
        }
    }
    public bool inMeleeRange { get; set; }
    public Vector3 sensorPosition {
        get {
            if (SensorTrigger == null)  return (Vector3.zero);
            Vector3 point = SensorTrigger.transform.position;
            point.x = SensorTrigger.center.x * SensorTrigger.transform.lossyScale.x;
            point.y = SensorTrigger.center.y * SensorTrigger.transform.lossyScale.y;
            point.z = SensorTrigger.center.z * SensorTrigger.transform.lossyScale.z;
            return (point);
        }
    }
    public float sensorRadius {
        get {
            if (SensorTrigger == null)  return (0.0f);
            return Mathf.Max(SensorTrigger.radius * SensorTrigger.transform.lossyScale.x,
                             SensorTrigger.radius * SensorTrigger.transform.lossyScale.y,
                             SensorTrigger.radius * SensorTrigger.transform.lossyScale.z);
        }
    }

    /// <summary>
    /// MonoBehaviour Callback: used for referencing
    /// </summary>
    protected virtual void Awake() {
        // Referencing Components
        animator    = GetComponent<Animator>();
        navAgent    = GetComponent<NavMeshAgent>();
        _collider   = GetComponent<Collider>();

        if (GameSceneManager.Instance != null) {
            // Register State Machines to GameSceneManager
            if (Collider != null)       GameSceneManager.Instance.RegisterStateMachine(Collider.GetInstanceID(), this);
            if (SensorTrigger != null)  GameSceneManager.Instance.RegisterStateMachine(SensorTrigger.GetInstanceID(), this);
        }
    }
    /// <summary>
    /// MonoBehaviour Callback: called once before first frame.
    /// </summary>
    protected virtual void Start() {
        if (SensorTrigger != null) {
            AISensor script = SensorTrigger.GetComponent<AISensor>();
            if (script != null) {
                script.parentStateMachine = this;
            }
        }

        // Fetch all states on this gameObject and add them into the States Dictionary.
        AIState[] aiStates = GetComponents<AIState>();
        foreach (AIState state in aiStates) {
            if (state != null && !States.ContainsKey(state.GetStateType())) {
                States.Add(state.GetStateType(), state);
                state.SetStateMachine(this);
            }
        }
        // Setting up the CurrentState of AIStateMachine.
        if (States.ContainsKey(CurrentStateType)) {
            CurrentState = States[CurrentStateType];
            CurrentState.OnEnterState();
        } else {
            CurrentState = null;
        }

        if (animator != null) {
            AIStateMachineLink[] scripts = animator.GetBehaviours<AIStateMachineLink>();
            foreach (AIStateMachineLink link in scripts) {
                link.stateMachine = this;
            }
        }
    }
    /// <summary>
    /// MonoBehaviour Callback: called once per frame.
    /// </summary>
    protected virtual void Update() {
        if (CurrentState == null)   return;

        AIStateType newStateType = CurrentState.OnUpdate();
        if (newStateType != CurrentStateType) {
            AIState newState = null;
            if (States.TryGetValue(newStateType, out newState)) {
                TransitionToState(newState);
            } else if (States.TryGetValue(AIStateType.Idle, out newState)) {
                TransitionToState(newState);
            }
        }
        CurrentStateType = newStateType;
    }
    /// <summary>
    /// MonoBehaviour Callback: called once per physics frame update.
    /// </summary>
    protected virtual void FixedUpdate() {
        VisualThreat.Clear();
        AudioThreat.Clear();

        if (Target.type != AITargetType.None) {
            Target.distance = Vector3.Distance(transform.position, Target.position);
        }

        targetReached = false;
    }
    /// <summary>
    /// MonoBehavior Callback: called by physics when the AI's Main collider enters its trigger. 
    /// This allows the child state to know when it has entered the sphere of influence of a 
    /// waypoint or last sighted player.
    /// </summary>
    /// <param name="other"></param>
    protected virtual void OnTriggerEnter(Collider other) {
        if (TargetTrigger == null || other != TargetTrigger)    return;

        targetReached = true;

        // Notify child state.
        if (CurrentState != null) {
            CurrentState.OnDestinationReached(true);
        }
    }
    protected virtual void OnTriggerStay(Collider other) {
        if (TargetTrigger == null || other != TargetTrigger)    return;

        targetReached = true;
    }
    /// <summary>
    /// MonoBehavior Callback: called by physics when the AI's Main collider is no longer at its
    /// destination. i.e. typically called when a new target has been set by the child.
    /// </summary>
    /// <param name="other"></param>
    protected virtual void OnTriggerExit(Collider other) {
        if (TargetTrigger == null || other != TargetTrigger)    return;

        targetReached = false;

        // Notify child state.
        if (CurrentState != null) {
            CurrentState.OnDestinationReached(false);
        }
    }
    /// <summary>
    /// Animator Callback: called after root motion has been calculated but not applied to the 
    /// gameObject. Allowing us to determine what to do with the rotation and position information.
    /// </summary>
    protected virtual void OnAnimatorMove() {
        if (CurrentState != null) {
            CurrentState.OnAnimatorUpdated();
        }
    }
    /// <summary>
    /// Animator Callback: called just before the IK system being 
    /// updated giving us a chance to setup IK Targets and Weights.
    /// </summary>
    /// <param name="layerIndex"></param>
    protected virtual void OnAnimatorIK(int layerIndex) {
        if (CurrentState != null) {
            CurrentState.OnAnimatorIKUpdated();
        }
    }
    /// <summary>
    /// Transit the CurrentState to the provided state.
    /// </summary>
    /// <param name="newState"></param>
    private void TransitionToState(AIState newState) {
        CurrentState.OnExitState();
        newState.OnEnterState();
        CurrentState = newState;
    }
    /// <summary>
    /// Sets the Target of the AIStateMachine.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="c"></param>
    /// <param name="p"></param>
    /// <param name="d"></param>
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d) {
        Target.Set(t, c, p, d);
        if (TargetTrigger != null) {
            TargetTrigger.radius                = StoppingDistance;
            TargetTrigger.transform.position    = Target.position;
            TargetTrigger.enabled               = true;
        }
    }
    /// <summary>
    /// Overloaded version of SetTarget function.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="c"></param>
    /// <param name="p"></param>
    /// <param name="d"></param>
    /// <param name="s"></param>
    public void SetTarget(AITargetType t, Collider c, Vector3 p, float d, float s) {
        Target.Set(t, c, p, d);
        if (TargetTrigger != null) {
            TargetTrigger.radius                = s;
            TargetTrigger.transform.position    = Target.position;
            TargetTrigger.enabled               = true;
        }
    }
    /// <summary>
    /// /// Overloaded version of SetTarget function.
    /// </summary>
    /// <param name="t"></param>
    public void SetTarget(AITarget t) {
        Target = t;
        if (TargetTrigger != null) {
            TargetTrigger.radius = StoppingDistance;
            TargetTrigger.transform.position = Target.position;
            TargetTrigger.enabled = true;
        }
    }
    /// <summary>
    /// Clears the current target of AIStateMachine.
    /// </summary>
    public void ClearTarget() {
        Target.Clear();
        if (TargetTrigger != null) {
            TargetTrigger.enabled = false;
        }
    }
    /// <summary>
    /// Called by our AISensor component when an AI Aggravator has entered/exited the sensor trigger.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="other"></param>
    public virtual void OnTriggerEvent(AITriggerEventType type, Collider other) {
        if (CurrentState != null) {
            CurrentState.OnTriggerEvent(type, other);
        }
    }
    /// <summary>
    /// Updates the NavMeshAgent position and rotation controlls settings.
    /// </summary>
    /// <param name="positionUpdate"></param>
    /// <param name="rotationUpdate"></param>
    public void NavAgentControl(bool positionUpdate, bool rotationUpdate) {
        if (NavAgent != null) {
            NavAgent.updatePosition = positionUpdate;
            NavAgent.updateRotation = rotationUpdate;
        }
    }
    /// <summary>
    /// Called by State Machine Behaviour to Enable or Disable root motion.
    /// </summary>
    /// <param name="rootPosition"></param>
    /// <param name="rootRotation"></param>
    public void AddRootMotionRequest(int rootPosition, int rootRotation) {
        RootPositionRefCount += rootPosition;
        RootRotationRefCount += rootRotation;
    }
    public Vector3 GetWayPointPosition(bool increment) {
        if (CurrentWayPointIndex == -1) {
            if (RandomPatrol) {
                CurrentWayPointIndex = Random.Range(0, WayPointNetWrok.WayPoints.Count);
            } else {
                CurrentWayPointIndex = 0;
            }
        } else if (increment) {
            UpdateWayPoint();
        }
        if (WayPointNetWrok.WayPoints[CurrentWayPointIndex] != null) {
            Transform newWayPoint = WayPointNetWrok.WayPoints[CurrentWayPointIndex];
            SetTarget(AITargetType.WayPoint, null, newWayPoint.position, Vector3.Distance(transform.position, newWayPoint.position));
            return (newWayPoint.position);
        }
        return (Vector3.zero);
    }
    /// <summary>
    /// Called to select a new waypoint. Either randomly selects a new waypoint from the waypoint network or increments the current
    /// waypoint index (with wrap-around) to visit the waypoints in the network in sequence. Sets the new waypoint as the the
    /// target and generates a nav agent path for it.
    /// </summary>
    private void UpdateWayPoint() {
        // If its a random patrol then select a random index.
        if (RandomPatrol && WayPointNetWrok.WayPoints.Count > 1) {
            int oldWayPoint = CurrentWayPointIndex;
            while (CurrentWayPointIndex == oldWayPoint) {
                CurrentWayPointIndex = Random.Range(0, WayPointNetWrok.WayPoints.Count);
            }
        } else {
            CurrentWayPointIndex = CurrentWayPointIndex == WayPointNetWrok.WayPoints.Count - 1 ? 0 : CurrentWayPointIndex + 1;
        }
    }
}
