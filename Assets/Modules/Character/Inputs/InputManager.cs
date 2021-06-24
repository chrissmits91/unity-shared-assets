using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;

    public static InputManager Instance {
        get {
            return _instance;
        }
    }

    private PlayerControls playerControls;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }

        playerControls = new PlayerControls();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable() {
        playerControls.Enable();
    }

    private void OnDisable() {
        playerControls.Disable();
    }

    public Vector2 GetPlayerMovement() {
        return playerControls.Player.Movement.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta() {
        return playerControls.Player.Look.ReadValue<Vector2>();
    }

    public bool PlayerJumpedThisFrame() {
        return playerControls.Player.Jump.triggered;
    }

    public bool PlayerFiredThisFrame() {
        return playerControls.Player.Fire.triggered;
    }

    public UnityEngine.InputSystem.InputAction PlayerAim() {
        return playerControls.Player.Aim;
    }

    public bool PlayerReloadedThisFrame() {
        return playerControls.Player.Reload.triggered;
    }
}
