using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 4.0f;
    public Vector3 minBounds = new Vector3(-10, 0, -10);
    public Vector3 maxBounds = new Vector3(10, 10, 10);

    void Update()
    {
        // Horizontal and vertical movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Up and down movement
        float moveY = 0.0f;
        if (Input.GetKey(KeyCode.Z))
        {
            moveY = 1.0f;
        }
        else if (Input.GetKey(KeyCode.X))
        {
            moveY = -1.0f;
        }

        Vector3 move = transform.right * moveX + transform.forward * moveZ + transform.up * moveY;
        transform.position += move * moveSpeed * Time.deltaTime;

        // Clamp the camera position within the bounds
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y),
            Mathf.Clamp(transform.position.z, minBounds.z, maxBounds.z)
        );

        // Rotation
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            Vector3 rotation = transform.localEulerAngles;
            rotation.y += mouseX;
            rotation.x -= mouseY;
            transform.localEulerAngles = rotation;
        }
    }
}
