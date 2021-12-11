using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {
    void Start() {
        if (GameObject.Find("SoundManager") == null) {
            SceneManager.LoadScene("Scenes/Opening");
            return;
        }
        if (!SoundManager.instance.bgm.isPlaying) {
            SoundManager.instance.bgm.Stop();
            SoundManager.instance.bgm.Play();
        }
        GetComponent<MapGenerator>().InitMap(2);
    }

    public void StartGame() {
        PlayMode.instance.startLevel = 1;
        SceneManager.LoadScene("Scenes/Main");
    }

    public void StartTutor() {
        PlayMode.instance.startLevel = 0;
        SceneManager.LoadScene("Scenes/Main");
    }

    public void ExitGame() {
        Application.Quit();
    }

    public void ViewRank() {
        SceneManager.LoadScene("Scenes/Rank");
    }
}
