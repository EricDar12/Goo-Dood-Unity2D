using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour {

    [Header("Virtual Camera Settings")]
    [SerializeField] private GameObject v_cam;

    [Header("Room Settings")]
    [SerializeField] private BoxCollider2D _entranceZone;
    [SerializeField] private BoxCollider2D _exitZone;
    [SerializeField] private Transform _defaultSpawnPoint;

    public Transform CurrentSpawnPoint { get; private set; }

    void Start() {
        CurrentSpawnPoint = _defaultSpawnPoint;
    }

    void Update() {
        
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player") && !collision.isTrigger) {
            v_cam.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player") && !collision.isTrigger) {
            v_cam.SetActive(false);
        }
    }

}
