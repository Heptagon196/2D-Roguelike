using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Food : MonoBehaviour {
    public int points = 10;
    public AudioClip[] eatSound;
    private bool destoryed = false;
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player") && !destoryed) {
            SoundManager.instance.RandomPlay(eatSound);
            other.gameObject.GetComponent<Player>().healthPoint += points;
            destoryed = true;
        }
    }

    private void LateUpdate() {
        if (destoryed) {
            Destroy(this.gameObject);
        }
    }
}
