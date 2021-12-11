using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun: MonoBehaviour {
    public int Bullets = 10;
    private bool destoryed = false;
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            other.gameObject.GetComponent<Player>().bullet += Bullets;
            destoryed = true;
        }
    }

    private void LateUpdate() {
        if (destoryed) {
            Destroy(this.gameObject);
        }
    }
}
