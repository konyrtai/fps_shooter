using System;
using System.Collections;
using System.Collections.Generic;
using Domain.Consts;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviourPunCallbacks
{
    /// <summary>
    /// Анимация
    /// </summary>
    public Animator animator;

    /// <summary>
    /// Модель игрока
    /// </summary>
    public GameObject playerModel;
    
    /// <summary>
    /// Ввод с джойстиков
    /// </summary>
    [SerializeField] private PlayerInput playerInput;

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
    public bool isGrounded = true;

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
    /// Звук ходьбы
    /// </summary>
    public AudioSource walkingSound;

    void Start()
    {
        // Cursor.lockState = CursorLockMode.Locked; // Confined - курсор может находиться только в окне "игры", Lock - пропадает курсор и залочен в центре экрана
        camera = Camera.main; // главная камера
        if (photonView.IsMine)
        {
            playerModel.SetActive(false); // отключаем свою модельку для себя
        }
    }

    void Update()
    {
        // управление только своим персонажем
        if (photonView.IsMine == false)
            return;

        var movementInput = playerInput.actions["Move"].ReadValue<Vector2>();
        var lookInput = playerInput.actions["Look"].ReadValue<Vector2>();
        var jumpInput = playerInput.actions["Jump"].ReadValue<float>() > 0;

        mouseInput = lookInput * mouseSensitivity;
        
        // поворачиваем налево-направо "тело"
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
            transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

        if (viewPoint is null)
            throw new NullReferenceException("ViewPoint is null or empty");

        verticalRotationStore += mouseInput.y;

        // ограничение по зрению вверх вниз
        verticalRotationStore = Mathf.Clamp(verticalRotationStore, -60f, 60f);

        // поворачиваем вверх-низ "голову" модельки с камерой
        viewPoint.rotation = Quaternion.Euler(-verticalRotationStore, viewPoint.rotation.eulerAngles.y,
            viewPoint.rotation.eulerAngles.z);

        // получаем направление движения
        moveDirection = new Vector3(movementInput.x, 0f, movementInput.y);

        // TODO: бег
        // moveSpeed = Input.GetKeyDown(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        moveSpeed = walkSpeed;

        var yVelocity = movement.y;

        // давижение по направлению взгляда
        movement = ((transform.forward * moveDirection.z) + (transform.right * moveDirection.x)).normalized * moveSpeed;

        // гравитация 
        movement.y = yVelocity;

        if (characterController.isGrounded)
        {
            movement.y = 0f;
        }

        // проверка что персонаж стоит на поверхности
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers);

        var isJumping = jumpInput && isGrounded;

        if (isJumping)
        {
            movement.y = jumpForce;
        }
        
        // применение гравитации
        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;

        // движение
        characterController.Move(movement * Time.deltaTime);

        var isWalking = moveDirection.magnitude > 0;
        UpdateAnimator(isWalking, isJumping);
        WalkingSound(isWalking, isJumping);
    }

    /// <summary>
    /// Звук ходьбы
    /// </summary>
    /// <param name="isWalking"></param>
    /// <param name="isJumping"></param>
    private void WalkingSound(bool isWalking, bool isJumping)
    {
        if (isJumping)
        {
            if (walkingSound.isPlaying)
            {
                walkingSound.Stop();
            }
            return;
        }
        
        if (isWalking && walkingSound.isPlaying == false)
        {
            walkingSound.Play();
        }
        else if(isWalking == false && walkingSound.isPlaying)
        {
            walkingSound.Stop();
        }
    }

    private void LateUpdate()
    {
        if (photonView.IsMine == false)
            return;

        UpdateCameraPosition();
    }

    /// <summary>
    /// Обновление позиции камеры
    /// </summary>
    private void UpdateCameraPosition()
    {
        camera.transform.position = viewPoint.position;
        camera.transform.rotation = viewPoint.rotation;
    }

    /// <summary>
    /// Обновить анимацию
    /// </summary>
    private void UpdateAnimator(bool isWalking, bool isJumping)
    {
        animator.SetBool(PlayerAnimatorConstants.IsRunning, isWalking);
        animator.SetBool(PlayerAnimatorConstants.IsJumping, isJumping);
    }
}