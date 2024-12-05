using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHandler : MonoBehaviour {

    [Header("Respawn Settings")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private float respawnDelay;
    private Transform checkPoint;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    
    public enum PlayerState {
        Alive,
        Dying,
        Dead,
        Respawning
    }

    internal static PlayerState CurrentState {  get; private set; } = PlayerState.Alive;

    void Start() {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
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

            if (rb != null) {
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0f;
            }
            playerMovement.enabled = false;
        }
    }

    internal void KillPlayerAnimationComplete() {
        if (CurrentState == PlayerState.Dying) {
            StartCoroutine(RespawnPlayer());
            CurrentState = PlayerState.Dead;
        }
    }

    private IEnumerator RespawnPlayer() {

        yield return new WaitForSeconds(respawnDelay);

        CurrentState = PlayerState.Respawning;
    }

    internal void RespawnPlayerAnimationComplete() {
        // This method is actually called closer to the beginning of the respawn animation
        if (CurrentState == PlayerState.Respawning) {

            rb.gravityScale = playerMovement.originalGravity;
            transform.position = checkPoint != null ? checkPoint.position : respawnPoint.position;

            if (rb != null) {
                rb.velocity = Vector2.zero;
            }

            playerMovement.enabled = true;
            CurrentState = PlayerState.Alive;
        }
    }

    private void SetCheckPoint(Transform newCheckPoint) {
        checkPoint = newCheckPoint;
    }
}
