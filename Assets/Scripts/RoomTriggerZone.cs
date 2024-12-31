using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTriggerZone : MonoBehaviour {

    private RoomController _roomController;

    void Start() {
        _roomController = GetComponentInParent<RoomController>();
        if (_roomController ==  null ) {
            Debug.LogError($"Room Controller Not Found For {gameObject.name}");
        }
    }

    void Update() {

    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            Transform spawnPoint = transform.Find("spawnPoint");
            if (spawnPoint != null) {
                //_roomController.CurrentSpawnPoint = spawnPoint; figure this out
            }
        }
    }

}
