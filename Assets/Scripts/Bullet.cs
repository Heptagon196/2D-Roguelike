using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Bullet : MonoBehaviour {
    private bool destroyed = false;
    private Vector2 savedVelocity = new Vector2(0f, 0f);
    private Rigidbody2D rgbody;
    private bool velocityStore = false;
    private int startLevel;

    private void Awake() {
        rgbody = GetComponent<Rigidbody2D>();
        startLevel = GameController.instance.level;
    }

    private void FixedUpdate() {
        if (GameController.instance.isPaused()) {
            if (!velocityStore) {
                savedVelocity = rgbody.velocity;
                rgbody.velocity = new Vector2(0f, 0f);
                velocityStore = true;
            }
        } else if (velocityStore) {
            rgbody.velocity = savedVelocity;
            velocityStore = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Enemy")) {
            other.gameObject.GetComponent<Enemy>().healthPoint--;
        }
        if (other.gameObject.CompareTag("Wall")) {
            other.gameObject.GetComponent<Wall>().durability--;
        }
        destroyed = true;
        if (other.gameObject.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Items")) {
            destroyed = false;
        }
    }

    private void LateUpdate() {
        if (destroyed || startLevel != GameController.instance.level) {
            Destroy(this.gameObject);
        }
    }
}
