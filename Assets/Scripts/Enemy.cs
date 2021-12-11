using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Enemy: MovingObj {
    public int money = 20;
    public float walkingSpeed = 1f;
    public float rushingSpeed = 2f;
    public float deltaTime = 2f;
    public float movingTime = 1f;
    public float attackTime = 1f;
    public int damgeToPlayer = 10;
    public int healthPoint = 10;
    public Animator thisAnimator;
    public LayerMask layer;
    public AudioClip[] attackSound;
    private float lastAttackTimePoint;
    private float lastChange;
    private float angle;
    private RaycastHit2D[] hit;
    private GameObject player;
    private static readonly int EnemyAttack = Animator.StringToHash("enemyAttack");
    private static readonly int PlayerHit = Animator.StringToHash("playerHit");
    private static readonly int EnemyWalk = Animator.StringToHash("enemyWalk");

    private new void Awake() {
        lastChange = Time.time;
        lastAttackTimePoint = Time.time;
        angle = Random.Range(0, (float) Math.PI * 2f);
        player = GameObject.FindWithTag("Player");
        base.Awake();
    }
    protected override Vector2 Move() {
        player.GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;
        hit = Physics2D.LinecastAll(player.transform.position, transform.position, layer);
        player.GetComponent<BoxCollider2D>().enabled = true;
        GetComponent<BoxCollider2D>().enabled = true;
        bool hasWall = false;
        for (int i = 0; i < hit.Length; i++) {
            if (hit[i].transform.gameObject.CompareTag("Wall") || hit[i].transform.gameObject.CompareTag("Border")) {
                hasWall = true;
                break;
            }
        }
        if (!hasWall) {
            lastChange = Time.time - movingTime;
            return Time.deltaTime * rushingSpeed * ((player.transform.position - transform.position) / (player.transform.position - transform.position).magnitude);
        }
        if (Time.time - lastChange >= movingTime + deltaTime) {
            angle = Random.Range(0, (float) Math.PI * 2f);
            lastChange = Time.time;
            return new Vector2(0f, 0f);
        }
        if (Time.time - lastChange <= movingTime) {
            thisAnimator.SetTrigger(EnemyWalk);
            return new Vector2((float) Math.Cos(angle) * walkingSpeed * Time.deltaTime, (float) Math.Sin(angle) * walkingSpeed * Time.deltaTime);
        } else {
            return new Vector2(0f, 0f);
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Player") && Time.time - lastAttackTimePoint >= attackTime) {
            SoundManager.instance.RandomPlay(attackSound);
            other.gameObject.GetComponent<Player>().healthPoint -= Math.Max(damgeToPlayer - other.gameObject.GetComponent<Player>().defense, 1);
            other.gameObject.GetComponent<Player>().starvingSpeed += 0.1f;
            lastAttackTimePoint = Time.time;
            thisAnimator.SetTrigger(EnemyAttack);
            other.gameObject.GetComponent<Animator>().SetTrigger(PlayerHit);
        }
    }

    private void LateUpdate() {
        if (healthPoint <= 0) {
            player.GetComponent<Player>().coins += money;
            Destroy(this.gameObject);
        }
    }
}
