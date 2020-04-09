using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Test : MonoBehaviour {

    private Animator    Anim;
    private int         HorizontalHash;
    private int         VerticalHash;
    private int         AttackHash;

    private void Start() {
        Anim            = GetComponent<Animator>();
        HorizontalHash  = Animator.StringToHash("Horizontal");
        VerticalHash    = Animator.StringToHash("Vertical");
        AttackHash      = Animator.StringToHash("Attack");
    }

    private void Update() {
        float x = Input.GetAxis("Horizontal") * 2.32f;
        float y = Input.GetAxis("Vertical") * 5.667774f;
        if (Input.GetButtonDown("Fire1")) Anim.SetTrigger(AttackHash);
        Anim.SetFloat(HorizontalHash, x, 0.35f, Time.deltaTime);
        Anim.SetFloat(VerticalHash, y, 1.0f, Time.deltaTime);
    }
}
