using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailEffect : MonoBehaviour
{

    private PlayerMovement playerMovement;
    private SpriteRenderer playerSpriteRenderer;

    [Header("Trail Configuration")]
    [SerializeField] private float trailDelay = 0.1f;
    [SerializeField] private float trailDelayLifetime = 1f;
    [SerializeField] private GameObject trailPrefab;

    private float trailDelaySeconds;
    private bool wasDashing = false;

    void Start() {
        playerMovement = GetComponent<PlayerMovement>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        trailDelaySeconds = trailDelay;
    }

    void Update() {

    }

    private void FixedUpdate() {
        RenderDashTrail();
    }

    private void RenderDashTrail() {
        if (playerMovement.isDashing) {
            if (trailDelaySeconds > 0f) {
                trailDelaySeconds -= Time.deltaTime;
            }
            else {
                GameObject currentTrail = Instantiate(trailPrefab, transform.position, transform.rotation);
                Sprite currentSprite = playerSpriteRenderer.sprite;
                currentTrail.GetComponent<SpriteRenderer>().sprite = currentSprite;
                currentTrail.GetComponent<SpriteRenderer>().flipX = playerSpriteRenderer.flipX;
                Destroy(currentTrail, trailDelayLifetime);
                trailDelaySeconds = trailDelay;
            }
            wasDashing = true;
        }
        else if (wasDashing) {
            trailDelaySeconds = trailDelay;
            wasDashing = false;
        }
    }
}
