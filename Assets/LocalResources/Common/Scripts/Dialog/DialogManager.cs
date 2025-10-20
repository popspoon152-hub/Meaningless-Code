using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DialogManager : MonoBehaviour
{
    // Start is called before the first frame update
    public TextAsset dialogDataFile;//csv文档


    public SpriteRenderer spriteLeft;//左侧图像

    public SpriteRenderer spriteRight;//右侧图像

    public SpriteRenderer SayPart;//左


    public TMP_Text nameText;//角色名字文本

    public TMP_Text dialogText;//对话内容文本

    public float waitingTime;//每个字等待时间

    public List<Sprite> sprites = new List<Sprite>();//角色图片列表

    Dictionary<string, Sprite> imageDic = new Dictionary<string, Sprite>();//角色名字图片对应字典

    public int dialogIndex;//当前对话索引

    public string[] dialogRows;//按行分割内容

    public Button nextButton;//继续

    public GameObject OptionButton;//选项预制体

    public Transform ButtonGroup;//选项父节点，用于排序

    private Coroutine typeTextCoroutine;

    private void Awake()
    {
        SayPart.gameObject.SetActive(false);
        ReadText(dialogDataFile);
        ReadCharacter();
    }
    void Start()
    {
        //ReadText(dialogDataFile);
        //ReadCharacter();
    }

    IEnumerator TypeText(TMP_Text tmp_text, string str, float interval)
    {
        int i = 0;
        while (i <= str.Length)
        {
            tmp_text.text = str.Substring(0, i++);
            yield return new WaitForSeconds(interval);
        }
    }
    public void UpdateText(string _name, string _text)
    {
        nameText.text = _name;
        //dialogText.text = _text;
        // 停止正在运行的协程
        if (typeTextCoroutine != null)
        {
            StopCoroutine(typeTextCoroutine);
        }

        // 启动新的协程
        typeTextCoroutine = StartCoroutine(TypeText(dialogText, _text, waitingTime));
    }

    public void UpdataImage(string _name, string _position)
    {
        if (_position == "左")
        {
            SayPart.gameObject.SetActive(true);
            spriteLeft.sprite = imageDic[_name];
        }
        else if (_position == "右")
        {
            SayPart.gameObject.SetActive(true);
            spriteRight.sprite = imageDic[_name];
        }
    }

    public void ReadCharacter()
    {
        for (int i = 1; i < sprites.Count+1; i++)
        {
            string[] cells = dialogRows[i].Split(',');
            if (cells[8] != null && cells[8] != "")
            {
                imageDic[cells[8]] = sprites[i-1];
            }
              
        }
    }

    public void ReadText(TextAsset _textAsset)
    {
        //dialogRows = _textAsset.text.Split('\n');
        //Debug.Log("读取成功");
        // 使用 UTF-8 编码读取文件内容
        string[] lines = _textAsset.text.Split(new[] {'\n'}, System.StringSplitOptions.RemoveEmptyEntries);

        // 清理每一行并分割
        dialogRows = new string[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            dialogRows[i] = lines[i].Trim(); // 移除每行开头和结尾的空格
        }

        // 拆分每一行
        Debug.Log("读取成功");
    }


    public void ShowDialogRow()
    {
        for (int i = 0; i < dialogRows.Length; i++)
        {
            string[] cells = dialogRows[i].Split(',');
            if (cells[0] == "#" && int.Parse(cells[1]) == dialogIndex)
            {
                //Debug.Log("succeed");
                UpdateText(cells[2], cells[4]);
                UpdataImage(cells[2], cells[3]);

                dialogIndex = int.Parse(cells[5]);
                nextButton.gameObject.SetActive(true);
                break;
            }
            else if (cells[0] == "$" && int.Parse(cells[1]) == dialogIndex)
            {
                nextButton.gameObject.SetActive(false);
                GenerateOption(i);
            }
            else if (cells[0]=="END" && int.Parse(cells[1]) == dialogIndex)
            {
                dialogIndex = 0;
                SceneManager.LoadScene(int.Parse(cells[6]));//跳转目标场景
            }
        }


    }

    public void OnClickNext()
    {
        ShowDialogRow();
    }

    public void GenerateOption(int _index)
    {
        string[] cells = dialogRows[_index].Split(",");
        if (cells[0] == "$")
        {
            GameObject button = Instantiate(OptionButton, ButtonGroup);//绑定按钮事件
            button.GetComponentInChildren<TMP_Text>().text = cells[4];
            button.GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    OnOptionClick(int.Parse(cells[5]));
                }
            );
            GenerateOption(_index + 1);
        }

    }

    public void OnOptionClick(int _id)//点击选项跳转
    {
        dialogIndex = _id;
        ShowDialogRow();
        for (int i = 0; i < ButtonGroup.childCount; i++)
        {
            Destroy(ButtonGroup.GetChild(i).gameObject);
        }
        nextButton.gameObject.SetActive(true);
    }

    public void OptionEffect(string _effect,int _param,string _target)//效果，数值，目标
    {

    }
}
