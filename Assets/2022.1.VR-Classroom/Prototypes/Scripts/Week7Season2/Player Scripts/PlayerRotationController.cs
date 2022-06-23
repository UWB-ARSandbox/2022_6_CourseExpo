using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

public class PlayerRotationController : MonoBehaviour
{
    private PlayerInputActions playerInputActions;

    private PlayerController _playerControllerRef;

    private InputAction rotation;
    private InputAction headRotation;
    private bool isXRActive => XRSettings.isDeviceActive;

    private bool isCursorLocked => Cursor.lockState == CursorLockMode.Locked;
    
    private float _curUpDownAngle = 0f;

    private Vector2 rotationInput = Vector2.zero;

    //private Quaternion _lastRotation = Quaternion.identity;
    private float _lastRotation = 0f;


    void OnEnable()
    {
        //rotation
        rotation = playerInputActions.Player.Rotation;
        rotation.performed += Rotation;
        rotation.Enable();

        //rotation
        headRotation = playerInputActions.Player.HeadRotation;
        headRotation.performed += HeadRotation;
        //headRotation.Enable();
    }

    void OnDisable()
    {
        rotation.Disable();
        headRotation.Disable();
    }

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        _playerControllerRef = gameObject.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isCursorLocked)
        {
            //Rotation
            rotationInput *= Time.deltaTime * 8f;
            if (isXRActive)
            {
                rotationInput.x *= 4f;
            }
            _curUpDownAngle -= rotationInput.y;
            _curUpDownAngle = Mathf.Clamp(_curUpDownAngle, -90f, 90f);
            Camera.main.transform.localRotation = Quaternion.Euler(_curUpDownAngle, 0f, 0f);
            transform.Rotate(Vector3.up, rotationInput.x);
        }
    }

    private void HeadRotation(InputAction.CallbackContext obj)
    {
        var thisRotation = obj.ReadValue<Quaternion>().eulerAngles.y;
        if (thisRotation != _lastRotation)
        {
            var delta = thisRotation - _lastRotation;
            rotationInput.x += delta;
            _lastRotation = thisRotation;
        }
    }

    private void Rotation(InputAction.CallbackContext obj)
    {
        if (!(Mouse.current.rightButton.isPressed && _playerControllerRef.HasObject))
            rotationInput += obj.ReadValue<Vector2>();
    }
}
