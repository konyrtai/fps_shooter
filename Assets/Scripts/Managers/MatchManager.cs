using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Domain;
using Domain.Enums;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;
using EventType = Domain.Enums.EventType;

public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static MatchManager instance;

    private void Awake()
    {
        instance = this;
    }
    
    /// <summary>
    /// Игроки в матче
    /// </summary>
    public List<PlayerInfo> players = new();

    /// <summary>
    /// Количество убийств до победы
    /// </summary>
    public int killsToWin = 5;

    /// <summary>
    /// Состояние игры 
    /// </summary>
    public GameState state = GameState.Waiting;

    /// <summary>
    /// Сколько ожидать после окончания игры
    /// </summary>
    public float waitAfterEndingInSeconds = 5;
    
    private void Start()
    {
        // если нет подключения
        if (PhotonNetwork.IsConnected == false)
        {
            SceneManager.LoadScene(0); // переходим в главное меню
            state = GameState.Waiting;
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName); // если подключились, отправляем инфу о новом игроке
            state = GameState.Playing;
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200) // значения больше 200 зарезервированы photon
        {
            var code = (Domain.Enums.EventType)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            switch (code)
            {
                case EventType.NewPlayer:
                    NewPlayerReceived(data);
                    break;
                case EventType.ListPlayers:
                    ListPlayersReceived(data);
                    break;
                case EventType.UpdateStats:
                    UpdateStatsReceived(data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this); // прослушивать события, вызывается OnEvent
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// Отправить данные о новом игроке
    /// </summary>
    /// <param name="username"></param>
    public void NewPlayerSend(string username)
    {
        var package = new object[4];

        // согласно playerInfo
        package[0] = username; // имя
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber; // id
        package[2] = 0; // kills
        package[3] = 0; // deaths

        // посылаем событие по сети
        PhotonNetwork.RaiseEvent((byte)Domain.Enums.EventType.NewPlayer,
            package,
            new RaiseEventOptions
            {
                Receivers = ReceiverGroup.MasterClient // одна группа с мастер клиентом
            },
            new SendOptions
            {
                Reliability = true
            });
    }

    /// <summary>
    /// Данные о новом игроке
    /// </summary>
    /// <param name="data"></param>
    public void NewPlayerReceived(object[] data)
    {
        var playerInfo = new PlayerInfo(name: (string)data[0], actor: (int)data[1], killsCount: (int)data[2],
            deathsCount: (int)data[3]);
        players.Add(playerInfo);

        ListPlayersSend();
    }

    /// <summary>
    /// Отправить новый список игроков
    /// </summary>
    public void ListPlayersSend()
    {
        var package = new object[players.Count + 1];
        package[0] = state; // send game state
        
        for (int i = 0; i < players.Count; i++)
        {
            var playerPackage = new object[4];
            playerPackage[0] = players[i].Name;
            playerPackage[1] = players[i].Actor;
            playerPackage[2] = players[i].KillsCount;
            playerPackage[3] = players[i].DeathsCount;
            package[i + 1] = playerPackage; // send player package
        }

        PhotonNetwork.RaiseEvent((byte)Domain.Enums.EventType.ListPlayers,
            package,
            new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All // одна группа с мастер клиентом
            },
            new SendOptions
            {
                Reliability = true
            });
    }

    /// <summary>
    /// Получен список игроков
    /// </summary>
    /// <param name="data"></param>
    public void ListPlayersReceived(object[] data)
    {
        state = (GameState)data[0];
        
        players.Clear();
        for (int i = 1; i < data.Length; i++)
        {
            var playerPackage = data[i] as object[];

            var playerInfo = new PlayerInfo(name: (string)playerPackage[0], actor: (int)playerPackage[1],
                killsCount: (int)playerPackage[2], deathsCount: (int)playerPackage[3]);

            players.Add(playerInfo);
        }

        StateCheck();
    }

    public void UpdateStatsSend(int actorIdSending, StatType statToUpdate, int amountToChange)
    {
        var package = new object[3];
        package[0] = actorIdSending;
        package[1] = statToUpdate;
        package[2] = amountToChange;

        PhotonNetwork.RaiseEvent((byte)Domain.Enums.EventType.UpdateStats,
            package,
            new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All // одна группа с мастер клиентом
            },
            new SendOptions
            {
                Reliability = true
            });
    }

    /// <summary>
    /// Получена обновленная статистика
    /// </summary>
    /// <param name="data"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void UpdateStatsReceived(object[] data)
    {
        var actor = (int)data[0];
        var statType = (StatType)data[1];
        var amount = (int)data[2];

        // обновляем статистику игрока
        var player = players.FirstOrDefault(x => x.Actor == actor);
        if (player != null)
        {
            switch (statType)
            {
                case StatType.Kills:
                    player.KillsCount += amount;
                    break;
                case StatType.Deaths:
                    player.DeathsCount += amount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        // обновляем UI если статистика для локального игрока
        if (PhotonNetwork.LocalPlayer.ActorNumber == actor)
        {
            var currentPlayer = players.FirstOrDefault(x => x.Actor == actor);
            if(currentPlayer == null) return;
            
            switch (statType)
            {
                case StatType.Kills:
                    UIController.instance.UpdateKillsCount(currentPlayer.KillsCount);
                    break;
                case StatType.Deaths:
                    UIController.instance.UpdateDeathsCount(currentPlayer.DeathsCount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        CheckVictory();
    }

    /// <summary>
    ///  Если игрок покинул игру
    /// </summary>
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        SceneManager.LoadScene(0); // перейти в главное меню
    }

    /// <summary>
    /// Проверка победы
    /// </summary>
    void CheckVictory()
    {
        var winner = players.FirstOrDefault(x => x.KillsCount >= killsToWin);
        if(winner is null) return;
        
        // если игрок управляет лобби и игра ещё не заканчивается
        if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
        {
            state = GameState.Ending;
            ListPlayersSend();
        }
    }

    /// <summary>
    /// Проверить состояние игры
    /// </summary>
    void StateCheck()
    {
        if (state == GameState.Ending)
        {
            EndGame();
        }
    }

    /// <summary>
    /// Завершить игру
    /// </summary>
    void EndGame()
    {
        state = GameState.Ending;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll(); // уничтожаем объекты по сети
        }
        
        UIController.instance.ShowMatchOverScreen();

        StartCoroutine(EndCoroutine());
    }

    IEnumerator EndCoroutine()
    {
        yield return new WaitForSeconds(waitAfterEndingInSeconds);

        PhotonNetwork.AutomaticallySyncScene = false; // отключить синхронизацию сцен
        PhotonNetwork.LeaveRoom(); // 
    }
}