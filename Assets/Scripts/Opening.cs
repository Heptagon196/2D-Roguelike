using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Opening : MonoBehaviour {
    public string firstOpen = "hasEntered.txt";
    public Text text;
    public Text title;
    public float deltaTime = 0.1f;
    void Start() {
        if (File.Exists(firstOpen)) {
            SceneManager.LoadScene("Scenes/Menu");
            return;
        }
        File.WriteAllText(firstOpen, "This file marks that it's not your first time to enter the game. You'll be able to see the opening again by deleting this file.");
        PrintOpening("In the year of 2020/2\nA terrible disease, the 'Pigeon Death'/2\nhas long devastated the world/4",
                                      "No pestilence/2\nhas ever been so fatal and hideous/4",
                                      "Those affected won't do anything/2\nincluding eating/4",
                                      "Gradually, they starve to skin and bone/2\nAnd at last/2\nthey turn into /2P/2I/2G/2E/2O/2N/2S/6",
                                      "They keep attacking those unaffected/2\nto spread the disease/4",
                                      "And all you can do/2\nis to /2S/2U/2R/2V/2I/2V/2E/4");
    }
    
    private IEnumerator _printOpening(string[] message) {
        for (int j = 0; j < message.Length; j++) {
            text.text = "";
            for (int i = 0; i < message[j].Length; i++) {
                int k = 1;
                if (message[j][i] == '/') {
                    i++;
                    if (message[j][i] == 'r') {
                        text.text = "";
                    }
                    if (message[j][i] == '/') {
                        text.text += '/';
                    }
                    if (message[j][i] >= '0' && message[j][i] <= '9') {
                        k = Convert.ToInt32(message[j][i].ToString());
                    }
                }
                else {
                    text.text += message[j][i];
                }
                yield return new WaitForSeconds(deltaTime * k);
            }
        }
        TextFade.instance.FadeOut(text);
        TextFade.instance.FadeIn(title);
        Sparkle.instance.ShowWords();
        while (!Input.GetKeyDown(KeyCode.Space)) {
            yield return null;
        }
        SceneManager.LoadScene("Scenes/Menu");
    }

    public void PrintOpening(params string[] message) {
        StartCoroutine(_printOpening(message));
    }
}
