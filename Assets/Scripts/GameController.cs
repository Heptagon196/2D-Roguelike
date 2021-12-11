using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    public float deltaStarvingSpeed = 0f;
    public GameObject messageBox;
    public CameraFollow cameraFollow;
    public static GameController instance = null;
    public float showMessageTime = 0.5f;
    public GameObject Menu;
    public GameObject ContinueButton;
    public GameObject UploadButton;
    public AudioClip gameOverSound;
    private Text showRemainedFoods;
    private GameObject background;
    private Text message;
    private Player player;
    private int showingHealthPoint = 100;
    private int showingBulletNumber = 0;
    private int showingCoins = 0;
    private int showingTime = 0;
    private string showingWeapon = "";
    public int level = 0;
    [HideInInspector] public bool isShowingMessage = false;
    [HideInInspector] public float pausingTime;
    [HideInInspector] public float pausingStartTime;
    [HideInInspector] public bool forceUpdate = false;
    public int paused;
    private static readonly int PlayerDeath = Animator.StringToHash("playerDeath");

    public void setPause() {
        paused++;
    }

    public void setContinue() {
        paused--;
        if (paused < 0) {
            paused = 0;
        }
    }

    private IEnumerator _awaitAction(UnityAction action) {
        while (isPaused()) {
            yield return null;
        }
        action.Invoke();
    }

    public void AwaitAction(UnityAction action) {
        StartCoroutine(_awaitAction(action));
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this.gameObject);
            return;
        }
        if (GameObject.Find("SoundManager") == null) {
            SceneManager.LoadScene("Scenes/Opening");
            return;
        }
        paused = 0;
        showRemainedFoods = GameObject.Find("RemainedFoods").GetComponent<Text>();
        background = GameObject.Find("BlackBackground");
        message = GameObject.Find("CentralMessage").GetComponent<Text>();
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        level = PlayMode.instance.startLevel - 1;
        if (level == -1) {
            LoadNextLevel();
            AwaitAction(delegate() {
                messageBox.GetComponent<MessageBox>().ShowPassage(true,
                    new Sentence("Tutorial", "Welcome to this game!"),
                    new Sentence("First, please approach that businessman, left click on him, and start chatting."));
            });
        } else {
            LoadNextLevel();
        }
    }
    
    private void Update() {
        if (!ChooseWeapon.instance.menuIsOpen() && !ShopMenu.instance.menuIsOpen() && Input.GetKeyDown(KeyCode.Escape) && !isShowingMessage && !OptionMenuControl.instance.menuIsOpen() && !MessageBox.instance.hasBox && !MessageBox.instance.isShowing()) {
            if (paused != 0) {
                ContinueGame();
            } else {
                PauseGame();
            }
        }
    }

    private void FixedUpdate() {
        if (isBonus() && !MessageBox.instance.hasBox && ((int) player.healthPoint != showingHealthPoint ||
                          (int) (BonusStage.instance.lastingTime - Time.time + BonusStage.instance.gameStartTimePoint) != showingTime)) {
            showingHealthPoint = (int) player.healthPoint;
            showingTime = (int) (BonusStage.instance.lastingTime - Time.time + BonusStage.instance.gameStartTimePoint);
            UpdateRemainedResources();
            return;
        }
        if ((int) player.healthPoint != showingHealthPoint || player.bullet != showingBulletNumber || player.weapon.weaponName != showingWeapon || showingCoins != player.coins || forceUpdate) {
            forceUpdate = false;
            showingHealthPoint = (int) player.healthPoint;
            showingBulletNumber = player.bullet;
            showingWeapon = player.weapon.weaponName;
            showingCoins = player.coins;
            UpdateRemainedResources();
        }
    }

    private void ShowMessage() {
        message.text = "Day " + level;
        if (level == 0) {
            message.text = "Tutorial";
        }
        if (isBonus()) {
            message.text = "Bonus";
        }
        background.SetActive(true);
        message.gameObject.SetActive(true);
        PauseForSeconds(showMessageTime);
        isShowingMessage = true;
        Invoke(nameof(HideMessage), showMessageTime);
    }

    private void HideMessage() {
        background.SetActive(false);
        message.gameObject.SetActive(false);
        isShowingMessage = false;
    }

    public void LoadNextLevel() {
        if (isBonus(level + 1) || level == -1) {
            MapGenerator.instance.rows /= 2;
            MapGenerator.instance.columns /= 2;
        }
        if (isBonus(level) && level != -10 && level != -5) {
            MapGenerator.instance.rows *= 2;
            MapGenerator.instance.columns *= 2;
        }
        if (level == -10) {
            level = -5;
        } else if (level == -5) {
            PlayMode.instance.startLevel = 1;
            SceneManager.LoadScene("Scenes/Main");
            return;
        } else {
            level++;
        }
        ShowMessage();
        player.GetComponent<Player>().starvingSpeed += deltaStarvingSpeed;
        GetComponent<MapGenerator>().InitMap(level);
        if (isBonus() && level != 0) {
            cameraFollow.isFollowing = false;
        }
        if (isBonus(level - 1)) {
            forceUpdate = true;
        }
    }

    public void UpdateRemainedResources() {
        if (isBonus()) {
            showRemainedFoods.text = "Food: " + showingHealthPoint + "  Time left: " + Math.Max(showingTime + 1, 0) + "s";
        } else {
            showRemainedFoods.text = "Food: " + showingHealthPoint + "  Bullets: " + showingBulletNumber + "  Coins: " + showingCoins + "\nWeapon: " + showingWeapon;
        }
    }

    public void PauseForSeconds(float time) {
        pausingStartTime = Time.time;
        pausingTime = time;
    }

    public bool isPaused() {
        return Time.time - pausingStartTime < pausingTime || paused != 0;
    }

    public bool menuIsOpen() {
        return Menu.activeInHierarchy;
    }

    private void showGameOverMenu() {
        isShowingMessage = true;
        message.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 60f, 0f);
        message.text = "After " + level + " days, you starved.";
        message.gameObject.SetActive(true);
        background.SetActive(true);
        Menu.SetActive(true);
        UploadButton.SetActive(true);
    }

    public void GameOver() {
        SoundManager.instance.bgm.Pause();
        SoundManager.instance.Play(gameOverSound);
        setPause();
        player.GetComponent<Animator>().SetTrigger(PlayerDeath);
        Invoke(nameof(showGameOverMenu), 0.4f);
    }

    public void PauseGame() {
        if (isBonus()) {
            return;
        }
        SoundManager.instance.bgm.Pause();
        setPause();
        //Menu.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
        message.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 110f, 0f);
        message.text = "Paused\n\nDay " + level;
        if (level == 0) {
            message.text = "Paused\n\nTutorial";
        }
        message.gameObject.SetActive(true);
        Menu.SetActive(true);
        ContinueButton.SetActive(true);
    }
    
    public void ContinueGame() {
        if (isBonus()) {
            return;
        }
        SoundManager.instance.bgm.Play();
        setContinue();
        //Menu.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, -50f, 0f);
        message.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 60f, 0f);
        message.gameObject.SetActive(false);
        Menu.SetActive(false);
        ContinueButton.SetActive(false);
    }

    public void RestartGame() {
        if (!SoundManager.instance.bgm.isPlaying) {
            SoundManager.instance.bgm.Stop();
            SoundManager.instance.bgm.Play();
        }
        SceneManager.LoadScene("Scenes/Main");
    }

    public void BackToTitle() {
        SceneManager.LoadScene("Scenes/Menu");
    }

    public void UploadScore() {
        Rank.instance.playerScore = level;
        SceneManager.LoadScene("Scenes/Rank");
    }

    public void ExitGame() {
        Application.Quit();
    }

    public bool isBonus(int k) {
        return (k % 5 == 0) && (k != 0);
    }
    
    public bool isBonus() {
        return isBonus(level);
    }
}
