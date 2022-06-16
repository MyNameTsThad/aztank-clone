using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Path;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
    public static GameManager instance { get; private set; }
    private GameObject board;

    [Header("Prefabs")] 
    public GameObject dotPrefab;
    public GameObject wallPrefab;
    public GameObject tankPrefab;
    
    [Header("Settings")]
    [SerializeField] private int sideLength = 9;
    [SerializeField][Range(2, 5)] private int players = 2;
    public float moveSpeed = 3f;
    public float rotationSpeed = 1f;
    
    private GameObject[] corners;
    private GameObject[] walls;

    private void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (instance != null && instance != this) {
            Destroy(this);
        }
        else {
            instance = this;
            DontDestroyOnLoad(instance);
        }
    }   

    // Start is called before the first frame update
    private void Start() {
        board = GameObject.FindGameObjectWithTag("Board");
        corners = GameObject.FindGameObjectsWithTag("Corners");

        float diffrenceX = Math.Abs(corners[1].transform.position.x - corners[0].transform.position.x);
        float diffrenceY = Math.Abs(corners[1].transform.position.y - corners[0].transform.position.y);

        //populate board with dots
        for (var i = 0; i < sideLength + 1; i++) {
            for (var j = 0; j < sideLength + 1; j++) {
                GameObject dot = Instantiate(dotPrefab);
                dot.transform.position = new Vector3(corners[1].transform.position.x + diffrenceX / sideLength * i,
                    corners[1].transform.position.y + diffrenceY / sideLength * j, 0);
            }
        }

        //populate the maze
        GenerateMaze(diffrenceX, diffrenceY);

        //spawn tanks
        // for (int i = 0; i < players; i++) {
        //     GameObject tank = Instantiate(tankPrefab);
        //     var rand = new System.Random();
        //     int randX = rand.Next(1, 9), randY = rand.Next(1, 9);
        //     float variation = Random.Range(0f, 0.6f);
        //     float randRot = Random.Range(0f, 360f);
        //     
        //     //gen the center point
        //     tank.transform.position = new Vector3();
        //
        // }
    }

    private void GenerateMaze(float diffrenceX, float diffrenceY) {
        var maze = MazeGenerator.Generate(sideLength, sideLength);
        for (int i = 0; i < sideLength; i++) {
            for (int j = 0; j < sideLength; j++) {
                var cell = maze[i, j];
                    
                var dotPosX = corners[1].transform.position.x + diffrenceX / sideLength * i + 0.5f;
                var dotPosY = corners[1].transform.position.y + diffrenceY / sideLength * j + 0.5f;
                
                if (cell.HasFlag(WallState.UP) && Random.Range(1, 20) != 1) {
                    GameObject wall = Instantiate(wallPrefab);
                    wall.transform.position = new Vector3(dotPosX, dotPosY + 0.5f, 0);
                    wall.transform.rotation = Quaternion.Euler(0, 0, 90);
                    wall.name = "Wall " + i + " " + j + " UP";
                }
                if (cell.HasFlag(WallState.DOWN) && Random.Range(1, 20) != 2) {
                    GameObject wall = Instantiate(wallPrefab);
                    wall.transform.position = new Vector3(dotPosX, dotPosY - 0.5f, 0);
                    wall.transform.rotation = Quaternion.Euler(0, 0, 90);
                    wall.name = "Wall " + i + " " + j + " DOWN";
                }
                if (cell.HasFlag(WallState.LEFT) && Random.Range(1, 20) != 3) {
                    GameObject wall = Instantiate(wallPrefab);
                    wall.transform.position = new Vector3(dotPosX - 0.5f, dotPosY, 0);
                    wall.name = "Wall " + i + " " + j + " LEFT";
                }
                if (cell.HasFlag(WallState.RIGHT) && Random.Range(1, 20) != 4) {
                    GameObject wall = Instantiate(wallPrefab);
                    wall.transform.position = new Vector3(dotPosX + 0.5f, dotPosY, 0);
                    wall.name = "Wall " + i + " " + j + " RIGHT";
                }
            }
        }
        walls = GameObject.FindGameObjectsWithTag("Walls");
    }

    private void DeleteMaze() {
        foreach (var wall in walls) {
            Destroy(wall);
        }
    }
    
    public void ResetBoard(InputAction.CallbackContext context)
    {
        Debug.Log("Resetting!");
        float diffrenceX = Math.Abs(corners[1].transform.position.x - corners[0].transform.position.x);
        float diffrenceY = Math.Abs(corners[1].transform.position.y - corners[0].transform.position.y);
        DeleteMaze();
        GenerateMaze(diffrenceX, diffrenceY);
    }
}