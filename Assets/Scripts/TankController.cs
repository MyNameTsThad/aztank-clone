using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour {
    public Color tankColor;
    private Vector2 movement, rotation;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start() {
        SpriteRenderer[] children = gameObject.GetComponentsInChildren<SpriteRenderer>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        for (int index = 0; index < children.Length; index++) {
            SpriteRenderer child = children[index];
            float percent = index == 0 ? 1f : index == 1 ? 0.69f : 0.55f;
            child.color = new Color(tankColor.r * percent, tankColor.g * percent, tankColor.b * percent, tankColor.a);
        }
    }
    
    private void FixedUpdate() {
        transform.Rotate(0, 0, rotation.x * GameManager.instance.rotationSpeed);
        var up = transform.TransformDirection(Vector3.up);
        rb.AddForce(movement * GameManager.instance.moveSpeed * Time.fixedDeltaTime * up);
    }

    public void Fire(InputAction.CallbackContext context) {
        Debug.Log("Fire!");
    }
    public void Move(InputAction.CallbackContext context) {
        movement = context.ReadValue<Vector2>();
        Debug.Log("Moving! " + movement);
    }
    public void Rotate(InputAction.CallbackContext context) {
        rotation = context.ReadValue<Vector2>();
        Debug.Log("Rotating! " + rotation);
    }
}