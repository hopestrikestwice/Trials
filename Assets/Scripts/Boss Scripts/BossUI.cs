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

    [Tooltip("UI Slider to display Boss' Health")]
    [SerializeField]
    private Slider bossHealthSlider;

    Transform bossTransform;
    Renderer bossRenderer;

    private void Awake()
    {
        this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);

        // _canvasGroup = this.GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (boss != null)
        {
            // Reflect the Boss Health
            if (bossHealthSlider != null)
            {
                bossHealthSlider.value = boss.GetHealth();
            }
        }
        // else
        // {
        //     boss = GameObject.Find("Kraken").GetComponent<KrakenManager>();
        // }
    }

    public void SetTarget(KrakenManager _boss)
    {
        if (_boss == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for KrakenUI.SetTarget.", this);
            return;
        }

        //Cache references for efficiency
        boss = _boss;

        bossTransform = this.boss.GetComponent<Transform>();
        bossRenderer = this.boss.GetComponent<Renderer>();
    }
}
