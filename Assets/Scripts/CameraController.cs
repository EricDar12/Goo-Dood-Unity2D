using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [Header("Virtual Camera Settings")]
    [SerializeField] private GameObject v_cam;

    void Start() {
        
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
