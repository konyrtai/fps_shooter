using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public static MatchManager instance;

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
    }
}
