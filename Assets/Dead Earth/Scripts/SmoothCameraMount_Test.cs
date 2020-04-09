using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCameraMount_Test : MonoBehaviour {

    public Transform    Mount;
    public float        Speed = 3.0f;

    private void Start() {
        
    }

    private void LateUpdate() {
        transform.position  =   Vector3.Lerp(transform.position, Mount.position, Time.deltaTime * Speed);
        transform.rotation  =   Quaternion.Slerp(transform.rotation, Mount.rotation, Time.deltaTime * Speed);
    }
}
