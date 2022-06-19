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

    [Header("Settings")] [SerializeField] private int sideLength = 9;
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

    [Header("Input")] public InputAction[] tanksMoveAction;
    public InputAction[] tanksRotateAction;
    public InputAction[] tanksShootAction;

    [Header("Gameplay-related public variables")]
    public int[] scores;
    
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

        //populate board with dots
        for (var i = 0; i < sideLength + 1; i++) {
            for (var j = 0; j < sideLength + 1; j++) {
                GameObject dot = Instantiate(dotPrefab);
                dot.transform.position = new Vector3(corners[1].transform.position.x + differenceX / sideLength * i,
                    corners[1].transform.position.y + differenceY / sideLength * j, 0);
            }
        }

        //populate the maze
        GenerateMaze(differenceX, differenceY);
        
        scores = new int[players];
        Array.Fill(scores, 0);

        //spawn tanks
        for (int i = 0; i < players; i++) {
            GameObject tank = Instantiate(tankPrefab);
            var rand = new System.Random();
            int randX = rand.Next(1, 9), randY = rand.Next(1, 9);
            float variationX = Random.Range(-0.3f, 0.3f), variationY = Random.Range(-0.3f, 0.3f);
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
    }

    private void GenerateMaze(float diffrenceX, float diffrenceY) {
        var maze = MazeGenerator.Generate(sideLength, sideLength);
        for (int i = 0; i < sideLength; i++) {
            for (int j = 0; j < sideLength; j++) {
                var cell = maze[i, j];

                var dotPosX = corners[1].transform.position.x + diffrenceX / sideLength * i + 0.5f;
                var dotPosY = corners[1].transform.position.y + diffrenceY / sideLength * j + 0.5f;

                if (cell.HasFlag(WallState.UP) && Random.Range(1, 10) != 1) {
                    GameObject wall = Instantiate(wallPrefab);
                    wall.transform.position = new Vector3(dotPosX, dotPosY + 0.5f, 0);
                    wall.transform.rotation = Quaternion.Euler(0, 0, 90);
                    wall.name = "Wall " + i + " " + j + " UP";
                }

                if (cell.HasFlag(WallState.DOWN) && Random.Range(1, 10) != 2) {
                    GameObject wall = Instantiate(wallPrefab);
                    wall.transform.position = new Vector3(dotPosX, dotPosY - 0.5f, 0);
                    wall.transform.rotation = Quaternion.Euler(0, 0, 90);
                    wall.name = "Wall " + i + " " + j + " DOWN";
                }

                if (cell.HasFlag(WallState.LEFT) && Random.Range(1, 10) != 3) {
                    GameObject wall = Instantiate(wallPrefab);
                    wall.transform.position = new Vector3(dotPosX - 0.5f, dotPosY, 0);
                    wall.name = "Wall " + i + " " + j + " LEFT";
                }

                if (cell.HasFlag(WallState.RIGHT) && Random.Range(1, 10) != 4) {
                    GameObject wall = Instantiate(wallPrefab);
                    wall.transform.position = new Vector3(dotPosX + 0.5f, dotPosY, 0);
                    wall.name = "Wall " + i + " " + j + " RIGHT";
                }
            }
        }
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

    public void ResetBoard(InputAction.CallbackContext context) {
        if (context.performed) {
            NewGame();
        }
    }

    public void NewGame() {
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

        gameState = GameState.PLAYING;

        board = GameObject.FindGameObjectWithTag("Board");
        corners = GameObject.FindGameObjectsWithTag("Corners");

        float differenceX = Math.Abs(corners[1].transform.position.x - corners[0].transform.position.x);
        float differenceY = Math.Abs(corners[1].transform.position.y - corners[0].transform.position.y);

        //populate board with dots
        for (var i = 0; i < sideLength + 1; i++) {
            for (var j = 0; j < sideLength + 1; j++) {
                GameObject dot = Instantiate(dotPrefab);
                dot.transform.position = new Vector3(corners[1].transform.position.x + differenceX / sideLength * i,
                    corners[1].transform.position.y + differenceY / sideLength * j, 0);
            }
        }

        //populate the maze
        GenerateMaze(differenceX, differenceY);

        //scores = new int[players];
        //Array.Fill(scores, 0);

        //spawn tanks
        for (int i = 0; i < players; i++) {
            GameObject tank = Instantiate(tankPrefab);
            var rand = new System.Random();
            int randX = rand.Next(1, 9), randY = rand.Next(1, 9);
            float variationX = Random.Range(-0.3f, 0.3f), variationY = Random.Range(-0.3f, 0.3f);
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
    }

    public void CheckWin() {
        if (GameObject.FindGameObjectsWithTag("Player").Length == 2) {
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