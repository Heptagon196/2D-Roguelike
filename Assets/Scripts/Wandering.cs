using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wandering : MonoBehaviour {
    public float walkingSpeed = 1f;
    public float deltaTime = 1.5f;
    public float movingTime = 1f;
    public Animator thisAnimator;
    private float lastChange;
    private float angle;
    private static readonly int EnemyAttack = Animator.StringToHash("enemyAttack");
    private static readonly int PlayerHit = Animator.StringToHash("playerHit");
    private static readonly int EnemyWalk = Animator.StringToHash("enemyWalk");

    private void Awake() {
        lastChange = Time.time;
        angle = Random.Range(0, (float) Math.PI * 2f);
    }
    private void FixedUpdate() {
        if (Time.time - lastChange >= movingTime + deltaTime) {
            angle = Random.Range(0, (float) Math.PI * 2f);
            lastChange = Time.time;
            return;
        }
        if (Time.time - lastChange <= movingTime) {
            thisAnimator.SetTrigger(EnemyWalk);
            transform.Translate(new Vector2((float) Math.Cos(angle) * walkingSpeed * Time.deltaTime, (float) Math.Sin(angle) * walkingSpeed * Time.deltaTime));
        } else {
            return;
        }
    }
}
