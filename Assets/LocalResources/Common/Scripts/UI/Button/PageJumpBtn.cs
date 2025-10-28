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
    GalBeforeFirstStage,
    FirstStagePage,
    GalAfterFirstStage,
    SecondStagePage,
    GalBeforeSecondStage,
    EndPage,
    SettingsPage,
}

public class PageJumpBtn : MonoBehaviour
{
    public Scenes Scene;
    private MyButton _btn;

    [Header("转场")]
    public Animator transition;
    public float transitionTime;


    private void Start()
    {
        _btn = GetComponent<MyButton>();
        _btn.OnDoubleClick.AddListener(HandleBtnClick);
    }

    private void HandleBtnClick()
    {
        var sceneName = Scene.ToString();
        SceneManager.LoadScene(sceneName);
        //StartCoroutine(loadAnim());
    }

    IEnumerator LoadAnim()
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);

        var sceneName = Scene.ToString();
        SceneManager.LoadScene(sceneName);
    }
}
