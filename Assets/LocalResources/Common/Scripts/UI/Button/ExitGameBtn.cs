using UnityEngine;
using UnityEngine.UI;

public class ExitGameBtn : MonoBehaviour
{
    private Button _btn;

    void Start()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(HandleBtnClick);
    }

    private static void HandleBtnClick()
    {
        Application.Quit();
    }
}
