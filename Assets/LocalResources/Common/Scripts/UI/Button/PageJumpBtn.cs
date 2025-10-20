using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.TimeZoneInfo;

public enum Scenes
{
    StartPage,
    FirstStagePage,
    GalPage,
    SecondStagePage,
    ThirdStagePage,
    SettingsPage,
    VideoPage
}

public class PageJumpBtn : MonoBehaviour
{
    public Scenes scene;
    private MyButton _btn;

    [Header("¹ý³¡¶¯»­")]
    public Animator transition;
    public float transitionTime;


    private void Start()
    {
        _btn = GetComponent<MyButton>();
        _btn.OnDoubleClick.AddListener(HandleBtnClick);
    }

    private void HandleBtnClick()
    {
        var sceneName = scene.ToString();
        SceneManager.LoadScene(sceneName);
        //StartCoroutine(loadAnim());
    }

    IEnumerator loadAnim()
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);

        var sceneName = scene.ToString();
        SceneManager.LoadScene(sceneName);
    }
}
