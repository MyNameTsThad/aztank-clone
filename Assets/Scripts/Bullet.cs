using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    private CircleCollider2D circleCollider2D;
    private Rigidbody2D rigidbody2D;
    private int invulnerableTimer;
    private int despawnTimer = 0;
    void Start() {
        circleCollider2D = GetComponent<CircleCollider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        invulnerableTimer = GameManager.Instance.invulnerableTimer;
    }

    private void FixedUpdate() {
        if (invulnerableTimer > 0) {
            invulnerableTimer--;
        }
        if (despawnTimer > GameManager.Instance.despawnTimerTicks) {
            Destroy(gameObject);
        } else {
            despawnTimer++;
        }
    }

    private void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.CompareTag("Player") && invulnerableTimer <= 0) {
            Destroy(gameObject);
            Destroy(col.gameObject);
            FindObjectOfType<AudioManager>().Play("Explode", 1);
            GameManager.Instance.CheckWin();
        }
    }
}