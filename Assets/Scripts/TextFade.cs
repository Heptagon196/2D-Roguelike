using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFade : MonoBehaviour {
    public static TextFade instance = null;
    public float deltaTime = 0.1f;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(gameObject);
        }
    }

    public IEnumerator _fadeIn(Text text) {
        Color tmp = text.color;
        for (int i = 0; i < 256; i += 15) {
            text.color = new Color(tmp.r, tmp.g, tmp.b, i / 256f);
            yield return new WaitForSeconds(deltaTime);
        }
    }

    public void FadeIn(Text text) {
        StartCoroutine(_fadeIn(text));
    }
    
    public IEnumerator _fadeOut(Text text) {
        Color tmp = text.color;
        for (int i = 255; i >= 0; i -= 15) {
            text.color = new Color(tmp.r, tmp.g, tmp.b, i / 256f);
            yield return new WaitForSeconds(deltaTime);
        }
    }

    public void FadeOut(Text text) {
        StartCoroutine(_fadeOut(text));
    }
}
