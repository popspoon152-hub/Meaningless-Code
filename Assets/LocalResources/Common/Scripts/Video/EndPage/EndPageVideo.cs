using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class EndPageVideo : MonoBehaviour
{
    [Header("Video")]
    public StartPageStats StartStats;
    public VideoClip EndVideo;

    [Header("Audio")]
    public AudioSource EndAudio;

    public VideoPlayer VideoPlayer;
    private void Start()
    {
        StartCoroutine(PlayEndingAnimation());
    }

    private IEnumerator PlayEndingAnimation()
    {
        if (EndVideo != null)
        {
            VideoPlayer.clip = EndVideo;
            VideoPlayer.isLooping = false;
            VideoPlayer.Play();

            yield return new WaitForSeconds((float)EndVideo.length);
        }
        BackToMainInterface();
    }

    private void BackToMainInterface()
    {
        StartStats.IsGameFinished = true;
        SceneManager.LoadScene("StartPage");
    }
}
