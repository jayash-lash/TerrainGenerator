using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private float _movingYSpeed = 5;
    [SerializeField] private float _movingSpeed = 5;
    [SerializeField] private float _rotationSpeed = 5;

    private float _rotationX = 0f;
    private float _rotationY = 0f;

    private void Update()
    {
        RotateCamera();
        Walk();
        HandleInput();
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        _rotationX -= mouseY * _rotationSpeed;
        _rotationX = Mathf.Clamp(_rotationX, -90f, 90f);

        _rotationY += mouseX * _rotationSpeed;

        transform.localRotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void HandleInput()
    {
        if (Input.GetKey(KeyCode.Space))
            Fly(Vector3.up);
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            Fly(Vector3.down);
    }

    private void Walk()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0.0f, verticalInput);

        movementDirection.Normalize();
        transform.Translate(movementDirection * _movingSpeed * Time.deltaTime, Space.Self);
    }

    private void Fly(Vector3 direction)
    {
        transform.Translate(direction * _movingYSpeed * Time.deltaTime);
    }

    private Vector3 GetMovementDirection(Vector3 movementInput)
    {
        return transform.right * movementInput.x + transform.forward * movementInput.z;
    }

    public void Move(Vector3 movementInput)
    {
        Vector3 movementDirection = GetMovementDirection(movementInput);
        movementDirection.y = 0;
        transform.Translate(movementDirection * _movingSpeed * Time.deltaTime, Space.World);
    }
}
