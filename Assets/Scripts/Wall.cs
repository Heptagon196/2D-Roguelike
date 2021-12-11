using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wall : MonoBehaviour {
    public int durability = 4;
    public float particleSpeed = 10;
    public GameObject particle;
    private void LateUpdate() {
        if (durability <= 0) {
            //Debug.Log(GetComponent<SpriteRenderer>().sprite.name);
            //particle.GetComponent<SpriteRenderer>().sprite = GetComponent<SpriteRenderer>().sprite;
            for (int i = 0; i < 10; i++) {
                float angle = Random.Range(0f, (float)(2 * Math.PI));
                var tmp = Instantiate(particle, transform.position + new Vector3(0.2f * (float) Math.Cos(angle), 0.2f * (float) Math.Sin(angle), 0f), transform.rotation);
                tmp.GetComponent<Rigidbody2D>().velocity = Random.Range(0f, 1f) * particleSpeed * new Vector2((float) Math.Cos(angle), (float) Math.Sin(angle));
            }
            Destroy(this.gameObject);
        }
    }
}
