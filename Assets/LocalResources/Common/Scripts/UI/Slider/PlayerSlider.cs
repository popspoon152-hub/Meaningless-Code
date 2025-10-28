using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlider : MonoBehaviour
{
    [SerializeField] private Slider instantBar;
    [SerializeField] private Slider delayedBar;


    public void UpdateHealth(float current, float delayed, float max)
    {
        instantBar.value = current / max;
        delayedBar.value = delayed / max;
    }
}
