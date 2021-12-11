using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Collections;
using UnityEngine;
using rand = UnityEngine.Random;

public class BonusStage : MonoBehaviour {
    public static BonusStage instance = null;
    public CameraFollow cameraFollow;
    public GameObject player;
    public GameObject snakeBody;
    public float snakeSpeed = 0.4f;
    public float moveSpeed = 0.2f;
    public float lastingTime = 39f;
    public GameObject[] foods;
    public GameObject[] enemies;
    
    [HideInInspector] public float gameStartTimePoint;
    [HideInInspector] public bool gameOver;

    private int moveDirection = 0;
    private int nextDirection = 0;
    private readonly int[] dx = {0, 0, 1, -1};
    private readonly int[] dy = {1, -1, 0, 0};
    private int head;
    private int tail;
    private List<GameObject> Food = new List<GameObject>();
    private List<GameObject> Body = new List<GameObject>();
    private List<GameObject> Enemies = new List<GameObject>();
    private List<int> prv = new List<int>();
    private List<int> nxt = new List<int>();

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this.gameObject);
        }
    }
    
    private IEnumerator smoothMove(GameObject obj, Vector3 deltapos, float deltatime) {
        for (int i = 0; i < 10; i++) {
            obj.transform.position += deltapos / 10;
            yield return new WaitForSeconds(deltatime / 20);
        }
    }

    private IEnumerator snakeMove() {
        yield return new WaitForSeconds(GameController.instance.showMessageTime);
        moveDirection = 0;
        nextDirection = 0;
        gameStartTimePoint = Time.time;
        gameOver = false;
        isSnake = true;
        while (!gameOver) {
            moveDirection = nextDirection;
            Vector3 nextpos = Body[head].transform.position + new Vector3(dx[moveDirection], dy[moveDirection]);
            RaycastHit2D hit;
            Body[head].transform.GetComponent<BoxCollider2D>().enabled = false;
            hit = Physics2D.Linecast(Body[head].transform.position, nextpos);
            Body[head].transform.GetComponent<BoxCollider2D>().enabled = true;
            if (Time.time - gameStartTimePoint > lastingTime) {
                gameOver = true;
                EndGame();
                yield break;
            }
            if (hit.transform != null) {
                /*
                Debug.Log(hit.transform.gameObject.tag);
                Debug.Log(hit.transform.position);
                Debug.Log(Body[head].transform.position);
                */
                if (hit.transform.gameObject.CompareTag("Food") || hit.transform.gameObject.CompareTag("Soda")) {
                    Destroy(hit.transform.gameObject);
                    int p = Body.Count;
                    Body.Add(Instantiate(snakeBody, nextpos, Quaternion.identity));
                    prv.Add(-1); // prv[p] = -1;
                    nxt.Add(new int()); // nxt[p] = head;
                    nxt[p] = head;
                    prv[head] = p;
                    head = p;
                    player.GetComponent<Player>().healthPoint += hit.transform.gameObject.GetComponent<Food>().points;
                    SoundManager.instance.RandomPlay(hit.transform.gameObject.GetComponent<Food>().eatSound);
                    GenerateFood();
                    yield return new WaitForSeconds(snakeSpeed);
                } else {
                    gameOver = true;
                    EndGame();
                    yield break;
                }
                continue;
            }
            Body[tail].transform.position = nextpos;
            if (head != tail) {
                int tmp = prv[tail];
                nxt[prv[tail]] = -1;
                prv[tail] = -1;
                nxt[tail] = head;
                prv[head] = tail;
                head = tail;
                tail = tmp;
            }
            yield return new WaitForSeconds(snakeSpeed);
        }
    }

    private void Update() {
        if (!isSnake) {
            return;
        }
        if (Input.GetKey(KeyCode.A) && moveDirection != 2) {
            nextDirection = 3;
        }
        if (Input.GetKey(KeyCode.D) && moveDirection != 3) {
            nextDirection = 2;
        }
        if (Input.GetKey(KeyCode.W) && moveDirection != 1) {
            nextDirection = 0;
        }
        if (Input.GetKey(KeyCode.S) && moveDirection != 0) {
            nextDirection = 1;
        }
    }

    private void EndGame() {
        gameOver = true;
        GameController.instance.setContinue();
        if (isSnake) {
            foreach (var i in Body) {
                if (i != player) {
                    Destroy(i.gameObject);
                }
            }
        } else {
            foreach (var i in Enemies) {
                Destroy(i.gameObject);
            }
        }
        foreach (var i in Food) {
            Destroy(i.gameObject);
        }
        MessageBox.instance.ShowPassage(true, new Sentence("Bonus stage is over!"));
        MessageBox.instance.StartActionAfterMessage(() => {
            cameraFollow.isFollowing = true;
            GameController.instance.LoadNextLevel();
            isSnake = false;
        });
    }
    
    private void GenerateFood() {
        Vector3 pos = new Vector3();
        bool shouldContinue = true;
        while (shouldContinue) {
            shouldContinue = false;
            pos = MapGenerator.instance.GetRandomPos(false);
            foreach (var i in Body) {
                if ((i.transform.position - pos).magnitude < 0.1f) {
                    shouldContinue = true;
                }
            }
        }
        Food.Add(Instantiate(foods[rand.Range(0, foods.Length)], pos, Quaternion.identity));
    }

    private bool isSnake = false;
    
    public void StartBonusStage1() {
        GameController.instance.setPause();
        Body.Clear();
        prv.Clear();
        nxt.Clear();
        Body.Add(player);
        prv.Add(-1);
        nxt.Add(-1);
        head = tail = 0;
        List<int> l = new List<int>();
        List<int> l2 = new List<int>();
        for (int i = 1; i <= MapGenerator.instance.columns - 1; i++) {
            l.Add(i);
        }
        MapGenerator.Range lastRange = new MapGenerator.Range(0, 0);
        for (int k = 0; k < 7; k++) {
            int p = rand.Range(0, l.Count);
            l2.Add(l[p]);
            l.RemoveAt(p);
        }
        l2.Sort();
        foreach (var k in l2) {
            var vec = MapGenerator.instance.GetRandomPosInColumn(k, lastRange);
            //Debug.Log(vec);
            Instantiate(MapGenerator.instance.walls[rand.Range(0, MapGenerator.instance.walls.Length)], vec, Quaternion.identity).transform.SetParent(MapGenerator.instance.Map);
            lastRange = new MapGenerator.Range((int)(vec.y - 1 + Mathf.Epsilon), (int)(vec.y + 1 + Mathf.Epsilon));
            //Debug.Log(lastRange);
        }
        GenerateFood();
        StartCoroutine(snakeMove());
    }

    private IEnumerator bonus2Move() {
        yield return new WaitForSeconds(GameController.instance.showMessageTime);
        gameStartTimePoint = Time.time;
        moveDirection = 0;
        nextDirection = 0;
        gameOver = false;
        while (!gameOver) {
            if (Time.time - gameStartTimePoint > lastingTime) {
                gameOver = true;
                EndGame();
                yield break;
            }
            if (Input.GetKey(KeyCode.A)) {
                moveDirection = 3;
            } else if (Input.GetKey(KeyCode.D)) {
                moveDirection = 2;
            } else if (Input.GetKey(KeyCode.W)) {
                moveDirection = 0;
            } else if (Input.GetKey(KeyCode.S)) {
                moveDirection = 1;
            } else {
                yield return null;
                continue;
            }
            var nextpos = player.transform.position + new Vector3(dx[moveDirection], dy[moveDirection]);
            RaycastHit2D hit;
            player.transform.GetComponent<BoxCollider2D>().enabled = false;
            hit = Physics2D.Linecast(player.transform.position, nextpos);
            player.transform.GetComponent<BoxCollider2D>().enabled = true;
            if (hit.transform != null && hit.transform.gameObject.CompareTag("Border")) {
                yield return new WaitForSeconds(moveSpeed);
                continue;
            }
            StartCoroutine(smoothMove(player, new Vector3(dx[moveDirection], dy[moveDirection]), moveSpeed));
            yield return new WaitForSeconds(moveSpeed);
        }
    }

    public void StartBonusStage2() {
        GameController.instance.setPause();
        isSnake = false;
        MapGenerator.instance.GenerateObject(MapGenerator.instance.foods, new MapGenerator.Range(10, 10));
        List<int> l = new List<int>();
        for (int i = 1; i <= MapGenerator.instance.columns - 1; i++) {
            l.Add(i);
        }
        for (int i = 0; i < 7; i++) {
            int p = rand.Range(0, l.Count);
            Enemies.Add(Instantiate(enemies[rand.Range(0, enemies.Length)], new Vector3(l[p], rand.Range(1, MapGenerator.instance.rows)), Quaternion.identity));
            l.RemoveAt(p);
        }
        StartCoroutine(bonus2Move());
    }
}
