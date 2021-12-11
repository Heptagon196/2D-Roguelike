using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpAndDownEnemy : MonoBehaviour {
    public float walkingSpeed = 5f;
    public Animator thisAnimator;
    public AudioClip[] attackSound;
    private int moveTo;
    private float lastChange;
    private float lastChange2;
    private static readonly int EnemyAttack = Animator.StringToHash("enemyAttack");
    private static readonly int PlayerHit = Animator.StringToHash("playerHit");
    private static readonly int EnemyWalk = Animator.StringToHash("enemyWalk");

    private void Awake() {
        moveTo = Random.Range(0, 2) * 2 - 1;
    }
    
    private void FixedUpdate() {
        transform.Translate(Time.deltaTime * walkingSpeed * moveTo * Vector2.up);
    }
    
    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Player") && !BonusStage.instance.gameOver) {
            SoundManager.instance.RandomPlay(attackSound);
            BonusStage.instance.SendMessage("EndGame");
            //other.gameObject.GetComponent<Player>().healthPoint -= Math.Max(damgeToPlayer - other.gameObject.GetComponent<Player>().defense, 0);
            thisAnimator.SetTrigger(EnemyAttack);
            other.gameObject.GetComponent<Animator>().SetTrigger(PlayerHit);
        } else if (Time.time - lastChange > 0.01f) {
            moveTo *= -1;
            lastChange = Time.time;
        }
    }
}
