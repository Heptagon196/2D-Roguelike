using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using rand = UnityEngine.Random;
using Random = System.Random;

public class MapGenerator : MonoBehaviour {
    [Serializable]
    public class Range {
        public int minimum, maximum;
        public Range(int min, int max) {
            minimum = min;
            maximum = max;
        }
        public int getRand() {
            return rand.Range(minimum, maximum + 1);
        }

        public Range add(int x) {
            return new Range(minimum + x, maximum + x);
        }

        public Range times(int x) {
            return new Range(minimum * x, maximum * x);
        }
    }

    public static MapGenerator instance = null;
    public int columns = 12;
    public int rows = 8;
    public float weaponPossibility = 0.3f;
    public float businessmanPossibility = 0.3f;
    public Range wallCount = new Range(7, 18);
    public Range foodCount = new Range(3, 7);
    public BonusStage bonusStage;
    public GameObject player;
    public GameObject businessman = null;
    public GameObject exit;
    public GameObject exitToBonus;
    public GameObject[] floors;
    public GameObject[] walls;
    public GameObject[] borders;
    public GameObject[] foods;
    public GameObject[] enemies;
    public GameObject[] guns;
    public GameObject[] weapons;
    public GameObject[] diaries;
    
    [HideInInspector] public int[,] Grid = new int[50, 50];
    [HideInInspector] public int gridcnt;
    [HideInInspector] public Transform Map = null;
    
    private int bonusStageType = 0;
    private int[] down = new int[55];
    private int[] up = new int[55];

    /*
    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            GameController.instance.LoadNextLevel();
        }
    }
    */

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }
    }

    void InitList() {
        gridcnt = 0;
        for (int i = 0; i < 50; i++) {
            for (int j = 0; j < 50; j++) {
                Grid[i, j] = 0;
            }
        }
    }
    
    void GenerateBorder(bool generateExit = true) {
        for (int i = -1; i <= columns + 1; i ++) {
            for (int j = -1; j <= rows + 1; j ++) {
                GameObject type;
                if (i == -1 || j == -1 || i == columns + 1 || j == rows + 1) {
                    type = borders[rand.Range(0, borders.Length)];
                } else {
                    type = floors[rand.Range(0, floors.Length)];
                }
                Instantiate(type, new Vector3(i, j, 0f), Quaternion.identity).transform.SetParent(Map);
            }
        }
        player.transform.position = new Vector3(0f, 0f, 0f);
    }
    
    void GenerateIrregularBorderWithExit() {
        int[,] mark = new int[columns + 3, rows + 3];
        down = new int[55];
        up = new int[55];
        for (int i = 0; i <= columns + 2; i++) {
            mark[i, 0] = mark[i, rows + 2] = 1;
        }
        for (int i = 0; i <= rows + 2; i++) {
            mark[0, i] = mark[columns + 2, i] = 1;
        }
        for (int i = 1; i <= columns + 1; i++) {
            down[i] = (int)(Mathf.PerlinNoise(Time.time + DateTime.Now.Second / Mathf.PI, i * Mathf.PI) * (0.5 * rows));
            up[i] = (int)(Mathf.PerlinNoise(Time.time + DateTime.Now.Second / Mathf.PI, (i + columns + 1) * Mathf.PI) * (0.5 * rows));
            while (down[i] + up[i] >= rows) {
                up[i]--;
                down[i]--;
            }
            Instantiate(borders[rand.Range(0, borders.Length)], new Vector3(i - 1, down[i] - 1, 0f), Quaternion.identity).transform.SetParent(Map);
            Instantiate(borders[rand.Range(0, borders.Length)], new Vector3(i - 1, rows - up[i] + 1, 0f), Quaternion.identity).transform.SetParent(Map);
            for (int j = 1; j <= down[i]; j++) {
                mark[i, j] = 1;
                Grid[i - 1, j - 1] = 1;
            }
            for (int j = rows + 1; j >= rows - up[i] + 2; j--) {
                mark[i, j] = 1;
                Grid[i - 1, j - 1] = 1;
            }
            for (int j = down[i] + 1; j <= rows - up[i] + 1; j++) {
                Instantiate(floors[rand.Range(0, floors.Length)], new Vector3(i - 1, j - 1, 0f), Quaternion.identity).transform.SetParent(Map);
            }
            if (i == 1) {
                player.transform.position = new Vector3(i - 1, down[i], 0);
            }
            if (i == columns + 1) {
                GenerateExit(i - 1, rows - up[i]);
            }
        }
        down[0] = down[columns + 2] = Int32.MaxValue;
        up[0] = up[columns + 2] = Int32.MaxValue;
        for (int i = 1; i <= columns + 1; i++) {
            gridcnt += up[i] + down[i];
            for (int j = down[i] - 2; j >= Math.Min(down[i - 1], down[i + 1]) - 1; j--) {
                Instantiate(borders[rand.Range(0, borders.Length)], new Vector3(i - 1, j, 0f), Quaternion.identity).transform.SetParent(Map);
            }
            for (int j = rows - up[i] + 2; j <= rows - Math.Min(up[i - 1], up[i + 1]) + 1; j++) {
                Instantiate(borders[rand.Range(0, borders.Length)], new Vector3(i - 1, j, 0f), Quaternion.identity).transform.SetParent(Map);
            }
        }
        for (int i = down[1] - 1; i <= rows - up[1] + 1; i++) {
            Instantiate(borders[rand.Range(0, borders.Length)], new Vector3(-1, i, 0f), Quaternion.identity).transform.SetParent(Map);
        }
        for (int i = down[columns + 1] - 1; i <= rows - up[columns + 1] + 1; i++) {
            Instantiate(borders[rand.Range(0, borders.Length)], new Vector3(columns + 1, i, 0f), Quaternion.identity).transform.SetParent(Map);
        }
        down[0] = down[columns + 2] = Int32.MinValue;
        up[0] = up[columns + 2] = Int32.MinValue;
    }

    public Vector3 GetRandomPos(bool mark = true) {
        int x = 0, y = 0;
        if (gridcnt >= ((columns - 1) * (rows - 1))) {
            return new Vector3(-100, -100, 0);
        }
        do {
            x = rand.Range(1, columns);
            y = rand.Range(1, rows);
        } while (Grid[x, y] != 0);
        if (mark) {
            gridcnt++;
            Grid[x, y] = 1;
        }
        return new Vector3(x, y, 0);
    }

    private int max3(int a, int b, int c) {
        return Math.Max(Math.Max(a, b), c);
    }

    public Vector3 GetRandomWallPos() {
        int x = 0, y = 0;
        if (gridcnt >= ((columns - 1) * (rows - 1))) {
            return new Vector3(-100, -100, 0);
        }
        do {
            x = rand.Range(1, columns);
            y = rand.Range(max3(down[x + 1], down[x + 2] - 1, down[x] - 1) + 1, rows - max3(up[x + 1], up[x + 2] - 1, up[x] - 1));
        } while (Grid[x, y] != 0);
        gridcnt++;
        Grid[x, y] = 1;
        return new Vector3(x, y, 0);
    }
    
    public Vector3 GetRandomPosInColumn(int col, Range exclude = null) {
        int y = 0;
        do {
            y = rand.Range(1, rows);
        } while (Grid[col, y] != 0 || (exclude != null && (y >= exclude.minimum && y <= exclude.maximum)));
        gridcnt++;
        Grid[col, y] = 1;
        return new Vector3(col, y, 0);
    }
    
    public void GenerateObject(GameObject[] type, Range range) {
        if (type == null || type.Length == 0) {
            return;
        }
        int objcnt = range.getRand();
        for (int i = 0; i < objcnt; i ++) {
            Instantiate(type[rand.Range(0, type.Length)], GetRandomPos(), Quaternion.identity).transform.SetParent(Map);
        }
    }

    public void GenerateWalls(Range range) {
        if (walls == null || walls.Length == 0) {
            return;
        }
        int wallcnt = range.getRand();
        for (int i = 0; i < wallcnt; i++) {
            Instantiate(walls[rand.Range(0, walls.Length)], GetRandomWallPos(), Quaternion.identity).transform.SetParent(Map);
        }
    }

    public void TutorialClean() {
        for (int i = 0; i < Map.childCount; i++) {
            if (!Map.GetChild(i).transform.CompareTag("Businessman") && !Map.GetChild(i).transform.CompareTag("Floor") && !Map.GetChild(i).transform.CompareTag("Border")) {
                Destroy(Map.GetChild(i).gameObject);
            }
        }
    }

    public void GenerateExit(int X = -255, int Y = -255) {
        if (X == -255) {
            X = columns;
            Y = rows;
        }
        Instantiate(exit, new Vector3(X, Y, 0f), Quaternion.identity).transform.SetParent(Map);
    }
    
    public void GenerateExitToBonus(int X = -255, int Y = -255) {
        if (X == -255) {
            X = columns;
            Y = rows;
        }
        Instantiate(exitToBonus, new Vector3(X, Y, 0f), Quaternion.identity).transform.SetParent(Map);
    }
    
    public void InitMap(int level) {
        if (Map == null) {
            Map = new GameObject("BoardHolder").transform;
        }
        
        for (int i = 0; i < Map.childCount; i++) {
            Destroy(Map.GetChild(i).gameObject);
        }
        
        InitList();
        
        if (level == 0) {
            GenerateBorder();
            var tmp = Instantiate(businessman, GetRandomPos(), Quaternion.identity);
            tmp.transform.SetParent(Map);
            tmp.transform.position = new Vector3(3, 3, 0);
            return;
        }

        if (GameController.instance != null && GameController.instance.isBonus(level)) {
            GenerateBorder();
            player.transform.position = new Vector3(0f, 0f, 0f);
            if (bonusStageType == 0) {
                bonusStage.StartBonusStage1();
            } else if (bonusStageType == 1) {
                bonusStage.StartBonusStage2();
            }
            bonusStageType += 1;
            bonusStageType %= 2;
            return;
        }

        if (bonusStage == null) {
            GenerateBorder();
        } else {
            GenerateIrregularBorderWithExit();
        }

        int actualLevel = level - (int) (level / 5);
        int addtionalCount = (int) Math.Log(actualLevel, 2);

        if (SceneManager.GetActiveScene().name == "Main") {
            GenerateWalls(wallCount.add(addtionalCount).times(3));
            GenerateObject(foods, foodCount.times(3));
            int enemiesCount = (int) (Math.Pow(actualLevel, 0.75)) * 2;
            GenerateObject(enemies, new Range(enemiesCount, enemiesCount));
            GenerateObject(diaries, new Range(1, 4));
        } else {
            GenerateObject(walls, wallCount.add(addtionalCount));
            GenerateObject(foods, foodCount);
            int enemiesCount = (int) (Math.Pow(actualLevel, 0.75));
            GenerateObject(enemies, new Range(enemiesCount, enemiesCount));
        }

        for (int i = 0; i < 3; i++) {
            if (rand.Range(0f, 1f) < weaponPossibility) {
                GenerateObject(guns, new Range(1, 1));
            }

            if (rand.Range(0f, 1f) < weaponPossibility) {
                GenerateObject(weapons, new Range(1, 1));
            }
        }
        
        if (businessman == null) {
            return;
        }
        for (int i = 0; i < 2; i++) {
            if (rand.Range(0f, 1f) < businessmanPossibility) {
                Instantiate(businessman, GetRandomPos(), Quaternion.identity).transform.SetParent(Map);
            }
        }
    }
}
