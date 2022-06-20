using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class TankController : MonoBehaviour {
    public Color tankColor;
    [SerializeField] private Transform shootPoint;
    public int scoreIndex;
    public PowerupType currentPowerUp;

    private float _movement, _rotation;
    private Rigidbody2D _rb;

    private bool alive = true;

    private int _currentMagSize;

    [FormerlySerializedAs("_currentSpraySize")] [HideInInspector]
    public int currentSpraySize;

    [HideInInspector] [FormerlySerializedAs("_currentMagSizeMax")]
    public int currentMagSizeMax;

    [HideInInspector] public int maxReloadTimer;
    private int _reloadTimer;

    [HideInInspector] public InputAction move;
    [HideInInspector] public InputAction rotate;
    [HideInInspector] public InputAction fire;

    void Start() {
        alive = true;
        currentPowerUp = PowerupType.NONE;
        _currentMagSize = GameManager.Instance.defaultMagazineSize;
        currentMagSizeMax = GameManager.Instance.defaultMagazineSize;
        currentSpraySize = 0;
        maxReloadTimer = GameManager.Instance.reloadTimerTicks;
        SpriteRenderer[] children = gameObject.GetComponentsInChildren<SpriteRenderer>();
        _rb = gameObject.GetComponent<Rigidbody2D>();
        for (int index = 0; index < children.Length; index++) {
            SpriteRenderer child = children[index];
            if (child.transform.name != "Explosion") {
                float percent = index == 0 ? 1f : index == 1 ? 0.69f : 0.55f;
                child.color = new Color(tankColor.r * percent, tankColor.g * percent, tankColor.b * percent, tankColor.a);
            }
        }

        fire.performed += Fire;
    }

    private void OnDestroy() {
        //unbind inputs
        alive = false;
        if (fire != null) {
            fire.performed -= Fire;
        }

        move = null;
        rotate = null;
        fire = null;
    }

    private void FixedUpdate() {
        if (alive) {
            transform.Rotate(0, 0, -_rotation * GameManager.Instance.rotationSpeed);
            Vector2 direction = transform.up;
            _rb.velocity = GameManager.Instance.gameState != GameState.WIN2
                ? direction * (_movement * Time.fixedDeltaTime * GameManager.Instance.moveSpeed)
                : Vector3.zero;

            if (_currentMagSize <= 0 || _reloadTimer > 0) {
                if (_reloadTimer <= 0) {
                    _reloadTimer = maxReloadTimer;
                    //Debug.Log("reloadTimer now set to: " + reloadTimer);
                    _currentMagSize = currentMagSizeMax;
                    //Debug.Log("currentMagSize now set to: " + currentMagSize);
                }
                else {
                    _reloadTimer--;
                    //Debug.Log("reloadTimer now set to: " + reloadTimer);
                }
            }
        }
    }

    public IEnumerator DeathAnimation() {
        alive = false;
        OnDestroy();
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;
        Animator anim = GetComponent<Animator>();
        anim.enabled = true;
        anim.Play("Tank_Die");
        yield return new WaitForSeconds(GameManager.Instance.deathAnimationTime);
        Destroy(gameObject);
    }

    public void Fire(InputAction.CallbackContext context) {
        if (context.performed && (GameManager.Instance.gameState != GameState.WIN1 |
                                  GameManager.Instance.gameState != GameState.WIN2) &&
            alive) {
            //Debug.Log("Fire!");
            if (_currentMagSize > 0 && _reloadTimer <= 0) {
                Vector3 diffrence = shootPoint.position - transform.position;
                float distance = diffrence.magnitude;
                Vector2 direction = diffrence / distance;
                direction.Normalize();

                if (currentPowerUp == PowerupType.SPRAY && currentSpraySize > 0) {
                    Vector3 shootPointPos = shootPoint.position;
                    var transform1 = transform;
                    Quaternion shootPointRot1 = transform1.rotation;
                    Vector3 lea = transform1.localEulerAngles;
                    Quaternion shootPointRot2 = Quaternion.Euler(lea.x, lea.y,
                        lea.z + GameManager.Instance.sprayVariation);
                    Quaternion shootPointRot3 = Quaternion.Euler(lea.x, lea.y,
                        lea.z - GameManager.Instance.sprayVariation);
                    GameObject bullet1 = Instantiate(GameManager.Instance.bulletPrefab, shootPointPos, shootPointRot1);
                    bullet1.GetComponent<Rigidbody2D>().velocity =
                        bullet1.transform.up * GameManager.Instance.bulletSpeed;
                    GameObject bullet2 = Instantiate(GameManager.Instance.bulletPrefab, shootPointPos, shootPointRot2);
                    bullet2.GetComponent<Rigidbody2D>().velocity =
                        bullet2.transform.up * GameManager.Instance.bulletSpeed;
                    GameObject bullet3 = Instantiate(GameManager.Instance.bulletPrefab, shootPointPos, shootPointRot3);
                    bullet3.GetComponent<Rigidbody2D>().velocity =
                        bullet3.transform.up * GameManager.Instance.bulletSpeed;
                    currentSpraySize--;
                }
                else {
                    GameObject bullet = Instantiate(GameManager.Instance.bulletPrefab, shootPoint.position,
                        transform.rotation);
                    bullet.GetComponent<Rigidbody2D>().velocity =
                        bullet.transform.up * GameManager.Instance.bulletSpeed;
                }

                FindObjectOfType<AudioManager>().Play("Shoot", 1);

                _currentMagSize--;
                //Debug.Log("currentMagSize now set to: " + currentMagSize);
            }
        }
    }

    private void Update() {
        if (alive) {
            if (currentSpraySize <= 0) {
                currentPowerUp = PowerupType.NONE;
            }

            if (move != null && rotate != null) {
                _movement = move.ReadValue<float>();
                _rotation = rotate.ReadValue<float>();
            }
        }
    }
}