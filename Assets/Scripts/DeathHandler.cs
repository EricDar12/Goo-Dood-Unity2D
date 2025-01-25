using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHandler : MonoBehaviour {

    [Header("Respawn Settings")]
    [SerializeField] private float _respawnDelay = 1f;

    private Rigidbody2D _rb;
    private PlayerMovement _playerMovement;
    private RoomManager _roomManager;

    public enum PlayerState {
        Alive,
        Dying,
        Dead,
        Respawning
    }

    public static PlayerState CurrentState { get; private set; } = PlayerState.Alive;

    void Start() {
        _playerMovement = GetComponent<PlayerMovement>();
        _rb = GetComponent<Rigidbody2D>();
        _roomManager = FindObjectOfType<RoomManager>();
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

    private IEnumerator RespawnPlayer() {
        yield return new WaitForSeconds(_respawnDelay);
        CurrentState = PlayerState.Respawning;
    }

    #region Animation Events
    // Called via the death and respawn animations as events

    internal void KillPlayerAnimationComplete() {
        if (CurrentState == PlayerState.Dying) {
            StartCoroutine(RespawnPlayer());
            CurrentState = PlayerState.Dead;
        }
    }

    internal void RespawnPlayerAnimationComplete() {
        // This method is actually called closer to the beginning of the respawn animation
        if (CurrentState == PlayerState.Respawning) {

            _rb.gravityScale = _playerMovement.OriginalGravity;
            // Access the active room to obtain respawn position
            _rb.transform.position = _roomManager.ActiveRoom.PlayerSpawnPoint.position;
            if (_rb != null) {
                _rb.velocity = Vector2.zero;
            }

            _playerMovement.enabled = true;
            CurrentState = PlayerState.Alive;
        }
    }
    #endregion
}
