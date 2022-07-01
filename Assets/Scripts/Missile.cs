using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public class Missile : MonoBehaviour {
    /*[HideInInspector]*/ public ControlType controlType;
    /*[HideInInspector]*/ public TankController owner;
    /*[HideInInspector]*/ public InputAction rotateAction;

    private Rigidbody2D _rb;
    private float _rotation, _movement;
    private Vector2 _velocity;

    // Start is called before the first frame update
    void Start() {
        _rb = gameObject.GetComponent<Rigidbody2D>();
        _velocity = _rb.velocity;
    }

    // Update is called once per frame
    void Update() {
        if (rotateAction != null && controlType == ControlType.MANUAL) {
            _rotation = rotateAction.ReadValue<float>();
        }
    }

    private void FixedUpdate() {
        if (controlType == ControlType.MANUAL) {
            transform.Rotate(0, 0, -_rotation * GameManager.Instance.rotationSpeed);
            Vector2 direction = transform.up;
            _rb.velocity = GameManager.Instance.gameState != GameState.WIN2
                ? direction * (GameManager.Instance.missileSpeed * Time.fixedDeltaTime)
                : Vector3.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            Destroy(gameObject);
            if (GameManager.Instance.lethalBullets) {
                collision.gameObject.tag = "Dead";
                FindObjectOfType<AudioManager>().Play("Explode", 1);
                GameManager.Instance.CheckWin();
                StartCoroutine(collision.gameObject.GetComponent<TankController>().DeathAnimation());
            }
        } else {
            ReflectProjectile(_rb, collision.contacts[0].normal);
        }
    }
    
    private void ReflectProjectile(Rigidbody2D rb, Vector2 reflectVector) {    
        _velocity = Vector2.Reflect(_velocity, reflectVector);
        rb.velocity = _velocity;
    }

    private void OnDestroy() {
        //hand control back over
        owner.ReturnControl();
    }
}

public enum ControlType {
    MANUAL,
    AUTOMATIC
}