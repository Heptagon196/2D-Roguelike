using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sparkle : MonoBehaviour {
    public static Sparkle instance = null;
    public float deltaTime = 0.1f;
    public Text text;
    readonly int L = 105;
    readonly int R = 255;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this.gameObject);
        }
    }

    private IEnumerator Sparkling() {
        Color tmp = text.color;
        for (int i = 0; i < R; i += 10) {
            text.color = new Color(tmp.r, tmp.g, tmp.b, i / 256f);
            yield return new WaitForSeconds(deltaTime);
        }
        while (true) {
            for (int i = 0; i <= 2 * (R - L); i += 10) {
                int p = Math.Abs(R - L - i) + L;
                text.color = new Color(tmp.r, tmp.g, tmp.b, p / 256f);
                yield return new WaitForSeconds(deltaTime);
            }
        }
    }
    
    public void ShowWords() {
        text.enabled = true;
        StartCoroutine(Sparkling());
    }
}
