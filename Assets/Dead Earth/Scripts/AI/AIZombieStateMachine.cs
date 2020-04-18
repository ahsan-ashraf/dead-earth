using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AIZombieStateMachine concrete class.
/// State Machine used by zombie characters.
/// </summary>
public class AIZombieStateMachine : AIStateMachine {
    // Inspector Assigned.
    [SerializeField] [Range(10.0f, 360.0f)] private float   FOV             = 50.0f;
    [SerializeField] [Range(0.0f, 1.0f)]    private float   Sight           = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)]    private float   Hearing         = 1.0f;
    [SerializeField] [Range(0.0f, 1.0f)]    private float   Aggression      = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)]    private float   Intelligence    = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)]    private float   Satisfaction    = 1.0f;
    [SerializeField] [Range(0, 100)]        private int     Health          = 100;

    // Private.
    private int     Seeking     = 0;
    private bool    Feeding     = false;
    private bool    Crawling    = false;
    private int     AttackType  = 0;
    private float   Speed       = 0.0f;
    
    // Hashes.
    private int SpeedHash   = Animator.StringToHash("Speed");
    private int SeekingHash = Animator.StringToHash("Seeking");
    private int FeedingHash = Animator.StringToHash("Feeding");
    private int AttackHash  = Animator.StringToHash("Attack");

    // Properties.
    public float    fov             { get { return FOV; } }
    public float    sight           { get { return Sight; } }
    public float    hearing         { get { return Hearing; } }
    public bool     crawling        { get { return Crawling; } }
    public float    intelligence    { get { return Intelligence; } }
    public float    aggression      { get { return Aggression; }        set { Aggression    = value; } }
    public float    satisfaction    { get { return Satisfaction; }      set { Satisfaction  = value; } }
    public int      health          { get { return Health; }            set { Health        = value; } }
    public int      seeking         { get { return Seeking; }           set { Seeking       = value; } }
    public bool     feeding         { get { return Feeding; }           set { Feeding       = value; } }
    public int      attackType      { get { return AttackType; }        set { AttackType    = value; } }
    public float    speed           { get { return Speed; }             set { Speed = value; } }

    /// <summary>
    /// MonoBehaviour Callback: called once per frame.
    /// Updates parameters of animator per frame.
    /// </summary>
    protected override void Update() {
        base.Update();

        if (animator != null) {
            animator.SetFloat   (SpeedHash,     speed);
            animator.SetBool    (FeedingHash,   feeding);
            animator.SetInteger (SeekingHash,   seeking);
            animator.SetInteger (AttackHash,    attackType);
        }
    }
}
