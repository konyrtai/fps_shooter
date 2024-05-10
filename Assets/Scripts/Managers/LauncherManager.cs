using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LauncherManager : MonoBehaviourPunCallbacks
{
    public static LauncherManager instance;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Экран загрузки
    /// </summary>
    public GameObject loadingScreen;

    /// <summary>
    /// Текст загрузки
    /// </summary>
    public TMP_Text loadingText;

    /// <summary>
    /// Главное меню с кнопками
    /// </summary>
    public GameObject menuButtons;

    /// <summary>
    /// Экран создания комнаты
    /// </summary>
    public GameObject createRoomScreen;

    /// <summary>
    /// Название комнаты - экран создания комнаты
    /// </summary>
    public TMP_Text roomNameInputCreateRoomScreen;

    /// <summary>
    /// Экран с подключенной комнатой
    /// </summary>
    public GameObject roomScreen;

    /// <summary>
    /// Наименование подключенной комнаты
    /// </summary>
    public TMP_Text roomNameText;

    /// <summary>
    /// Экран с отображением ошибки
    /// </summary>
    public GameObject errorScreen;

    /// <summary>
    /// Текст с ошибкой
    /// </summary>
    public TMP_Text errorText;

    /// <summary>
    /// Экран со списком комнат
    /// </summary>
    public GameObject roomBrowserScreen;

    /// <summary>
    /// Подключение к комнате
    /// </summary>
    public RoomButton roomButton;

    /// <summary>
    /// Все комнаты
    /// </summary>
    public List<RoomButton> allRoomButtons = new List<RoomButton>();

    /// <summary>
    /// Имя игрока
    /// </summary>
    public TMP_Text playerName;

    /// <summary>
    /// Игроки в комнате
    /// </summary>
    public List<TMP_Text> playersInRoom = new List<TMP_Text>();

    /// <summary>
    /// Экран смены имени
    /// </summary>
    public GameObject changeNameScreen;

    /// <summary>
    /// Имя игрока - экран смены имени
    /// </summary>
    public TMP_Text playerNameInputFieldValue;

    /// <summary>
    /// Какой уровень запустить
    /// </summary>
    public string levelToPlay;

    /// <summary>
    /// Кнопка "начать игру"
    /// </summary>
    public GameObject startGameButton;

    void Start()
    {
        OpenLoadingScreen("CONNECTING TO SERVER...");

        // подключаемся к PUN серверу, используя Photon Server Settings
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// Подключились к серверу
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(); //
        PhotonNetwork.AutomaticallySyncScene = true; // синхронизировать сцены, начало игры
        OpenLoadingScreen("JOINING LOBBY...");
    }

    /// <summary>
    /// Если хост ливнул и теперь другой игрок хост
    /// </summary>
    /// <param name="newMasterClient"></param>
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdateRoomInfo();
    }

    /// <summary>
    /// Подключились к лобби
    /// </summary>
    public override void OnJoinedLobby()
    {
        OpenMainScreen();
        CheckPlayerName();
    }

    /// <summary>
    /// Создать комнату
    /// </summary>
    public void CreateRoom()
    {
        var roomName = roomNameInputCreateRoomScreen.text;
        if (string.IsNullOrEmpty(roomName) || string.IsNullOrWhiteSpace(roomName))
            return;

        var options = new RoomOptions
        {
            MaxPlayers = 4,
        };
        PhotonNetwork.CreateRoom(roomName, options);
        OpenLoadingScreen("CREATING ROOM...");
    }

    /// <summary>
    /// При подключении к комнате
    /// </summary>
    public override void OnJoinedRoom()
    {
        CloseMenus();
        roomScreen.SetActive(true);
        roomNameText.text = PhotonNetwork.CurrentRoom.Name; // имя комнаты
        UpdateRoomInfo();
    }

    /// <summary>
    /// Обновить информацию о комнате
    /// </summary>
    private void UpdateRoomInfo()
    {
        DisplayAllPlayers();
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient); // чтобы кнопку мог запустать только тот кто создал комнату
    }

    /// <summary>
    /// Отобразить всех игроков в комнате
    /// </summary>
    private void DisplayAllPlayers()
    {
        if (PhotonNetwork.IsConnected == false)
            return;

        if (PhotonNetwork.CurrentRoom == null)
            return;

        // очищаем список
        foreach (var playerInRoom in playersInRoom)
        {
            Destroy(playerInRoom.gameObject);
        }

        playersInRoom.Clear();

        // отображение списка игроков
        foreach (var playerInRoom in PhotonNetwork.PlayerList)
        {
            var playerText = Instantiate(playerName, playerName.transform.parent);
            playerText.text = playerInRoom.NickName;
            playerText.gameObject.SetActive(true);
            playersInRoom.Add(playerText);
        }
    }

    /// <summary>
    /// Ошибка при создании комнаты
    /// </summary>
    /// <param name="returnCode"></param>
    /// <param name="message"></param>
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        OpenErrorScreen("FAILED TO CREATE ROOM: " + message);
    }

    /// <summary>
    /// Покинуть комнату
    /// </summary>
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        OpenLoadingScreen("LEAVING ROOM...");
    }

    /// <summary>
    /// Покинул комнату
    /// </summary>
    public override void OnLeftRoom()
    {
        OpenMainScreen();
    }

    /// <summary>
    /// Обновление списка комнат с сервера
    /// </summary>
    /// <param name="roomList"></param>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // очищаем список в UI 
        foreach (var room in allRoomButtons)
        {
            Destroy(room.gameObject);
        }

        allRoomButtons.Clear();
        roomButton.gameObject.SetActive(false);

        foreach (var room in roomList)
        {
            // в комнате макимальное количество игроков
            if (room.PlayerCount == room.MaxPlayers)
                continue;

            // ещё не уничтожена на сервере
            if (room.RemovedFromList)
                continue;

            // кнопка для отображения комнаты
            var roomB = Instantiate(roomButton, roomButton.transform.parent);
            roomB.gameObject.SetActive(true);
            roomB.SetButtonDetails(room);
            allRoomButtons.Add(roomB);
        }
    }

    /// <summary>
    /// Присоединиться к комнате
    /// </summary>
    /// <param name="roomInfo"></param>
    public void JoinRoom(RoomInfo roomInfo)
    {
        // подключаемся к комнате
        PhotonNetwork.JoinRoom(roomInfo.Name);

        CloseMenus();

        OpenLoadingScreen("JOINING ROOM: " + roomInfo.Name);
    }

    /// <summary>
    /// Зашел игрок
    /// </summary>
    /// <param name="newPlayer"></param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateRoomInfo();
    }

    /// <summary>
    /// Вышел игрок
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateRoomInfo();
    }

    /// <summary>
    /// Начать игру
    /// </summary>
    public void StartGame()
    {
        PhotonNetwork.LoadLevel(levelToPlay);
    }


    #region Screens

    /// <summary>
    /// Отключить все меню
    /// </summary>
    public void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
        changeNameScreen.SetActive(false);
    }

    /// <summary>
    /// Экран создания комнаты
    /// </summary>
    public void OpenCreateRoomScreen()
    {
        CloseMenus();
        createRoomScreen.SetActive(true);
    }

    /// <summary>
    /// Экран загрузки
    /// </summary>
    /// <param name="text">Текст загрузки</param>
    public void OpenLoadingScreen(string text)
    {
        CloseMenus();
        loadingText.text = text;
        loadingScreen.SetActive(true);
    }

    /// <summary>
    /// Экран с ошибкой
    /// </summary>
    /// <param name="text">Текст ошибки</param>
    private void OpenErrorScreen(string text)
    {
        CloseMenus();
        errorText.text = text;
        errorScreen.SetActive(true);
    }

    /// <summary>
    /// Открыть главный экран
    /// </summary>
    public void OpenMainScreen()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    /// <summary>
    /// Открыть экран с комнатами
    /// </summary>
    public void OpenRoomsBrowserScreen()
    {
        CloseMenus();
        roomBrowserScreen.SetActive(true);
    }

    /// <summary>
    /// Открыть окно смены имени
    /// </summary>
    public void OpenChangeNameScreen()
    {
        CloseMenus();
        changeNameScreen.SetActive(true);
    }

    /// <summary>
    /// Выйти из игры
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    #endregion


    #region Player Name

    /// <summary>
    /// Проверить имя игрока
    /// </summary>
    private void CheckPlayerName()
    {
        if (PlayerPrefs.HasKey("playerName") == false)
        {
            OpenChangeNameScreen();
            return;
        }

        var _playerName = PlayerPrefs.GetString("playerName");
        if (string.IsNullOrEmpty(_playerName) || string.IsNullOrWhiteSpace(_playerName))
        {
            OpenChangeNameScreen();
            return;
        }

        SetPlayerName(_playerName);
    }

    /// <summary>
    /// Сменить имя игрока
    /// </summary>
    public void ChangePlayerName()
    {
        if (string.IsNullOrEmpty(playerNameInputFieldValue.text))
            return;

        SetPlayerName(playerNameInputFieldValue.text);
        OpenMainScreen();
    }

    /// <summary>
    /// Записать имя игрока
    /// </summary>
    /// <param name="playerName"></param>
    private void SetPlayerName(string name)
    {
        // записываем ник в сеть
        PhotonNetwork.NickName = name;

        // сохраняем ник
        PlayerPrefs.SetString("playerName", name);
    }
    #endregion
}