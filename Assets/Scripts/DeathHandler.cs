using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHandler : MonoBehaviour {

    [Header("Respawn Settings")]
    [SerializeField] private Transform _respawnPoint;
    [SerializeField] private float _respawnDelay;

    private Rigidbody2D _rb;
    private Transform _checkPoint;
    private PlayerMovement _playerMovement;
    
    public enum PlayerState {
        Alive,
        Dying,
        Dead,
        Respawning
    }

    public static PlayerState CurrentState {  get; private set; } = PlayerState.Alive;

    void Start() {
        _playerMovement = GetComponent<PlayerMovement>();
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update() {
    
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (CurrentState == PlayerState.Alive && collision.gameObject.CompareTag("Spike")) {
            KillPlayer();
        }
    }

    private void KillPlayer() {
        
        if (CurrentState == PlayerState.Alive) {

            CurrentState = PlayerState.Dying;

            if (_rb != null) {
                _rb.velocity = Vector2.zero;
                _rb.gravityScale = 0f;
            }
            _playerMovement.enabled = false;
        }
    }

    internal void KillPlayerAnimationComplete() {
        if (CurrentState == PlayerState.Dying) {
            StartCoroutine(RespawnPlayer());
            CurrentState = PlayerState.Dead;
        }
    }

    private IEnumerator RespawnPlayer() {

        yield return new WaitForSeconds(_respawnDelay);

        CurrentState = PlayerState.Respawning;
    }

    internal void RespawnPlayerAnimationComplete() {
        // This method is actually called closer to the beginning of the respawn animation
        if (CurrentState == PlayerState.Respawning) {

            _rb.gravityScale = _playerMovement.OriginalGravity;
            transform.position = _checkPoint != null ? _checkPoint.position : _respawnPoint.position;

            if (_rb != null) {
                _rb.velocity = Vector2.zero;
            }

            _playerMovement.enabled = true;
            CurrentState = PlayerState.Alive;
        }
    }

    private void SetCheckPoint(Transform newCheckPoint) {
        _checkPoint = newCheckPoint;
    }
}
