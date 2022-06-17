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

    [Header("Prefabs")] public GameObject dotPrefab;
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

    [Header("Input")] public InputAction[] tanksMoveAction;
    public InputAction[] tanksRotateAction;
    public InputAction[] tanksShootAction;

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
            tank.GetComponent<TankController>().type = (i + 1).GetTankType();
            
            var inputMoveAction = tanksMoveAction[i];
            inputMoveAction.Enable();
            tank.GetComponent<TankController>().move = inputMoveAction;
            var inputRotationAction = tanksRotateAction[i];
            inputRotationAction.Enable();
            tank.GetComponent<TankController>().rotate = inputRotationAction;
            var inputFireAction = tanksShootAction[i];
            inputFireAction.Enable();
            inputFireAction.performed += tank.GetComponent<TankController>().Fire;
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

    public void ResetBoard(InputAction.CallbackContext context) {
        if (context.performed) {
            Debug.Log("Resetting!");
            // float diffrenceX = Math.Abs(corners[1].transform.position.x - corners[0].transform.position.x);
            // float diffrenceY = Math.Abs(corners[1].transform.position.y - corners[0].transform.position.y);
            // DeleteMaze();
            // GenerateMaze(diffrenceX, diffrenceY);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void CheckWin() {
        if (GameObject.FindGameObjectsWithTag("Player").Length == 2) {
            FindObjectOfType<AudioManager>().Play("Pass", 1);
        }
    }
}