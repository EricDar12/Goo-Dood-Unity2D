using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointTriggerZone : MonoBehaviour {

    private Room _currentRoom;

    private void Start() {
        _currentRoom = GetComponentInParent<Room>();
        if (_currentRoom == null) {
            Debug.Log("Room Not Found!");
        }
    }
    // Update the respawn position to the most recently entered or exited entrance/exit zone
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            // Find the spawn point associated with this zone
            Transform spawnPoint = transform.Find("SpawnPoint");
            if (spawnPoint != null) {
                _currentRoom.SetNewSpawnPoint(spawnPoint);
            }
        }
    }

}
