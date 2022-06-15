using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance { get; private set; }
    private GameObject board;
    public GameObject dotPrefab;
    public GameObject wallPrefab;

    private void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (instance != null && instance != this) {
            Destroy(this);
        } else {
            instance = this;
            DontDestroyOnLoad(instance);
        }
    }

    // Start is called before the first frame update
    void Start() {
        board = GameObject.FindGameObjectWithTag("Board");
    }

    // Update is called once per frame
    void Update() {
    }
}