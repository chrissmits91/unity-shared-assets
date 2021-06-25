using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [SerializeField] private float clampAngle = 80f;
    [SerializeField] private float horizontalSpeed = 10f;
    [SerializeField] private float verticalSpeed = 10f;
    private InputManager inputManager;
    private Vector3 startingRotation;

    private void Start() {
        inputManager = InputManager.Instance;
        startingRotation = transform.localRotation.eulerAngles;
    }

    private void Update() {
        Vector2 deltaInput = inputManager.GetMouseDelta();
        startingRotation.x += deltaInput.x * verticalSpeed * Time.deltaTime;
        startingRotation.y += deltaInput.y * horizontalSpeed * Time.deltaTime;
        startingRotation.y = Mathf.Clamp(startingRotation.y, -clampAngle, clampAngle);
        transform.localRotation = Quaternion.Euler(-startingRotation.y, startingRotation.x, 0f);
    }
}
