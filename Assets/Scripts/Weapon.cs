using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {
    public int value = 10;
    public int damageToEnemies = 1;
    public int damageToWalls = 1;
    public float attackTime = 0.3f;
    public float sqrSwayRange = 3f;
    public string weaponName;
    private bool destoryed = false;
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            ChooseWeapon.instance.AddWeapon(this, 1);
            destoryed = true;
        }
    }

    private void LateUpdate() {
        if (destoryed) {
            Destroy(this.gameObject);
        }
    }
}
