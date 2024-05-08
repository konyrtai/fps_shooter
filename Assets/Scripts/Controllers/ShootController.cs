using System;
using Controllers;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShootController : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// Аниматор
    /// </summary>
    public Animator animator;
    
    /// <summary>
    /// Ввод с джойстиков
    /// </summary>
    [SerializeField] public PlayerControls playerInput;
    
    /// <summary>
    /// Камера отдельно, чтобы после смерти персонажа она не "исчезала" = "выключалась"
    /// </summary>
    private Camera camera;
    
    /// <summary>
    /// След от выстрела на стенах
    /// </summary>
    public GameObject bulletImpact;

    /// <summary>
    /// След от попадания по игроку
    /// </summary>
    public GameObject playerHitImpact;
    
    /// <summary>
    /// Время между выстрелам
    /// </summary>
    public float timeBetweenShots = 0.1f;

    /// <summary>
    /// Счётчик выстрелов
    /// </summary>
    private float shotCounter;

    /// <summary>
    /// Максимальный перегрев оружия
    /// </summary>
    private float gunMaxHeat = 20f;

    /// <summary>
    /// Нагрев во время каждого выстрела
    /// </summary>
    private float gunHeatPerShot = 1f;

    /// <summary>
    /// Скорость остывания оружия во время бездействия
    /// </summary>
    private float gunCoolRate = 4f;

    /// <summary>
    /// Скорость остывания во время перегрева
    /// </summary>
    private float gunOverheatCoolRate = 5f;

    /// <summary>
    /// Счётчик перегрева
    /// </summary>
    private float gunHeatCounter;

    /// <summary>
    /// Признак перегрева оружия
    /// </summary>
    private bool gunOverHeated;

    /// <summary>
    /// Всё оружие
    /// </summary>
    public Gun[] guns;

    /// <summary>
    /// Используемое оружие
    /// </summary>
    public int gunInUse = 0;

    /// <summary>
    /// Время отображения дульной вспышки
    /// </summary>
    public float muzzleFlashDisplayTime = .02f;

    /// <summary>
    /// Счётчик
    /// </summary>
    public float muzzleFlashCounter;
    
    /// <summary>
    /// Здоровье
    /// </summary>
    private PlayerController playerController;
    
    // Start is called before the first frame update
    void Start()
    {
        UIController.instance.overheatedMessage.gameObject.SetActive(false);
        UIController.instance.gunTempSlider.maxValue = gunMaxHeat;
        
        camera = Camera.main; // главная камера
        guns[gunInUse].muzzleFlash.SetActive(false); // отключить дульную вспышку

        playerController = gameObject.GetComponent<PlayerController>();
        
        playerInput = new PlayerControls();
        playerInput.Enable();
        
        playerInput.Player.Nextweapon.performed += ChangeWeapon;
        playerInput.Player.Previousweapon.performed += ChangeWeapon;
        
        photonView.RPC(nameof(SetGun), RpcTarget.All, gunInUse);
    }

    private void OnDestroy()
    {
        playerInput.Player.Nextweapon.performed -= ChangeWeapon;
        playerInput.Player.Previousweapon.performed -= ChangeWeapon;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: test
        if (photonView.IsMine == false) 
            return;
        
        // выключаем дульную вспышку
        if (guns[gunInUse].muzzleFlash.activeInHierarchy) // если включена
        {
            muzzleFlashCounter -= Time.deltaTime;
            if (muzzleFlashCounter <= 0)
            {
                guns[gunInUse].muzzleFlash.SetActive(false);
            }
        }
        
        // проверка на перегрев
        if (!gunOverHeated)
        {
            // стрелять по нажатию
            var enemyInCrossHair = IsEnemyInCrosshair();
            if (enemyInCrossHair.HasValue)
            {
                if (guns[gunInUse].isAutomatic)
                {
                    shotCounter -= Time.deltaTime;
            
                    // продолжаем стрельбу
                    if (shotCounter <= 0) 
                    {
                        Shoot();
                    }
                }
                else
                {
                    Shoot();
                }
            }

            gunHeatCounter -= gunCoolRate * Time.deltaTime;
        }
        else
        {
            gunHeatCounter -= gunOverheatCoolRate * Time.deltaTime;
        }

        // если остыл
        if (gunHeatCounter < 0)
        {
            gunHeatCounter = 0;
            gunOverHeated = false;

            UIController.instance.overheatedMessage.gameObject.SetActive(false);
        }
        
        UIController.instance.gunTempSlider.value = gunHeatCounter;
    }

    /// <summary>
    /// Луч середина экрана
    /// </summary>
    /// <returns></returns>
    private Ray CrosshairRay()
    {
        var ray = camera.ViewportPointToRay(new Vector3(.5f, .5f, 0f)); // .5f .5f это середина экрана
        ray.origin = camera.transform.position;
        return ray;
    } 

    /// <summary>
    /// Враг в прицеле
    /// </summary>
    private RaycastHit? IsEnemyInCrosshair()
    {
        if(photonView.IsMine == false)
            return null;

        // если raycast упёрся в игрока
        return Physics.Raycast(CrosshairRay(), out RaycastHit hit) && hit.collider.CompareTag("Player")
            ? hit
            : null;
    }
    
    private void Shoot()
    {
        // управление только своим персонажем
        if (photonView.IsMine == false) 
            return;

        var enemyHit = IsEnemyInCrosshair();
        
        if (enemyHit.HasValue) 
        {
            PhotonNetwork.Instantiate(playerHitImpact.name, enemyHit.Value.point, Quaternion.identity); // создать "получение урона" на цели через photon
            enemyHit.Value.collider.gameObject.GetPhotonView().RPC(nameof(DealDamage), RpcTarget.All, photonView.Owner.NickName, guns[gunInUse].damageAmount, PhotonNetwork.LocalPlayer.ActorNumber); // нанесение урона

            // TODO: подумать что делать со следами от пуль на объектах
            // var bulletImpactRotation = Quaternion.LookRotation(enemyHit.Value.normal, Vector3.up); // поворот префаба на поверхности TODO: лучше изучить
            // var bulletImpactPosition = enemyHit.Value.point + enemyHit.Value.normal * .02f; // расположение префаба - поверхность hit - .02f чтобы на поверхности
            // var bulletImpactGameObject = Instantiate(bulletImpact, bulletImpactPosition , bulletImpactRotation);
            // Destroy(bulletImpactGameObject, 10f); // уничтожить объект через секунд. чтобы не засорять память
        }

        // задержка между выстрелами
        shotCounter = timeBetweenShots;

        // нагрев оружия
        gunHeatCounter += gunHeatPerShot;

        // если перегрелся
        if (gunHeatCounter >= gunMaxHeat)
        {
            gunHeatCounter = gunMaxHeat;

            gunOverHeated = true;
            
            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }
        
        // дульная вспышка
        guns[gunInUse].muzzleFlash.SetActive(true);
        muzzleFlashCounter = muzzleFlashDisplayTime;
    }
    
     /// <summary>
     /// Получить урон - RPC
     /// </summary>
     /// <param name="damageFromNickname">Кто нанес урон</param>
     /// <param name="damageAmount">Количество урона</param>
    [PunRPC]
    public void DealDamage(string damageFromNickname, int damageAmount, int actorId)
    {
        if (photonView.IsMine && playerController != null) 
        {
            playerController.TakeDamage(damageAmount, damageFromNickname, actorId);
        }
    }

     /// <summary>
     /// Обновление анимации
     /// </summary>
    private void UpdateAnimator()
    {
        var gun = guns[gunInUse];
        animator.SetBool("is_rifle", gun.IsRifle);
        animator.SetBool("is_pistol", !gun.IsRifle);
    }

    #region Switch weapon
    
    /// <summary>
    /// Event holder для кнопок смены оружия
    /// </summary>
    /// <param name="context"></param>
    private void ChangeWeapon(InputAction.CallbackContext context)
    {
        gunInUse = gunInUse == 0 ? guns.Length - 1 : gunInUse - 1;
        // SwitchGun();
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(SetGun), RpcTarget.All, gunInUse);
        }
    }
    
    /// <summary>
    /// Сменить оружие
    /// </summary>
    private void SwitchGun()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            guns[i].gameObject.SetActive(i == gunInUse);
            guns[i].muzzleFlash.SetActive(false);
        }

        var gun = guns[gunInUse];
        
        timeBetweenShots = gun.timeBetweenShots;
        gunHeatPerShot = gun.heatPerShot;
        
        UpdateAnimator();
    }

    /// <summary>
    /// Сменить оружие по сети
    /// </summary>
    /// <param name="gunToSwitch"></param>
    [PunRPC]
    public void SetGun(int gunToSwitch)
    {
        if (gunToSwitch < guns.Length)
        {
            gunInUse = gunToSwitch;
            SwitchGun();
        }
    }
    #endregion
}
