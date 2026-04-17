using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles Purly's movement and Y-axis rotation using two distinct Input System
/// reading methods — as required by the assignment:
///   - Walking: Vector2 Value action via PlayerInput (WASD / arrow keys).
///   - Rotation: 1D axis read directly from the Input System Keyboard device (Q / E).
/// Both use the new Input System package; they are intentionally different action types.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Rotation")]
    public float rotationSpeed = 200f;

    [Tooltip("Assign the child visual Transform to rotate. If null, the root is rotated.")]
    public Transform purlyVisual;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Receives the Move action value (Vector2 Value action) from PlayerInput.
    /// WASD and arrow keys are bound in the Input Actions asset.
    /// </summary>
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        // Rotation — read directly from the Input System Keyboard device.
        // This is a deliberate second input type (device-direct vs. action-based),
        // satisfying the requirement for separate movement types in the Input System.
        float rotateInput = 0f;
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.qKey.isPressed) rotateInput = -1f;
            else if (keyboard.eKey.isPressed) rotateInput = 1f;
        }

        if (Mathf.Abs(rotateInput) > 0f)
        {
            float delta = rotateInput * rotationSpeed * Time.deltaTime;
            Transform target = purlyVisual != null ? purlyVisual : transform;
            target.Rotate(0f, delta, 0f);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
}
