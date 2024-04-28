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
    /// Наносимый урон
    /// </summary>
    public int damageAmount = 10;
    
    /// <summary>
    /// Дульная вспышка
    /// </summary>
    public GameObject muzzleFlash;
    
    /// <summary>
    /// Винтовка
    /// </summary>
    public bool IsRifle;
}
