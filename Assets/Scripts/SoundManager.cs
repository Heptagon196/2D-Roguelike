using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour {
    public static SoundManager instance = null;
    public AudioSource bgm;
    public AudioSource efx;
    public float lowPitchRange = 0.95f;
    public float highPitchRange = 1.05f;
    void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this.gameObject);
            return;
        }
        if (!bgm.isPlaying) {
            bgm.Stop();
            bgm.Play();
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void Play(AudioClip clip) {
        if (clip == null) {
            return;
        }
        efx.clip = clip;
        efx.Play();
    }

    public void RandomPlay(AudioClip[] clips) {
        if (clips.Length == 0) {
            return;
        }
        efx.pitch = Random.Range(lowPitchRange, highPitchRange);
        efx.clip = clips[Random.Range(0, clips.Length)];
        efx.Play();
    }
}
