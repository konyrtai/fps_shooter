using System.Collections;
using System.Threading.Tasks;
using Controllers;
using Domain.Enums;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager instance;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// префаб для создания игрока
    /// </summary>
    public GameObject playerPrefab;

    /// <summary>
    /// Созданный игрок
    /// </summary>
    private GameObject player;

    /// <summary>
    /// Эффект смерти
    /// </summary>
    public GameObject playerDeathEffect;

    private void Start()
    {
        if (PhotonNetwork.IsConnected == false)
            return;

        SpawnPlayer();
    }

    /// <summary>
    /// Заспавнить игрока по сети
    /// </summary>
    public void SpawnPlayer()
    {
        UIController.instance.CloseDeathScreen();
        var spawnPoint = SpawnManager.instance.GetSpawnPoint(); // точка возрождения
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        var playerController = player.GetComponent<PlayerController>();
        playerController.health = 100; // устанавливаем здоровье
    }

    /// <summary>
    /// Убить игрока
    /// </summary>
    public void Die(string killer)
    {
        PhotonNetwork.Instantiate(playerDeathEffect.name, player.transform.position, Quaternion.identity); // эффект смерти
        UIController.instance.ShowDeathScreen(killer); // экран смерти
        PhotonNetwork.Destroy(player); // уничтожить модель
        
        MatchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, StatType.Deaths, 1);

        if (player is not null)
        {
            StartCoroutine(RespawnTimer()); // респаун после таймера
        }
    }

    /// <summary>
    /// Таймер возрождения
    /// </summary>
    /// <returns></returns>
    private IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(5);
        SpawnPlayer(); // возродить игрока
    }
}