using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossSlider : MonoBehaviour
{
    [SerializeField] private Slider BossBar;


    public void UpdateHealth(float current, float max)
    {
        BossBar.value = current / max;
    }
}
