using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour {
    public float lastingTime = 0.4f;
    private float time;
    void Awake() {
        time = Time.time;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag != "Particle") {
            lastingTime = 0;
        }
    }

    void LateUpdate() {
        if (Time.time - time >= lastingTime) {
            Destroy(this.gameObject);
        }
    }
}
