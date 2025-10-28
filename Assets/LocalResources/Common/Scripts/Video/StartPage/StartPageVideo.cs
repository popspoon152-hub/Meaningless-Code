using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class StartPageVideo : MonoBehaviour
{
    [Header("Videos")]
    public StartPageStats StartStats;
    public VideoClip OpeningAnimation;
    public VideoClip StartVideo;
    public VideoClip GameFinishedVideo;
    public VideoClip ExitVideo;

    [Header("Audios")]
    public AudioSource StartAudio;

    public Image TitleImage;
    public Image VolumeImage;
    public float Speed = 0.2f;

    public Slider VolumeSlider;

    public MyButton StartButton;
    [SerializeField] private float fadeDuration = 2.0f;

    public VideoPlayer VideoPlayer;

    //private bool isTransitioning = false;

    void Start()
    {
        TitleImage.gameObject.SetActive(false);
        StartButton.gameObject.SetActive(false);
        VolumeSlider.gameObject.SetActive(false);
        VolumeImage.gameObject.SetActive(false);

        StartAudio.Stop();

        StartButton.OnDoubleClick.AddListener(ExitStartPage);

        StartCoroutine(PlayOpeningAnimation());
    }

    private IEnumerator PlayOpeningAnimation()
    {
        if (OpeningAnimation != null)
        {
            VideoPlayer.clip = OpeningAnimation;
            VideoPlayer.isLooping = false;
            VideoPlayer.Play();

            // 等待开场动画播放完成
            yield return new WaitForSeconds((float)OpeningAnimation.length);

        }
        EnterMainInterface();
    }

    private void EnterMainInterface()
    {
        if (!StartStats.IsGameFinished)
        {
            VideoPlayer.clip = StartVideo;      
        }
        else
        {
            VideoPlayer.clip = GameFinishedVideo;
        }
            
        VideoPlayer.isLooping = true;
        VideoPlayer.Play();

        StartAudio.loop = true;
        StartAudio.Play();

        StartCoroutine(FadeIn());
        StartButton.gameObject.SetActive(true);
        VolumeSlider.gameObject.SetActive(true);
        VolumeImage.gameObject.SetActive(true);
    }

    private IEnumerator FadeIn()
    {
        TitleImage.gameObject.SetActive(true);

        float elapsedTime = 0f;
        UnityEngine.Color originalColor = TitleImage.color;
        UnityEngine.Color transparentColor = new UnityEngine.Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        UnityEngine.Color targetColor = new UnityEngine.Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        // 初始设置为透明
        TitleImage.color = transparentColor;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            TitleImage.color = new UnityEngine.Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // 确保最终完全不透明
        TitleImage.color = targetColor;
    }

    private void ExitStartPage()
    {
        StartButton.gameObject.SetActive(false);

        StartCoroutine(PlayExitVideoAndLoadScene());
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        UnityEngine.Color originalColor = TitleImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            TitleImage.color = new UnityEngine.Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // 确保最终完全透明
        TitleImage.color = new UnityEngine.Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }

    private IEnumerator PlayExitVideoAndLoadScene()
    {
        // 播放退出视频
        VideoPlayer.clip = ExitVideo;
        VideoPlayer.isLooping = false;
        VideoPlayer.Play();

        // 等待视频长度的时间
        yield return new WaitForSeconds((float)ExitVideo.length);

        // 加载场景
        StartAudio.Stop();
        SceneManager.LoadScene("FirstStagePage");
    }









    //void Start()
    //{
    //    VideoPlayer.clip = StartMoveVideo;
    //    VideoPlayer.isLooping = true;
    //    VideoPlayer.Play();

    //    StartButton.OnDoubleClick.AddListener(ExitStartPage);
    //    StartButton.gameObject.SetActive(true);
    //}

    //private void OnDestroy()
    //{
    //    StartButton.OnDoubleClick.RemoveListener(ExitStartPage);
    //    VideoPlayer.loopPointReached -= OnStartMoveVideoFinished;
    //    //VideoPlayer.loopPointReached -= OnExitVideoFinished;
    //}

    //private void ExitStartPage()
    //{
    //    if (isTransitioning) return;

    //    isTransitioning = true;
    //    StartButton.gameObject.SetActive(false);

    //    VideoPlayer.loopPointReached -= OnStartMoveVideoFinished;
    //    VideoPlayer.loopPointReached += OnStartMoveVideoFinished;

    //    VideoPlayer.clip = ExitVideo;
    //    VideoPlayer.isLooping = false;
    //    VideoPlayer.Play();

    //    Debug.Log("开始播放退出视频");
    //}

    //private void OnStartMoveVideoFinished(VideoPlayer source)
    //{
    //    Debug.Log("开始移动视频播放完成");
    //    VideoPlayer.loopPointReached -= OnStartMoveVideoFinished;

    //    SceneManager.LoadScene("FirstStagePage");
    //}

    ////private void OnExitVideoFinished(VideoPlayer source)
    ////{
    ////    VideoPlayer.loopPointReached -= OnExitVideoFinished;
    ////    SceneManager.LoadScene("FirstStagePage");
    ////}
}
