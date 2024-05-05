using System;
using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        // Cursor.lockState = CursorLockMode.Locked; // Confined - курсор может находиться только в окне "игры", Lock - пропадает курсор и залочен в центре экрана
        camera = Camera.main; // главная камера
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

        // получаем значения мышки
        // mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;

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

        // TODO: перенести на контролы
        // получаем направление движения
        // moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
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

        // animator.SetBool("is_jumping", isJumping);

        // применение гравитации
        movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;

        // движение
        characterController.Move(movement * Time.deltaTime);

        UpdateAnimator(moveDirection.magnitude > 0, isJumping);
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
    private void UpdateAnimator(bool isRunning, bool isJumping)
    {
        animator.SetBool("is_running", isRunning);
        animator.SetBool("is_jumping", isJumping);
    }
}