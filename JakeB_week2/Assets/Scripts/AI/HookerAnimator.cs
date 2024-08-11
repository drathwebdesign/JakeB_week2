using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookerAnimator : MonoBehaviour
{
    private const string IS_WALKING = "IsWalking";
    private const string IS_RUNNING = "IsRunning";

    private HookerAI hookerAI;
    private Animator animator;
    // Start is called before the first frame update
    private void Awake() {
        animator = GetComponent<Animator>();
        hookerAI = GetComponent<HookerAI>();
    }

    // Update is called once per frame
    void Update() {
        animator.SetBool(IS_WALKING, hookerAI.IsWalking());
        animator.SetBool(IS_RUNNING, hookerAI.IsRunning());
    }
}
