using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour {

    private float _xVelocity;
    private float _yVelocity;
    private bool _isJumping;
    private bool _isDashing;

    private Animator _animator;
    private PlayerMovement _playerMovement;
    private DeathHandler.PlayerState _previousState = DeathHandler.PlayerState.Alive;
    
    void Start() {
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    void Update() {
        UpdateAnimationParameters();
        UpdateDeathAnimationState();
    }

    private void UpdateAnimationParameters() {
        _xVelocity = _playerMovement.XVelocity;
        _yVelocity = _playerMovement.YVelocity;
        _isJumping = _playerMovement.IsJumping;
        _isDashing = _playerMovement.IsDashing;

        _animator.SetFloat("xVelocity", Mathf.Abs(_xVelocity));
        _animator.SetFloat("yVelocity", _yVelocity);
        _animator.SetBool("isJumping", _isJumping);
        _animator.SetBool("isDashing", _isDashing);
    }

    private void UpdateDeathAnimationState() {

        if (DeathHandler.CurrentState != _previousState) {

            switch (DeathHandler.CurrentState) {

                case DeathHandler.PlayerState.Dying:
                    _animator.SetTrigger("Die");
                    break;

                case DeathHandler.PlayerState.Respawning:
                    _animator.SetTrigger("Respawn");
                    break;
            }
        }
        _previousState = DeathHandler.CurrentState;
    }
}
