using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class Player : MovingObj {
    public int coins = 0;
    [HideInInspector] public bool hasKilledBusinessman = false;
    public float speed = 3f;
    public float healthPoint = 100f;
    public float starvingSpeed = 1f;
    public Weapon weapon;
    public int defense = 0;
    public int bullet = 0;
    public GameObject bulletType;
    public float bulletSpeed = 5f;
    public AudioClip[] attackSound;
    public LayerMask layer;
    public float shootTime = 0.2f;
    private float lastAttackTimePoint = 0;
    private static readonly int PlayerChop = Animator.StringToHash("playerChop");
    private static readonly int PlayerShoot = Animator.StringToHash("playerShoot");

    protected override Vector2 Move() {
        var v = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (Math.Abs(v.magnitude) < Mathf.Epsilon) {
            return new Vector2(0, 0);
        }
        if (v.magnitude > 1) {
            v /= v.magnitude;
        }
        v = Time.deltaTime * speed * v;
        return v;
    }

    private bool isChatting = false;
    private IEnumerator StartChatAfterSeconds(Businessman b) {
        if (isChatting) {
            yield break;
        }
        yield return new WaitForSeconds(0.1f);
        isChatting = true;
        b.StartChat();
        MessageBox.instance.StartActionAfterMessage(delegate {
            isChatting = false;
        });
    }

    private void Update() {
        if (healthPoint < 1) {
            healthPoint = 0;
        }
        if (controller.isPaused() || OptionMenuControl.instance.menuIsOpen()) {
            return;
        }
        bool canShoot = false;
        bool canAttack = false;
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.GetComponent<BoxCollider2D>().enabled = false;
        var hit = Physics2D.Linecast(transform.position, pos, layer);
        transform.GetComponent<BoxCollider2D>().enabled = true;
        if (Input.GetMouseButtonDown((int) MouseButton.LeftMouse) && hit.transform != null &&
            hit.transform.gameObject.CompareTag("Businessman") && !hit.transform.gameObject.GetComponent<Businessman>().isAgressive &&
            (hit.transform.position - transform.position).sqrMagnitude < 2f &&
            ((Vector3) pos - transform.position).sqrMagnitude < 3f) {
            StartCoroutine(StartChatAfterSeconds(hit.transform.gameObject.GetComponent<Businessman>()));
            return;
        }
        if ((Time.time - lastAttackTimePoint < shootTime || bullet <= 0) && (Time.time - lastAttackTimePoint < weapon.attackTime)) {
            return;
        }
        if (Input.GetMouseButton((int) MouseButton.LeftMouse)) {
            if (hit.transform != null && (hit.transform.gameObject.CompareTag("Enemy") || hit.transform.gameObject.CompareTag("Wall") || (hit.transform.gameObject.CompareTag("Businessman") && hit.transform.gameObject.GetComponent<Businessman>().isAgressive))) {
                if ((hit.transform.position - transform.position).sqrMagnitude <= weapon.sqrSwayRange) {
                    canAttack = true;
                } else {
                    canShoot = bullet > 0;
                }
            }
        }
        if (Input.GetMouseButton((int) MouseButton.RightMouse) && bullet > 0) {
            canShoot = true;
        }
        if (Time.time - lastAttackTimePoint < weapon.attackTime) {
            canAttack = false;
        }
        if (Time.time - lastAttackTimePoint < shootTime) {
            canShoot = false;
        }
        if (!canAttack && !canShoot) {
            return;
        }
        lastAttackTimePoint = Time.time;
        if (canAttack) {
            GetComponent<Animator>().SetTrigger(PlayerChop);
            SoundManager.instance.RandomPlay(attackSound);
            if (hit.transform.gameObject.CompareTag("Enemy")) {
                hit.transform.gameObject.GetComponent<Enemy>().healthPoint -= weapon.damageToEnemies;
            } else if (hit.transform.gameObject.CompareTag("Wall")) {
                hit.transform.gameObject.GetComponent<Wall>().durability -= weapon.damageToWalls;
            } else {
                hit.transform.gameObject.GetComponent<Businessman>().healthPoint -= weapon.damageToEnemies;
            }
            return;
        }
        if (canShoot) {
            bullet--;
            GetComponent<Animator>().SetTrigger(PlayerShoot);
            var tmp = Instantiate(bulletType, transform.position, transform.rotation);
            tmp.GetComponent<Rigidbody2D>().velocity =
                ((Vector3) pos - transform.position) / ((Vector3) pos - transform.position).magnitude * bulletSpeed;
            return;
        }
    }

    protected new void FixedUpdate() {
        base.FixedUpdate();
        if (controller.isPaused()) {
            return;
        }
        healthPoint -= Math.Max(starvingSpeed * Time.deltaTime, 0.2f * Time.deltaTime);
        if (healthPoint < 1) {
            if (GameController.instance.level == 0) {
                healthPoint = 100;
                return;
            }
            GameObject.FindWithTag("GameController").GetComponent<GameController>().GameOver();
        }
    }
}
