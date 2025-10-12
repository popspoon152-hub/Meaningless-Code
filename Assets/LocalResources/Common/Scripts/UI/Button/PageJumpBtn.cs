using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.TimeZoneInfo;

public enum Scenes
{
    HomePage,
    CharacterPage,
    SchedulePage
}

public class PageJumpBtn : MonoBehaviour
{
    public Scenes scene;
    private Button _btn;

    [Header("¹ý³¡¶¯»­")]
    public Animator transition;
    public float transitionTime;


    private void Start()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(HandleBtnClick);
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
