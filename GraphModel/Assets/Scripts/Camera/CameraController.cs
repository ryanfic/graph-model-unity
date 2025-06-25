using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float verticalSpeed = 5f;
    public float lookSensitivity = 1f;

    private Vector2 moveInput;
    private float upDownInput;
    private Vector2 lookInput;

    private float yaw;
    private float pitch;

    private CameraControls controls;

    private void Awake()
    {
        controls = new CameraControls();

        controls.Camera.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Camera.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Camera.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Camera.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Camera.UpDown.performed += ctx => upDownInput = ctx.ReadValue<float>();
        controls.Camera.UpDown.canceled += ctx => upDownInput = 0f;

        pitch = transform.rotation.eulerAngles.x;
        yaw = transform.rotation.eulerAngles.y;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        // Rotate camera
        yaw += lookInput.x * lookSensitivity;
        pitch -= lookInput.y * lookSensitivity;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);

        // Move camera
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 up = Vector3.up;

        Vector3 move = forward * moveInput.y + right * moveInput.x + up * upDownInput;

        int speedUp = 1 + (Input.GetKey(KeyCode.LeftShift) ? 1 : 0);
        transform.position += moveSpeed * speedUp * Time.deltaTime * move;

        if (Input.mouseScrollDelta != Vector2.zero)
        {
            moveSpeed += Input.mouseScrollDelta.y * Mathf.Pow(moveSpeed, 0.5f);
        }
    }
}
