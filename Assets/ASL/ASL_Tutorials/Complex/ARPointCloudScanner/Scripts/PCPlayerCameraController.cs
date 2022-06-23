using UnityEngine;

/// <summary>Simple keyboard and mouse control for the PC player in the ASL_ARPointCloud tutorial</summary>
public class PCPlayerCameraController : MonoBehaviour
{
    /// <summary>Movement speed sensitivity for keyboard input</summary>
    public float MovementSpeed = 5f;

    /// <summary>Mouse speed sensitivity</summary>
    public float MouseSensitivity = 5f;

    /// <summary>Unity horizontal input</summary>
    private float horizontalInput;

    /// <summary>Unity vertical input</summary>
    private float verticalInput;

    /// <summary>Maximum mouse look angle</summary>
    private float maxYAngle = 80f;

    /// <summary>Current rotation state</summary>
    private Vector2 _currentRotation;

    /// <summary>Unity update method for input processing</summary>
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        if (horizontalInput != 0f || verticalInput != 0f)
        {
            Vector3 transVector = Vector3.forward * verticalInput * Time.deltaTime * MovementSpeed;            
            transVector += Vector3.right * horizontalInput * Time.deltaTime * MovementSpeed;

            transform.Translate(transVector);
        }

        if (Input.GetMouseButton(0))
        {           
            _currentRotation.x += Input.GetAxis("Mouse X") * MouseSensitivity;
            _currentRotation.y -= Input.GetAxis("Mouse Y") * MouseSensitivity;
            _currentRotation.x = Mathf.Repeat(_currentRotation.x, 360f);
            _currentRotation.y = Mathf.Clamp(_currentRotation.y, -maxYAngle, maxYAngle);
            transform.rotation = Quaternion.Euler(_currentRotation.y, _currentRotation.x, 0);
        }
    }
}
