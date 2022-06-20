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
        }
        else {
            despawnTimer++;
        }
    }

    private void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.CompareTag("Player") && invulnerableTimer <= 0) {
            Destroy(gameObject);
            if (GameManager.Instance.lethalBullets) {
                col.gameObject.tag = "Dead";
                FindObjectOfType<AudioManager>().Play("Explode", 1);
                GameManager.Instance.CheckWin();
                StartCoroutine(col.gameObject.GetComponent<TankController>().DeathAnimation());
            }
        }
        // else if (col.gameObject.CompareTag("Walls")) {
        //     ContactPoint2D point = col.contacts[0];
        //     Vector2 newDir;
        //     Vector2 curDire = transform.TransformDirection(Vector2.up);
        //
        //     newDir = Vector2.Reflect(curDire, point.normal);
        //     transform.rotation = Quaternion.FromToRotation(Vector2.up, newDir);
        // }
    }
}