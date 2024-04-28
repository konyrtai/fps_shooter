using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Точки возрождения
    /// </summary>
    public Transform[] spawnPositions;
    void Start()
    {
        // чтобы не было видно точки возрождения
        foreach (var spawn in spawnPositions)
        {
            spawn.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Получить точку возрождения
    /// </summary>
    public Transform GetSpawnPoint()
    {
        var randomValue = Random.Range(0, spawnPositions.Length);
        var spawnPosition = spawnPositions[randomValue];
        return spawnPosition;
    } 
}
