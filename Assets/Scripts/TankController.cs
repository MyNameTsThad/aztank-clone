using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour {
    public Color tankColor;

    // Start is called before the first frame update
    void Start() {
        SpriteRenderer[] children = gameObject.GetComponentsInChildren<SpriteRenderer>();
        for (int index = 0; index < children.Length; index++) {
            SpriteRenderer child = children[index];
            float percent = index == 0 ? 1f : index == 1 ? 0.69f : 0.55f;
            child.color = new Color(tankColor.r * percent, tankColor.g * percent, tankColor.b * percent, tankColor.a);
        }
    }

    // Update is called once per frame
    void Update() {
    }
}