using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

[Serializable]
public class Sentence {
    public string speaker;
    public string message;
    public Sentence(string Content) {
         speaker = "_";
         message = Content;
    }
    public Sentence(string SpeakerName, string Content) {
         speaker = SpeakerName;
         message = Content;
    }
}

public class MessageBox : MonoBehaviour {
    public static MessageBox instance = null;
    public float deltaTime = 0.01f;
    public Text Message;
    public Image Background;
    public bool skipCurrent = false;
    //public int showingSentence;
    private string remainedString = "";
    private float messageBoxCloseTimePoint;
    private bool showAll = false;
    private float shouldLastTime;
    private const float epsilon = 0.1f;
    public bool hasBox;
    public int enable;
    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this.gameObject);
            return;
        }

        enable = 0;
        hasBox = false;
    }
    
    private void origEnable() {
        enable++;
        Message.enabled = true;
        Background.enabled = true;
    }

    private void origDisable() {
        if (enable > 0) {
            enable--;
            //Debug.Log(enable);
        }
        if (enable == 0) {
            Message.enabled = false;
            Background.enabled = false;
        }
    }

    public void OpenMessageBox() {
        messageBoxCloseTimePoint = -1;
        origEnable();
    }

    private float lastCloseTimePoint = 0f;
    public void CloseMessageBox() {
        /*
        if (Time.time - lastCloseTimePoint < 0.1f) {
            return;
        }
        */
        lastCloseTimePoint = Time.time;
        //Debug.Log("called at " + Time.time);
        origDisable();
        hasBox = false;
    }

    public bool isShowing() {
        return remainedString != "";
    }

    public void Update() {
        if (messageBoxCloseTimePoint != -1 && Time.time <= messageBoxCloseTimePoint + epsilon &&
            (Input.GetKeyDown(KeyCode.Return)) || Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.LeftControl) ||
            (GameController.instance.isPaused() && Input.GetMouseButtonDown((int) MouseButton.LeftMouse))) {
            showAll = true;
        }
        if (messageBoxCloseTimePoint == -1 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKey(KeyCode.LeftControl) ||
                                               (GameController.instance.isPaused() && Input.GetMouseButtonDown((int) MouseButton.LeftMouse)))) {
            showAll = true;
        }
    }

    private void AddAlpha() {
        if (showAll) {
            Message.text += remainedString;
            remainedString = "";
            showAll = false;
            if (Math.Abs(messageBoxCloseTimePoint - (-1)) > 0.1) {
                messageBoxCloseTimePoint = Time.time + shouldLastTime - epsilon;
                enable++;
                Invoke(nameof(CloseMessageBox), shouldLastTime);
            }
            return;
        }
        if (remainedString == "") {
            return;
        }
        Message.text += remainedString[0];
        remainedString = remainedString.Substring(1, remainedString.Length - 1);
    }

    private IEnumerator SpeakSentence(Sentence s) {
        messageBoxCloseTimePoint = -1;
        showAll = false;
        if (s.speaker != "") {
            Message.text = "【" + s.speaker + "】\n\n";
        } else {
            Message.text = "";
        }
        if (deltaTime <= 0) {
            Message.text += s.message;
            yield break;
        }
        remainedString = s.message;
        for (int i = 1; i <= s.message.Length; i++) {
            yield return new WaitForSeconds(deltaTime);
            AddAlpha();
            if (remainedString == "") {
                break;
            }
        }
    }

    private IEnumerator origShowPassage(bool closeWhenFinished, Sentence[] s) {
        for (int i = 0; i < s.Length; i ++) {
            //showingSentence = i;
            if (i == 0 && s[i].speaker == "_") {
                s[i].speaker = "";
            }
            if (i > 0 && s[i].speaker == "_") {
                s[i].speaker = s[i - 1].speaker;
            }
            StartCoroutine(SpeakSentence(s[i]));
            while (isShowing()) {
                yield return null;
            }
            while (((!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.Space) && !Input.GetMouseButtonDown((int) MouseButton.LeftMouse) && !Input.GetKey(KeyCode.LeftControl)) || OptionMenuControl.instance.menuIsOpen()) && !skipCurrent) {
                yield return null;
            }
            skipCurrent = false;
        }
        if (closeWhenFinished) {
            CloseMessageBox();
        } else {
            if (enable > 0) {
                enable--;
            }
        }
        hasBox = false;
        GameController.instance.setContinue();
    }

    private IEnumerator _showPassage(bool closeWhenFinished, Sentence[] s) {
        while (hasBox) {
            yield return null;
        }
        while (GameController.instance.menuIsOpen() || ChooseWeapon.instance.menuIsOpen()) {
            yield return null;
        }
        hasBox = true;
        GameController.instance.setPause();
        OpenMessageBox();
        StartCoroutine(origShowPassage(closeWhenFinished, s));
    }

    private IEnumerator _showMessage(string speakername, string message, float lastingTime) {
        while (hasBox) {
            yield return null;
        }
        while (GameController.instance.menuIsOpen() || ChooseWeapon.instance.menuIsOpen()) {
            yield return null;
        }
        shouldLastTime = lastingTime;
        showAll = false;
        //Debug.Log(enable);
        enable ++;
        //Debug.Log(enable);
        if (speakername != "") {
            Message.text = "【" + speakername + "】\n\n";
        } else {
            Message.text = "";
        }
        Message.enabled = true;
        Background.enabled = true;
        if (deltaTime <= 0) {
            Message.text += message;
        } else {
            remainedString = message;
            messageBoxCloseTimePoint = Time.time + deltaTime * message.Length + lastingTime - epsilon;
            //Debug.Log("Total time: " + (deltaTime * message.Length + lastingTime));
            //Debug.Log("Start: " + Time.time);
            for (int i = 1; i <= message.Length; i++) {
                yield return new WaitForSeconds(deltaTime);
                AddAlpha();
                if (remainedString == "") {
                    break;
                }
            }
            yield return new WaitForSeconds(lastingTime);
            //Debug.Log("End: " + Time.time);
            CloseMessageBox();
        }
    }

    public void ShowPassage(bool closeWhenFinished, params Sentence[] s) {
        StartCoroutine(_showPassage(closeWhenFinished, s));
    }

    public void ShowMessage(string message, float lastingTime) {
        StartCoroutine(_showMessage("", message, lastingTime));
    }
    
    public void ShowMessage(string speakername, string message, float lastingTime) {
        StartCoroutine(_showMessage(speakername, message, lastingTime));
    }

    private IEnumerator _startAction(UnityAction action) {
        while (hasBox) {
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        action.Invoke();
    }

    public void StartActionAfterMessage(UnityAction action) {
        StartCoroutine(_startAction(action));
    }
}
