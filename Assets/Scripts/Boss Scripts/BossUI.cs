/// BossUI.cs
/// 
/// Updates UI elements with boss stats.
///

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    private BossManagerCore boss;

    [Tooltip("UI Slider to display Player's Health")]
    [SerializeField]
    private Slider bossHealthSlider;

    private void Update()
    {
        if (boss != null)
        {
            // Reflect the Player Health
            if (bossHealthSlider != null)
            {
                bossHealthSlider.value = boss.GetHealth();
            }
        }
        else
        {
            boss = GameObject.FindWithTag("Boss").GetComponent<BossManagerCore>();
        }
    }
}
