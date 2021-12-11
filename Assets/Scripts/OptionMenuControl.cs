using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ButtonContent {
    public string text;
    public UnityAction action;
    public ButtonContent(string Text, UnityAction Action) {
        text = Text;
        action = Action;
    }
}

public class OptionMenuControl : MonoBehaviour {
    public static OptionMenuControl instance = null;
    public GameObject[] button;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this.gameObject);
            return;
        }
    }

    public bool menuIsOpen() {
        return button[1].activeInHierarchy;
    }

    public void CloseMenu() {
        for (int i = 0; i < 4; i ++) {
            button[i].SetActive(false);
        }
        GameController.instance.setContinue();
    }

    private IEnumerator SetToActiveAfterMessageFinished(int p) {
        while (!MessageBox.instance.hasBox) {
            yield return null;
        }
        while (MessageBox.instance.isShowing()) {
            yield return null;
        }
        if (!GameController.instance.isPaused()) {
            GameController.instance.setPause();
        }
        yield return new WaitForSeconds(0.1f);
        button[p].SetActive(true);
    }

    private void Activate(int p, ButtonContent b) {
        button[p].GetComponentInChildren<Text>().text = b.text;
        button[p].GetComponent<Button>().onClick.RemoveAllListeners();
        button[p].GetComponent<Button>().onClick.AddListener(() => {CloseMenu(); MessageBox.instance.skipCurrent = true; });
        button[p].GetComponent<Button>().onClick.AddListener(b.action);
        StartCoroutine(SetToActiveAfterMessageFinished(p));
    }
    
    public void ShowMenu(int count, params ButtonContent[] texts) {
        if (count == 1) {
            Activate(1, texts[0]);
        }
        if (count == 2) {
            Activate(1, texts[0]);
            Activate(2, texts[1]);
        }
        if (count == 3 || count == 4) {
            for (int i = 0; i < count; i++) {
                Activate(i, texts[i]);
            }
        }
    }
}
