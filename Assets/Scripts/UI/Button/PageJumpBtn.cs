using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.TimeZoneInfo;

/// <summary>
/// ����ѡ���scenes����ת
/// </summary>
public enum Scenes
{
    //HomePage,
}

public class PageJumpBtn : MonoBehaviour
{
    public Scenes scene;
    private Button _btn;

    [Header("��������")]
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


    //ת������
    IEnumerator loadAnim()
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);

        var sceneName = scene.ToString();
        SceneManager.LoadScene(sceneName);
    }
}
