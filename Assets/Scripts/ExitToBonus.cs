using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToBonus : MonoBehaviour {
    private GameObject controller = null;

    private void Awake() {
        if (controller == null) {
            controller = GameObject.FindWithTag("GameController");
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.GetComponent<Player>() != null) {
            if (GameController.instance.level == 0) {
                PlayMode.instance.startLevel = -10;
                SceneManager.LoadScene("Scenes/Main");
                return;
            }
            controller.GetComponent<GameController>().LoadNextLevel();
        }
    }
}
