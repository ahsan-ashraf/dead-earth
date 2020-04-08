using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorState { open, Animating, Closed }

public class SlidingDoorExample : MonoBehaviour {

    public float            SlidingDistance = 4.0f;
    public float            Duration        = 1.3f;
    public AnimationCurve   DoorCurve   = new AnimationCurve();

    private Transform   Transform;
    private Vector3     OpenPos;
    private Vector3     ClosePos;
    private DoorState   State;

    private void Start() {
        Transform   = transform;
        ClosePos    = transform.position;
        OpenPos     = ClosePos + (transform.right * SlidingDistance);
        State       = DoorState.Closed;
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space) && State != DoorState.Animating) {
            StartCoroutine(AnimateDoor(State == DoorState.open ? DoorState.Closed : DoorState.open));
        }
    }

    private IEnumerator AnimateDoor(DoorState newState) {
        State = DoorState.Animating;
        float time = 0.0f;
        Vector3 startPos = newState == DoorState.open ? ClosePos : OpenPos;
        Vector3 endPos   = newState == DoorState.open ? OpenPos : ClosePos;

        while(time <= Duration) {
            float t = time / Duration;
            transform.position = Vector3.Lerp(startPos, endPos, DoorCurve.Evaluate(t));
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        State = newState;
    }
}
