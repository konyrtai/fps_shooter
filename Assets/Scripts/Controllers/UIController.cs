using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        deathScreen.SetActive(false);
    }


    /// <summary>
    /// Сообщение о перегреве
    /// </summary>
    public TMP_Text overheatedMessage;
    
    /// <summary>
    /// Слайдер с температурой оружия
    /// </summary>
    public Slider gunTempSlider;

    /// <summary>
    /// Экран смерти
    /// </summary>
    public GameObject deathScreen;

    /// <summary>
    /// Сообщение о смерти
    /// </summary>
    public TMP_Text deathScreenMessage;

    /// <summary>
    /// Слайдер со здоровьем
    /// </summary>
    public Slider healthSlider;
    
    /// <summary>
    /// Показать сообщение о смерти
    /// </summary>
    /// <param name="killer"></param>
    public void ShowDeathScreen(string killer)
    {
        deathScreen.SetActive(true);
        deathScreenMessage.text = "WASTED. KILLER: " + killer;
    }

    /// <summary>
    /// Отключить сообщение о смерти
    /// </summary>
    public void CloseDeathScreen()
    {
        deathScreen.SetActive(false);
    }
}
