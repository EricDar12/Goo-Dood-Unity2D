using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    [Header("Virtual Camera Settings")]
    [SerializeField] private GameObject v_cam;

    [Header("Room Settings")]
    [SerializeField] private BoxCollider2D _entranceZone;
    [SerializeField] private BoxCollider2D _exitZone;
    [SerializeField] private Transform _defaultSpawnPoint;
    public Transform PlayerSpawnPoint { get; private set; }

    private void Start() {
        PlayerSpawnPoint = _defaultSpawnPoint;
    }

    public void SetNewSpawnPoint(Transform spawnPoint) {
        if (spawnPoint != null) {
            PlayerSpawnPoint = spawnPoint;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player") && !collision.isTrigger) {
            // Set this room to the active room upon entering it
            RoomManager.Instance.SetActiveRoom(this);
            v_cam.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player") && !collision.isTrigger) {
            v_cam.SetActive(false);
        }
    }
}
