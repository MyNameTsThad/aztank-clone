using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Path;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    private GameObject board;

    [Space(10)] public GameState gameState;
    [Space(10)] [Header("Prefabs")] public GameObject dotPrefab;

    public GameObject wallPrefab;
    public GameObject tankPrefab;
    public GameObject bulletPrefab;
    public GameObject powerupPrefab;
    [Space(5)] public Sprite bigBallSprite;
    public Sprite bombSprite;
    public Sprite laserSprite;
    public Sprite machineGunSprite;
    public Sprite missileSprite;
    public Sprite spraySprite;
    public Sprite wifiSprite;
    public Sprite wallDestroySprite;

    [Header("Settings")] [SerializeField] private int sideLength = 9;

    [Tooltip("Enables/Disables the maze generator.")] [SerializeField]
    private bool generateMaze = true;

    [Tooltip("The chance that a wall will be destroyed. In reverse % maybe, idk")] [SerializeField] [Range(1, 100)]
    private int mazeDestroyPercent = 10;

    [SerializeField] private bool reverse = false;
    [SerializeField] private float boxSize = 0.5f;
    [Tooltip("Bullets kill you or not?")] public bool lethalBullets = true;

    [Tooltip("Spawn Dots?")] [SerializeField]
    private bool spawnDots = true;

    [SerializeField] [Range(1, 5)] private int players = 2;
    public Color[] tankColors = { Color.blue, Color.red, Color.green, Color.yellow, Color.gray };
    public float moveSpeed = 3f;
    public float rotationSpeed = 1f;
    public float bulletSpeed = 10f;

    [Tooltip("Invulnerability Timer, In ticks (50 Ticks Per Second)")]
    public int invulnerableTimer = 5;

    [Tooltip("Despawn Timer, In ticks (50 Ticks Per Second)")]
    public int despawnTimerTicks = 1500;

    [Tooltip("Timer for each tank to reload their bullets, In ticks (50 Ticks Per Second)")]
    public int reloadTimerTicks = 250;

    [Tooltip("Each tank's default bullet storage size.")]
    public int defaultMagazineSize = 10;

    [Tooltip("Time from the second last tank dies and the 'Pass' sound plays, In Seconds")]
    public int passSoundTime = 3;

    [Tooltip("Time from the the 'Pass' sound plays and the game Reloading, In Seconds")]
    public int reloadTime = 2;

    [Tooltip("Minimum time for a powerup to spawn after the last one, In Seconds")]
    public int minPowerupSpawnTime = 1;

    [Tooltip("Maximum time for a powerup to spawn after the last one, In Seconds")]
    public int maxPowerupSpawnTime = 20;

    [Tooltip("Maximum number of powerups at a time.")]
    public int maxPowerups = 5;
    
    [Tooltip("imm too lazy to auto detect the animation time ok")]
    public float deathAnimationTime = 0.25f;

    [Header("Input")] public InputAction[] tanksMoveAction;
    public InputAction[] tanksRotateAction;
    public InputAction[] tanksShootAction;

    [Header("Gameplay-related public variables")]
    public int[] scores;

    [Header("Powerup related variables")] [Tooltip("SPRAY Powerup's bullets.")]
    public int spraySize = 10;

    [Tooltip("SPRAY Powerup's spray variation.")]
    public float sprayVariation = 0.1f;

    private GameObject[] corners;


    private void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) {
            Destroy(this);
        }
        else {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
    }

    // Start is called before the first frame update
    private void Start() {
        //keep this here for now
        gameState = GameState.PLAYING;

        board = GameObject.FindGameObjectWithTag("Board");
        corners = GameObject.FindGameObjectsWithTag("Corners");

        float differenceX = Math.Abs(corners[1].transform.position.x - corners[0].transform.position.x);
        float differenceY = Math.Abs(corners[1].transform.position.y - corners[0].transform.position.y);

        if (generateMaze) {
            if (spawnDots) {
                //populate board with dots
                for (var i = 0; i < sideLength + 1; i++) {
                    for (var j = 0; j < sideLength + 1; j++) {
                        GameObject dot = Instantiate(dotPrefab);
                        dot.transform.position = new Vector3(
                            corners[1].transform.position.x + differenceX / sideLength * i,
                            corners[1].transform.position.y + differenceY / sideLength * j, 0);
                    }
                }
            }

            //populate the maze
            GenerateMaze(differenceX, differenceY);
        }

        scores = new int[players];
        Array.Fill(scores, 0);

        //spawn tanks
        for (int i = 0; i < players; i++) {
            GameObject tank = Instantiate(tankPrefab);
            var rand = new System.Random();
            int randX = rand.Next(1, 9), randY = rand.Next(1, 9);
            float variationX = Random.Range(-0.2f, 0.2f), variationY = Random.Range(-0.2f, 0.2f);
            float randRot = Random.Range(0f, 360f);

            //gen the center point
            tank.transform.position = new Vector3(
                corners[1].transform.position.x - 0.5f + variationX + differenceX / sideLength * randX,
                corners[1].transform.position.y - 0.5f + variationY + differenceY / sideLength * randY,
                0);
            tank.transform.rotation = Quaternion.Euler(0, 0, randRot);

            tank.GetComponent<TankController>().tankColor = tankColors[i];
            tank.GetComponent<TankController>().scoreIndex = i;

            var inputMoveAction = tanksMoveAction[i];
            inputMoveAction.Enable();
            tank.GetComponent<TankController>().move = inputMoveAction;
            var inputRotationAction = tanksRotateAction[i];
            inputRotationAction.Enable();
            tank.GetComponent<TankController>().rotate = inputRotationAction;
            var inputFireAction = tanksShootAction[i];
            inputFireAction.Enable();
            tank.GetComponent<TankController>().fire = inputFireAction;
        }

        StartCoroutine(SpawnPowerups());
    }

    private void GenerateMaze(float diffrenceX, float diffrenceY) {
        var maze = MazeGenerator.Generate(sideLength, sideLength);
        var noSpawn = new List<string>();
        for (int i = 0; i < sideLength; i++) {
            for (int j = 0; j < sideLength; j++) {
                var cell = maze[i, j];

                var dotPosX = corners[1].transform.position.x + diffrenceX / sideLength * i + 0.5f;
                var dotPosY = corners[1].transform.position.y + diffrenceY / sideLength * j + 0.5f;

                if (reverse) {
                    if (cell.HasFlag(WallState.UP) && Random.Range(1, mazeDestroyPercent) == 1 &&
                        !noSpawn.Contains(getOppositeWallName("Wall " + i + " " + j + " UP", "UP"))) {
                        GameObject wall = Instantiate(wallPrefab);
                        wall.transform.position = new Vector3(dotPosX, dotPosY + boxSize, 0);
                        wall.transform.rotation = Quaternion.Euler(0, 0, 90);
                        wall.name = "Wall " + i + " " + j + " UP";
                        noSpawn.Add(wall.name);
                    }

                    if (cell.HasFlag(WallState.DOWN) && Random.Range(1, mazeDestroyPercent) == 1 &&
                        !noSpawn.Contains(getOppositeWallName("Wall " + i + " " + j + " DOWN", "DOWN"))) {
                        GameObject wall = Instantiate(wallPrefab);
                        wall.transform.position = new Vector3(dotPosX, dotPosY - boxSize, 0);
                        wall.transform.rotation = Quaternion.Euler(0, 0, 90);
                        wall.name = "Wall " + i + " " + j + " DOWN";
                        noSpawn.Add(wall.name);
                    }

                    if (cell.HasFlag(WallState.LEFT) && Random.Range(1, mazeDestroyPercent) == 1 &&
                        !noSpawn.Contains(getOppositeWallName("Wall " + i + " " + j + " LEFT", "LEFT"))) {
                        GameObject wall = Instantiate(wallPrefab);
                        wall.transform.position = new Vector3(dotPosX - boxSize, dotPosY, 0);
                        wall.name = "Wall " + i + " " + j + " LEFT";
                        noSpawn.Add(wall.name);
                    }

                    if (cell.HasFlag(WallState.RIGHT) && Random.Range(1, mazeDestroyPercent) == 1 &&
                        !noSpawn.Contains(getOppositeWallName("Wall " + i + " " + j + " RIGHT", "RIGHT"))) {
                        GameObject wall = Instantiate(wallPrefab);
                        wall.transform.position = new Vector3(dotPosX + boxSize, dotPosY, 0);
                        wall.name = "Wall " + i + " " + j + " RIGHT";
                        noSpawn.Add(wall.name);
                    }
                }
                else {
                    if (cell.HasFlag(WallState.UP) && Random.Range(1, mazeDestroyPercent) != 1 &&
                        !noSpawn.Contains(getOppositeWallName("Wall " + i + " " + j + " UP", "UP"))) {
                        GameObject wall = Instantiate(wallPrefab);
                        wall.transform.position = new Vector3(dotPosX, dotPosY + boxSize, 0);
                        wall.transform.rotation = Quaternion.Euler(0, 0, 90);
                        wall.name = "Wall " + i + " " + j + " UP";
                        noSpawn.Add(wall.name);
                    }

                    if (cell.HasFlag(WallState.DOWN) && Random.Range(1, mazeDestroyPercent) != 1 &&
                        !noSpawn.Contains(getOppositeWallName("Wall " + i + " " + j + " DOWN", "DOWN"))) {
                        GameObject wall = Instantiate(wallPrefab);
                        wall.transform.position = new Vector3(dotPosX, dotPosY - boxSize, 0);
                        wall.transform.rotation = Quaternion.Euler(0, 0, 90);
                        wall.name = "Wall " + i + " " + j + " DOWN";
                        noSpawn.Add(wall.name);
                    }

                    if (cell.HasFlag(WallState.LEFT) && Random.Range(1, mazeDestroyPercent) != 1 &&
                        !noSpawn.Contains(getOppositeWallName("Wall " + i + " " + j + " LEFT", "LEFT"))) {
                        GameObject wall = Instantiate(wallPrefab);
                        wall.transform.position = new Vector3(dotPosX - boxSize, dotPosY, 0);
                        wall.name = "Wall " + i + " " + j + " LEFT";
                        noSpawn.Add(wall.name);
                    }

                    if (cell.HasFlag(WallState.RIGHT) && Random.Range(1, mazeDestroyPercent) != 1 &&
                        !noSpawn.Contains(getOppositeWallName("Wall " + i + " " + j + " RIGHT", "RIGHT"))) {
                        GameObject wall = Instantiate(wallPrefab);
                        wall.transform.position = new Vector3(dotPosX + boxSize, dotPosY, 0);
                        wall.name = "Wall " + i + " " + j + " RIGHT";
                        noSpawn.Add(wall.name);
                    }
                }
            }
        }
    }

    private string getOppositeWallName(string name, string direction) {
        string[] split = name.Split(' ');
        int x = int.Parse(split[1]), y = int.Parse(split[2]);
        if (direction == "UP") {
            return "Wall " + x + " " + (y + 1) + " DOWN";
        }

        if (direction == "DOWN") {
            return "Wall " + x + " " + (y - 1) + " UP";
        }

        if (direction == "LEFT") {
            return "Wall " + (x - 1) + " " + y + " RIGHT";
        }

        if (direction == "RIGHT") {
            return "Wall " + (x + 1) + " " + y + " LEFT";
        }

        return name;
    }

    public IEnumerator WinDelay() {
        yield return new WaitForSeconds(passSoundTime);
        //check for tanks left
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length == 1) {
            FindObjectOfType<AudioManager>().Play("Pass", 1);
            gameState = GameState.WIN2;
            TankController controller = players[0].GetComponent<TankController>();
            scores[controller.scoreIndex] += 1;
            Debug.Log("Tank " + controller.scoreIndex + " score is now " + scores[controller.scoreIndex]);
        }

        foreach (var bullet in GameObject.FindGameObjectsWithTag("Bullets")) {
            bullet.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        yield return new WaitForSeconds(reloadTime);
        Debug.Log("reloading!");
        NewGame();
    }

    IEnumerator SpawnPowerups() {
        if (gameState == GameState.PLAYING && GameObject.FindGameObjectsWithTag("Powerups").Length < maxPowerups) {
            yield return new WaitForSeconds(Random.Range(minPowerupSpawnTime, maxPowerupSpawnTime));

            GameObject powerup = Instantiate(powerupPrefab);

            float differenceX = Math.Abs(corners[1].transform.position.x - corners[0].transform.position.x);
            float differenceY = Math.Abs(corners[1].transform.position.y - corners[0].transform.position.y);

            var rand = new System.Random();
            int randX = rand.Next(1, 9), randY = rand.Next(1, 9);

            powerup.transform.position = new Vector3(
                corners[1].transform.position.x - 0.5f + differenceX / sideLength * randX,
                corners[1].transform.position.y - 0.5f + differenceY / sideLength * randY,
                0);

            Array values = Enum.GetValues(typeof(PowerupType));
            PowerupType powerupType = (PowerupType)values.GetValue(Random.Range(1, values.Length));
            powerup.GetComponent<Powerup>().type = powerupType;
            Debug.Log("Spawned powerup " + powerupType);

            StartCoroutine(SpawnPowerups());
        }
    }

    public void ResetBoard(InputAction.CallbackContext context) {
        if (context.performed) {
            NewGame();
        }
    }

    public void NewGame() {
        StopCoroutine(SpawnPowerups());
        Debug.Log("Starting new Game!");
        foreach (var wall in GameObject.FindGameObjectsWithTag("Walls")) {
            Destroy(wall);
        }

        foreach (var tank in GameObject.FindGameObjectsWithTag("Player")) {
            Destroy(tank);
        }

        foreach (var bullet in GameObject.FindGameObjectsWithTag("Bullets")) {
            Destroy(bullet);
        }

        foreach (var powerup in GameObject.FindGameObjectsWithTag("Powerups")) {
            Destroy(powerup);
        }

        gameState = GameState.PLAYING;

        board = GameObject.FindGameObjectWithTag("Board");
        corners = GameObject.FindGameObjectsWithTag("Corners");

        float differenceX = Math.Abs(corners[1].transform.position.x - corners[0].transform.position.x);
        float differenceY = Math.Abs(corners[1].transform.position.y - corners[0].transform.position.y);

        if (generateMaze) {
            if (spawnDots) {
                //populate board with dots
                for (var i = 0; i < sideLength + 1; i++) {
                    for (var j = 0; j < sideLength + 1; j++) {
                        GameObject dot = Instantiate(dotPrefab);
                        dot.transform.position = new Vector3(
                            corners[1].transform.position.x + differenceX / sideLength * i,
                            corners[1].transform.position.y + differenceY / sideLength * j, 0);
                    }
                }
            }

            //populate the maze
            GenerateMaze(differenceX, differenceY);
        }

        //scores = new int[players];
        //Array.Fill(scores, 0);

        //spawn tanks
        for (int i = 0; i < players; i++) {
            GameObject tank = Instantiate(tankPrefab);
            var rand = new System.Random();
            int randX = rand.Next(1, 9), randY = rand.Next(1, 9);
            float variationX = Random.Range(-0.2f, 0.2f), variationY = Random.Range(-0.2f, 0.2f);
            float randRot = Random.Range(0f, 360f);

            //gen the center point
            tank.transform.position = new Vector3(
                corners[1].transform.position.x - 0.5f + variationX + differenceX / sideLength * randX,
                corners[1].transform.position.y - 0.5f + variationY + differenceY / sideLength * randY,
                0);
            tank.transform.rotation = Quaternion.Euler(0, 0, randRot);

            tank.GetComponent<TankController>().tankColor = tankColors[i];
            tank.GetComponent<TankController>().scoreIndex = i;

            var inputMoveAction = tanksMoveAction[i];
            inputMoveAction.Enable();
            tank.GetComponent<TankController>().move = inputMoveAction;
            var inputRotationAction = tanksRotateAction[i];
            inputRotationAction.Enable();
            tank.GetComponent<TankController>().rotate = inputRotationAction;
            var inputFireAction = tanksShootAction[i];
            inputFireAction.Enable();
            tank.GetComponent<TankController>().fire = inputFireAction;
        }

        StartCoroutine(SpawnPowerups());
    }

    public void CheckWin() {
        if (GameObject.FindGameObjectsWithTag("Player").Length == 1) {
            Debug.Log("One Tank Left!");
            gameState = GameState.WIN1;
            StartCoroutine(WinDelay());
        }
    }
}

public enum GameState {
    MENU,
    PLAYING,
    WIN1,
    WIN2,
    WAITING_NEW_GAME
}