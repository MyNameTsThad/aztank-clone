using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour {
    public PowerupType type = PowerupType.NONE;

    private SpriteRenderer _spriteRenderer;

    // Start is called before the first frame update
    void Start() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (type != PowerupType.NONE) {
            _spriteRenderer.sprite = GetSprite(type);
        }
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.CompareTag("Player")) {
            TankController controller = col.gameObject.GetComponent<TankController>();
            if (controller.currentPowerUp == PowerupType.NONE) {
                Debug.Log("Powerup: " + type);
                controller.currentPowerUp = type;
                if (type == PowerupType.SPRAY) controller.currentSpraySize = GameManager.Instance.spraySize;
                if (type == PowerupType.MACHINEGUN) controller.currentMagSize = controller.currentMagSizeMax;
                Destroy(gameObject);
            }
        }
    }

    private Sprite GetSprite(PowerupType type) {
        switch (type) {
            case PowerupType.BIGBALL:
                return GameManager.Instance.bigBallSprite;
            case PowerupType.BOMB:
                return GameManager.Instance.bombSprite;
            case PowerupType.LASER:
                return GameManager.Instance.laserSprite;
            case PowerupType.MACHINEGUN: //done 
                return GameManager.Instance.machineGunSprite;
            case PowerupType.MISSILE:
                return GameManager.Instance.missileSprite;
            case PowerupType.SPRAY: //done
                return GameManager.Instance.spraySprite;
            case PowerupType.WIFI:
                return GameManager.Instance.wifiSprite;
            case PowerupType.WALLDESTROY:
                return GameManager.Instance.wallDestroySprite;
            default: //whatever
                return GameManager.Instance.bigBallSprite;
        }
    }
}

public enum PowerupType {
    NONE,
    BIGBALL, //shoots a big ball that explodes after some time
    BOMB,
    LASER,
    MACHINEGUN,
    MISSILE,
    SPRAY,
    WIFI,
    WALLDESTROY
}