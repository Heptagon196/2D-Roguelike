using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

public class RankMenuControl: MonoBehaviour {
    public GameObject refresh;
    public GameObject uploadBox;
    public GameObject inputField;
    public List<GameObject> textBox;
    private void Awake() {
        if (GameObject.Find("SoundManager") == null) {
            SceneManager.LoadScene("Scenes/Opening");
            return;
        }
        GetComponent<MapGenerator>().InitMap(1);
        Rank.instance.textBox = textBox;
        refresh.GetComponent<Button>().onClick.RemoveAllListeners();
        refresh.GetComponent<Button>().onClick.AddListener(Rank.instance.ShowMessage);
        Rank.instance.ShowMessage();
        if (!SoundManager.instance.bgm.isPlaying) {
            SoundManager.instance.bgm.Stop();
            SoundManager.instance.bgm.Play();
        }
        if (Rank.instance.playerScore != -1) {
            uploadBox.SetActive(true);
        }
    }

    public void BackToTile() {
        if (Rank.instance.isRefreshing) {
            return;
        }
        SceneManager.LoadScene("Scenes/Menu");
    }

    public void SubmitScore() {
        Rank.instance.Upload(new Pair(inputField.GetComponent<InputField>().text, Rank.instance.playerScore));
        Rank.instance.playerScore = -1;
        Rank.instance.ShowMessage();
        uploadBox.SetActive(false);
    }
}
