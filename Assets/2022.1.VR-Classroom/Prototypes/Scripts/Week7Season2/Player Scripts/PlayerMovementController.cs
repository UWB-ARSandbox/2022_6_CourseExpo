using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    private InputAction movement;
    private InputAction jump;
    private InputAction toggleSprint;
    private InputAction toggleWalk;

    //Movement + Jump
    private CharacterController controller;
    public float speed = 8f;
    public float gravity = -9.81f * 5;
    public float jumpHeight = 1.8f;

    [Range(1.0f, 10.0f)]
    public float SprintSpeed = 1.7f;

    //Gravity
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    Vector3 velocity;

    private bool IsGrounded => Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    private static bool IsCursorLocked => Cursor.lockState == CursorLockMode.Locked;

    private bool IsSprinting { get; set; } = false;

    // Start is called before the first frame update
    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        controller = gameObject.GetComponent<CharacterController>();
        Debug.Assert(controller != null);
    }

    private void OnEnable()
    {
        //position
        movement = playerInputActions.Player.Movement;
        movement.Enable();

        //position
        jump = playerInputActions.Player.Jump;
        jump.performed += DoJump;
        jump.Enable();

        //interaction
        toggleSprint = playerInputActions.Player.Sprint;
        toggleSprint.performed += Sprint;
        toggleSprint.Enable();

        toggleWalk = playerInputActions.Player.Walk;
        toggleWalk.performed += Walk;
        toggleWalk.Enable();

    }

    private void Walk(InputAction.CallbackContext obj)
    {
        IsSprinting = false;
    }

    private void Sprint(InputAction.CallbackContext obj)
    {
        IsSprinting = true;
    }

    private void DoJump(InputAction.CallbackContext obj)
    {
        if (IsGrounded && IsCursorLocked && !PlayerController.IsTypingInput)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (IsGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }

        //Horizontal Movement
        if (IsCursorLocked && !PlayerController.IsTypingInput)
        {
            var runFactor = IsSprinting ? SprintSpeed : 1.0f;
            Vector2 moveValue = movement.ReadValue<Vector2>();
            Vector3 move = transform.right * moveValue.x + transform.forward * moveValue.y;
            controller.Move(move * speed * runFactor * Time.deltaTime);
        }

        //Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnDisable()
    {
        movement.Disable();
        jump.Disable();
    }
}
