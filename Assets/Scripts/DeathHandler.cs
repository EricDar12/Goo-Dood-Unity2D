using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHandler : MonoBehaviour {

    [Header("Respawn Settings")]
    [SerializeField] private float _respawnDelay = 1f;

    private Rigidbody2D _rb;
    private PlayerMovement _playerMovement;
    private RoomController _roomController;
    
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
        _roomController = FindObjectOfType<RoomController>();
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
            _rb.transform.position = _roomController.CurrentSpawnPoint.position;
            if (_rb != null) {
                _rb.velocity = Vector2.zero;
            }

            _playerMovement.enabled = true;
            CurrentState = PlayerState.Alive;
        }
    }

}
