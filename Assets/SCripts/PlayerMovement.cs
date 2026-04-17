using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles Purly's movement and Y-axis rotation using two distinct Input System
/// reading methods:
/// - Walking: Vector2 Value action via PlayerInput (WASD / arrow keys).
/// - Rotation: 1D axis read directly from the Input System Keyboard device (Q / E).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    // Units per second Purly moves.
    public float moveSpeed = 5f;

    [Header("Rotation")]
    // Degrees per second the visual rotates around the Y axis.
    public float rotationSpeed = 200f;

    [Tooltip("Assign the child visual Transform to rotate. If null, the root is rotated.")]
    // Optional child object to rotate separately from the Rigidbody root.
    public Transform purlyVisual;

    // Cached Rigidbody2D used for movement in FixedUpdate.
    private Rigidbody2D rb;

    // Latest movement input read from the Input System.
    private Vector2 moveInput;

    void Awake()
    {
        // Cache the Rigidbody2D reference once instead of looking it up repeatedly.
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Receives the Move action value (Vector2 Value action) from PlayerInput.
    /// WASD and arrow keys are bound in the Input Actions asset.
    /// </summary>
    public void OnMove(InputValue value)
    {
        // Read the Vector2 movement action from the PlayerInput component.
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        // Read keyboard rotation each frame because it is device-based, not action-callback based.
        float rotateInput = 0f;
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            // Q rotates one way and E rotates the other.
            if (keyboard.qKey.isPressed)
            {
                rotateInput = -1f;
            }
            else if (keyboard.eKey.isPressed)
            {
                rotateInput = 1f;
            }
        }

        // Skip rotation work if neither key is being pressed.
        if (Mathf.Abs(rotateInput) <= 0f)
        {
            return;
        }

        // Convert input into frame-based rotation.
        float delta = rotateInput * rotationSpeed * Time.deltaTime;

        // Rotate the child visual if assigned, otherwise rotate the root object.
        Transform target = purlyVisual != null ? purlyVisual : transform;
        target.Rotate(0f, delta, 0f);
    }

    void FixedUpdate()
    {
        // Normalize input so diagonal movement is not faster than straight movement.
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
}
