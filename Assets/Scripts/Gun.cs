using UnityEngine;

public class Gun : MonoBehaviour
{
    /// <summary>
    /// Автоматическое ли оружие
    /// </summary>
    public bool isAutomatic;
    
    /// <summary>
    /// Время между выстрелами
    /// </summary>
    public float timeBetweenShots = .1f;
    
    /// <summary>
    /// Нагрев при каждом выстреле
    /// </summary>
    public float heatPerShot = 1f;

    /// <summary>
    /// Дульная вспышка
    /// </summary>
    public GameObject muzzleFlash;
}
