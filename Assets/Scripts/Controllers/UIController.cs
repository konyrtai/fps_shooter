using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Domain;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
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
        CloseDeathScreen();
        CloseLeaderboard();
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
    /// Количество убийств
    /// </summary>
    public TMP_Text killsCountText;

    /// <summary>
    /// Количество смертей
    /// </summary>
    public TMP_Text deathsCountText;

    /// <summary>
    /// Таблица лидеров
    /// </summary>
    public GameObject leaderboardPanel;

    /// <summary>
    /// Игрок в таблице лидеров 
    /// </summary>
    public PlayerLeaderboard leaderboardPlayerDisplay;

    /// <summary>
    /// Список игроков в таблице лидеров
    /// </summary>
    public List<PlayerLeaderboard> leaderboardPlayers = new List<PlayerLeaderboard>();

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

    /// <summary>
    /// Обновить счетчик убийств
    /// </summary>
    /// <param name="killsCount"></param>
    public void UpdateKillsCount(int killsCount)
    {
        killsCountText.text = "KILLS: " + killsCount;
    }

    /// <summary>
    /// Обновить счетчик смертей
    /// </summary>
    /// <param name="deathsCount"></param>
    public void UpdateDeathsCount(int deathsCount)
    {
        deathsCountText.text = "DEATHS: " + deathsCount;
    }

    /// <summary>
    /// Отобразить список лидеров
    /// </summary>
    public void ShowLeaderboard()
    {
        if(leaderboardPanel.activeInHierarchy) return; // если уже отображается
        
        ClearLeaderboard();
        
        foreach (var player in MatchManager.instance.players.OrderByDescending(p => p.KillsCount))
        {
            var leaderboardPlayer = Instantiate(leaderboardPlayerDisplay, leaderboardPlayerDisplay.transform.parent);
            leaderboardPlayer.Set(player.Name, player.KillsCount, player.DeathsCount);
            leaderboardPlayer.gameObject.SetActive(true);
            leaderboardPlayers.Add(leaderboardPlayer);
        }
        
        leaderboardPanel.SetActive(true);
    }
    

    /// <summary>
    /// Закрыть таблицу лидеров
    /// </summary>
    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
        ClearLeaderboard();
    }

    /// <summary>
    /// Очистить таблицу лидеров
    /// </summary>
    private void ClearLeaderboard()
    {
        foreach (var leaderboardPlayer in leaderboardPlayers)
        {
            leaderboardPlayer.gameObject.SetActive(false);
            Destroy(leaderboardPlayer);
        }
        
        leaderboardPlayers.Clear();
        
        leaderboardPlayerDisplay.gameObject.SetActive(false);
    }
}
