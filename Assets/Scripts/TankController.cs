using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour {
    public Color tankColor;
    [SerializeField] private Transform shootPoint;
    public TankType type;
    private float _movement, _rotation;
    private Rigidbody2D _rb;

    private int _currentMagSize;
    private int _currentMagSizeMax;
    private int _reloadTimer;

    [HideInInspector] public InputAction move;
    [HideInInspector] public InputAction rotate;
    [HideInInspector] public InputAction fire;

    void Start() {
        _currentMagSize = GameManager.Instance.defaultMagazineSize;
        _currentMagSizeMax = GameManager.Instance.defaultMagazineSize;
        SpriteRenderer[] children = gameObject.GetComponentsInChildren<SpriteRenderer>();
        _rb = gameObject.GetComponent<Rigidbody2D>();
        for (int index = 0; index < children.Length; index++) {
            SpriteRenderer child = children[index];
            float percent = index == 0 ? 1f : index == 1 ? 0.69f : 0.55f;
            child.color = new Color(tankColor.r * percent, tankColor.g * percent, tankColor.b * percent, tankColor.a);
        }

        fire.performed += Fire;
    }

    private void OnDestroy() {
        //unbind inputs
        //move.Disable();
        //rotate.Disable();
        fire.performed -= Fire;
        //fire.Disable();
        move = null;
        rotate = null;
        fire = null;
    }

    private void FixedUpdate() {
        transform.Rotate(0, 0, -_rotation * GameManager.Instance.rotationSpeed);
        Vector2 direction = transform.up;
        _rb.velocity = (_movement * direction * Time.fixedDeltaTime * GameManager.Instance.moveSpeed);

        if (_currentMagSize <= 0 || _reloadTimer > 0) {
            if (_reloadTimer <= 0) {
                _reloadTimer = GameManager.Instance.reloadTimerTicks;
                //Debug.Log("reloadTimer now set to: " + reloadTimer);
                _currentMagSize = _currentMagSizeMax;
                //Debug.Log("currentMagSize now set to: " + currentMagSize);
            }
            else {
                _reloadTimer--;
                //Debug.Log("reloadTimer now set to: " + reloadTimer);
            }
        }
    }

    public void Fire(InputAction.CallbackContext context) {
        if (context.performed && GameManager.Instance.gameState != GameState.WIN) {
            //Debug.Log("Fire!");
            if (_currentMagSize > 0 && _reloadTimer <= 0) {
                Vector3 diffrence = shootPoint.position - transform.position;
                float distance = diffrence.magnitude;
                Vector2 direction = diffrence / distance;
                direction.Normalize();

                GameObject bullet = Instantiate(GameManager.Instance.bulletPrefab, shootPoint.position,
                    transform.rotation);
                bullet.GetComponent<Rigidbody2D>().velocity = direction * GameManager.Instance.bulletSpeed;

                FindObjectOfType<AudioManager>().Play("Shoot", 1);

                _currentMagSize--;
                //Debug.Log("currentMagSize now set to: " + currentMagSize);
            }
        }
    }

    private void Update() {
        _movement = move.ReadValue<float>();
        //Debug.Log("moving " + _movement);
        _rotation = rotate.ReadValue<float>();
        //Debug.Log("rotating " + _rotation);
    }
}

public enum TankType {
    FIRST,
    SECOND,
    THIRD,
    FOURTH,
    FIFTH
}

//https://stackoverflow.com/questions/5985661/methods-inside-enum-in-c-sharp
public static class TankTypeMethods {
    public static TankType GetTankType(this int number) {
        switch (number) {
            case 1:
                return TankType.FIRST;
            case 2:
                return TankType.SECOND;
            case 3:
                return TankType.THIRD;
            case 4:
                return TankType.FOURTH;
            case 5:
                return TankType.FIFTH;
            default:
                return TankType.FIRST;  
        }
    }
}