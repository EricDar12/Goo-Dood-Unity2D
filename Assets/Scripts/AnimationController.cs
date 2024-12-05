using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {

    private float xVelocity;
    private float yVelocity;
    private bool isJumping;
    private bool isDashing;

    private Animator animator;
    private PlayerMovement playerMovement;
    private DeathHandler.PlayerState previousState = DeathHandler.PlayerState.Alive;
    
    void Start() {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update() {
        UpdateAnimationParameters();
        UpdateDeathAnimationState();
    }

    private void UpdateAnimationParameters() {
        xVelocity = playerMovement.xVelocity;
        yVelocity = playerMovement.yVelocity;
        isJumping = playerMovement.isJumping;
        isDashing = playerMovement.isDashing;

        animator.SetFloat("xVelocity", Mathf.Abs(xVelocity));
        animator.SetFloat("yVelocity", yVelocity);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isDashing", isDashing);
    }

    private void UpdateDeathAnimationState() {

        if (DeathHandler.CurrentState != previousState) {

            switch (DeathHandler.CurrentState) {

                case DeathHandler.PlayerState.Dying:
                    animator.SetTrigger("Die");
                    break;

                case DeathHandler.PlayerState.Respawning:
                    animator.SetTrigger("Respawn");
                    break;
            }
        }
        previousState = DeathHandler.CurrentState;
    }
}
