using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedController_Test : MonoBehaviour {

    public float    Speed = 0.0f;

    private Animator AnimCtrl;

    private void Start() {
        AnimCtrl = GetComponent<Animator>();
    }

    private void Update() {
        AnimCtrl.SetFloat("Speed", Speed);
    }
}
