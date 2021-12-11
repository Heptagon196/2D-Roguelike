using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public GameObject player;
    public bool isFollowing = true;

    private void FixedUpdate() {
        if (!isFollowing) {
            transform.position = new Vector3(6, 4, -10);
            return;
        }
        transform.position = player.transform.position + new Vector3(0, 0, -10);
    }
}
