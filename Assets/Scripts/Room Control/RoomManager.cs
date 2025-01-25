using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour {

    public static RoomManager Instance { get; private set; }
    public Room ActiveRoom { get; private set; }

    // Enforce Singleton Pattern
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void SetActiveRoom(Room room) {
        if (room != null) {
            ActiveRoom = room;
        } else {
            Debug.Log("Error setting active room");
        }
    }

}

