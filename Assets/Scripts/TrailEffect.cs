using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailEffect : MonoBehaviour
{

    private PlayerMovement _playerMovement;
    private SpriteRenderer _playerSpriteRenderer;

    [Header("Trail Configuration")]
    [SerializeField] private float _trailDelay = 0.1f;
    [SerializeField] private float _trailDelayLifetime = 1f;
    [SerializeField] private GameObject _trailPrefab;

    private float _trailDelaySeconds;
    private bool _wasDashing = false;

    void Start() {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerSpriteRenderer = GetComponent<SpriteRenderer>();
        _trailDelaySeconds = _trailDelay;
    }

    void Update() {
        RenderDashTrail();
    }

    private void RenderDashTrail() {
        if (_playerMovement.IsDashing) {
            if (_trailDelaySeconds > 0f) {
                _trailDelaySeconds -= Time.deltaTime;
            }
            else {
                GameObject currentTrail = Instantiate(_trailPrefab, transform.position, transform.rotation);
                Sprite currentSprite = _playerSpriteRenderer.sprite;
                currentTrail.GetComponent<SpriteRenderer>().sprite = currentSprite;
                currentTrail.GetComponent<SpriteRenderer>().flipX = _playerSpriteRenderer.flipX;
                Destroy(currentTrail, _trailDelayLifetime);
                _trailDelaySeconds = _trailDelay;
            }
            _wasDashing = true;
        }
        else if (_wasDashing) {
            _trailDelaySeconds = _trailDelay;
            _wasDashing = false;
        }
    }
}
