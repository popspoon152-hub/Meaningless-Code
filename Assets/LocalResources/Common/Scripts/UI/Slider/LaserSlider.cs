using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaserSlider : MonoBehaviour
{
    [SerializeField] private Slider LaserBar;

    public void UpdateCD(float current, float max)
    {
        LaserBar.value = max - current;
    }
}
