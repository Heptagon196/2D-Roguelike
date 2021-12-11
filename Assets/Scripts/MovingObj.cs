using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObj : MonoBehaviour {
    private GameObject obj = null;
    private BoxCollider2D collider2d;
    private Rigidbody2D rgbody;
    protected GameController controller;
    public AudioClip[] moveSound;

    protected void Awake(){
        controller = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        collider2d = GetComponent<BoxCollider2D>();
        rgbody = GetComponent<Rigidbody2D>();
        if (obj == null) {
            obj = gameObject;
        }
    }
    
    protected abstract Vector2 Move();
    
    private void AttemptMove<T>(Vector2 movement) where T : Component {
        var tmp = obj.transform.position;
        obj.transform.Translate(new Vector3(movement.x, movement.y, 0f));
        if (obj.transform.position != tmp) {
            SoundManager.instance.RandomPlay(moveSound);
        }
    }

    protected void FixedUpdate() {
        if (controller.isPaused()) {
            return;
        }
        AttemptMove<Player>(Move());
    }
}
