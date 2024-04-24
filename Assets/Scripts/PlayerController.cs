using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// "точка зрения"
    /// </summary>
    public Transform viewPoint;

    /// <summary>
    /// Сенса мышки
    /// </summary>
    public float mouseSensitivity = 1f;

    /// <summary>
    /// Максимальное отклонение по вертикали
    /// </summary>
    private float verticalRotationStore;

    /// <summary>
    /// Vector2 - потому что мышка по горизонтали и вертикали работает
    /// </summary>
    private Vector2 mouseInput;

    /// <summary>
    /// Скорость ходьбы
    /// </summary>
    public float walkSpeed = 5f;
    
    /// <summary>
    /// Скорость бега
    /// </summary>
    public float runSpeed = 10f;

    /// <summary>
    /// Скорость передвижения
    /// </summary>
    private float moveSpeed;

    /// <summary>
    /// Направление движения
    /// </summary>
    private Vector3 moveDirection;

    /// <summary>
    /// Движение
    /// </summary>
    private Vector3 movement;
    
    /// <summary>
    /// Камера отдельно, чтобы после смерти персонажа она не "исчезала" = "выключалась"
    /// </summary>
    private Camera camera;
    
    /// <summary>
    /// Сила прыжка 
    /// </summary>
    public float jumpForce = 7f;

    /// <summary>
    /// Модификация гравитации
    /// </summary>
    public float gravityMod = 2f;

    /// <summary>
    /// Стоит на земле
    /// </summary>
    public bool isGrounded = false;

    /// <summary>
    /// Точка основания
    /// </summary>
    public Transform groundCheckPoint;
    
    /// <summary>
    /// Слой для ходьбы
    /// </summary>
    public LayerMask groundLayers;
    
    /// <summary>
    /// Управление персонажем: передвижение
    /// </summary>
    public CharacterController characterController;

    /// <summary>
    /// След от выстрела
    /// </summary>
    public GameObject bulletImpact;

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
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Confined - курсор может находиться только в окне "игры", Lock - пропадает курсор и залочен в центре экрана
        camera = Camera.main; // главная камера
        
        UIController.instance.overheatedMessage.gameObject.SetActive(false);
        UIController.instance.gunTempSlider.maxValue = gunMaxHeat;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: перенести на контролы
        // получаем значения мышки
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
        
        // поворачиваем налево-направо "тело"
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
        
        if (viewPoint is null)
            throw new NullReferenceException("ViewPoint is null or empty");

        verticalRotationStore += mouseInput.y;
        
        // ограничение по зрению вверх вниз
        verticalRotationStore = Mathf.Clamp(verticalRotationStore, -60f, 60f);

        // поворачиваем вверх-низ "голову" модельки с камерой
        viewPoint.rotation = Quaternion.Euler(-verticalRotationStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        
        // TODO: перенести на контролы
        // получаем направление движения
        moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        // бег
        moveSpeed = Input.GetKeyDown(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        var yVelocity = movement.y;
        
        // давижение по направлению взгляда
        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * moveSpeed ; 

        // гравитация 
        movement.y = yVelocity;

        if (characterController.isGrounded)
        {
            movement.y = 0f;
        }

        // проверка что персонаж стоит на поверхности
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers);
        
        // TODO: перенести на контролы
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            movement.y = jumpForce;
        }

        // применение гравитации
        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;
        
        // движение
        characterController.Move(movement * Time.deltaTime);
        
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
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
            
            // если зажим - авто стрельба
            if (Input.GetMouseButton(0) && guns[gunInUse].isAutomatic)
            {
                shotCounter -= Time.deltaTime;
            
                // продолжаем стрельбу
                if (shotCounter <= 0) 
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

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            // TODO: test
            gunInUse = gunInUse == guns.Length - 1 ? 0 : gunInUse + 1;
            SwitchGun();
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            // TODO: test
            gunInUse = gunInUse == 0 ? guns.Length - 1 : gunInUse - 1;
            SwitchGun();
        }
        
        // отобразить мышку если нажат esc
        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        if (Cursor.lockState == CursorLockMode.None && Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void LateUpdate()
    {
        camera.transform.position = viewPoint.position;
        camera.transform.rotation = viewPoint.rotation;
    }

    private void Shoot()
    {
        var ray = camera.ViewportPointToRay(new Vector3(.5f, .5f, 0f)); // .5f .5f это середина экрана
        ray.origin = camera.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit)) // если raycast во что-то упёрся
        {
            var bulletImpactRotation = Quaternion.LookRotation(hit.normal, Vector3.up); // поворот префаба на поверхности TODO: лучше изучить
            var bulletImpactPosition = hit.point + hit.normal * .02f; // расположение префаба - поверхность hit - .02f чтобы на поверхности
            var bulletImpactGameObject = Instantiate(bulletImpact, bulletImpactPosition , bulletImpactRotation);
            Destroy(bulletImpactGameObject, 10f); // уничтожить объект через секунд. чтобы не засорять память
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
        
        guns[gunInUse].muzzleFlash.SetActive(false);
    }
    
}