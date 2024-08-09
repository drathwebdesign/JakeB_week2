using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAnimator : MonoBehaviour
{

    private const string IS_WALKING = "IsWalking";
    private const string IS_RUNNING = "IsRunning";

    private ZombieAI zombieAI;
    private Animator animator;
    // Start is called before the first frame update
    private void Awake()
    {
        animator = GetComponent<Animator>();
        zombieAI = GetComponent<ZombieAI>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool(IS_WALKING, zombieAI.IsWalking());
        animator.SetBool(IS_RUNNING, zombieAI.IsRunning());
    }
}