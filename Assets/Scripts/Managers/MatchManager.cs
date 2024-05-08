using System;
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

    public List<PlayerInfo> players = new List<PlayerInfo>();
    private int currentPlayerIndex;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // если нет подключения
        if (PhotonNetwork.IsConnected == false)
        {
            SceneManager.LoadScene(0); // переходим в главное меню
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName); // если подключились, отправляем инфу о новом игроке
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

    public void NewPlayerReceived(object[] data)
    {
        var playerInfo = new PlayerInfo(name: (string)data[0], actor: (int)data[1], killsCount: (int)data[2],
            deathsCount: (int)data[3]);
        players.Add(playerInfo);

        ListPlayersSend();
    }

    public void ListPlayersSend()
    {
        var package = new object[players.Count];
        for (int i = 0; i < players.Count; i++)
        {
            var playerPackage = new object[4];
            playerPackage[0] = players[i].Name;
            playerPackage[1] = players[i].Actor;
            playerPackage[2] = players[i].KillsCount;
            playerPackage[3] = players[i].DeathsCount;
            package[i] = playerPackage;
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

    public void ListPlayersReceived(object[] data)
    {
        players.Clear();
        for (int i = 0; i < data.Length; i++)
        {
            var playerPackage = data[i] as object[];

            var playerInfo = new PlayerInfo(name: (string)playerPackage[0], actor: (int)playerPackage[1],
                killsCount: (int)playerPackage[2], deathsCount: (int)playerPackage[3]);

            players.Add(playerInfo);

            if (PhotonNetwork.LocalPlayer.ActorNumber == playerInfo.Actor)
            {
                currentPlayerIndex = i;
            }
        }
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
                    Debug.Log("Player: " + player.Name + " killsCount: " + player.KillsCount);
                    break;
                case StatType.Deaths:
                    player.DeathsCount += amount;
                    Debug.Log("Player: " + player.Name + " deathsCount: " + player.DeathsCount);
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
    }
}