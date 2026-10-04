using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform cameraPivot;

    [Header("Movement")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float jumpHeight = 1.2f;
    [SerializeField] float gravity = -20f;
    [SerializeField] float pushStrength = 4f;

    [Header("Look")]
    [SerializeField] float mouseSensitivity = 0.2f;
    [Tooltip("Sensitivity while aiming down sights")]
    [SerializeField] float aimSensitivity = 0.1f;
    [SerializeField] float maxPitch = 89f;

    [Header("Head Bob")]
    [Tooltip("Bob cycles per metre walked")]
    [SerializeField] float bobFrequency = 0.35f;
    [SerializeField] float bobVerticalAmount = 0.05f;
    [SerializeField] float bobSideAmount = 0.03f;
    [Range(0f, 1f)]
    [SerializeField] float aimBobMultiplier = 0.25f;
    [SerializeField] float bobSmoothing = 10f;

    public bool IsAiming { get; set; }

    CharacterController controller;
    float verticalVelocity;
    float pitch;
    Vector3 cameraRestPosition;
    float bobPhase;
    float bobWeight;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraPivot == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraPivot = cam.transform;
        }
        cameraRestPosition = cameraPivot.localPosition;
    }

    void OnEnable() => LockCursor(true);

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        
        if (keyboard.escapeKey.wasPressedThisFrame) LockCursor(false);
        else if (mouse.leftButton.wasPressedThisFrame) LockCursor(true);

        if (Cursor.lockState == CursorLockMode.Locked) Look(MouseLook(mouse.delta.ReadValue()));
        if (GameSettings.ArrowKeyTurning) Look(ArrowLook(keyboard), fromKeys: true);
        Move(keyboard);
        HeadBob();
    }

    
    void HeadBob()
    {
        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;
        float speed = horizontalVelocity.magnitude;
        bool walking = controller.isGrounded && speed > 0.1f;

        float targetWeight = walking ? Mathf.Clamp01(speed / walkSpeed) : 0f;
        if (IsAiming) targetWeight *= aimBobMultiplier;
        bobWeight = Mathf.Lerp(bobWeight, targetWeight, bobSmoothing * Time.deltaTime);

        if (walking) bobPhase += speed * bobFrequency * Mathf.PI * 2f * Time.deltaTime;

        Vector3 offset = new Vector3(
            Mathf.Sin(bobPhase) * bobSideAmount,
            -Mathf.Abs(Mathf.Cos(bobPhase)) * bobVerticalAmount * 2f + bobVerticalAmount,
            0f);
        cameraPivot.localPosition = cameraRestPosition + offset * bobWeight;
    }

    
    Vector2 MouseLook(Vector2 delta)
    {
        if (GameSettings.TrackpadMode) delta = new Vector2(TrackpadCurve(delta.x), TrackpadCurve(delta.y));
        float sensitivity = IsAiming ? aimSensitivity * GameSettings.AimSensitivity : mouseSensitivity * GameSettings.LookSensitivity;
        return delta * sensitivity;
    }

    
    
    static float TrackpadCurve(float v) => Mathf.Sign(v) * 1.87f * Mathf.Pow(Mathf.Abs(v), 0.7f);

    
    Vector2 ArrowLook(Keyboard keyboard)
    {
        Vector2 input = Vector2.zero;
        if (keyboard.rightArrowKey.isPressed) input.x += 1f;
        if (keyboard.leftArrowKey.isPressed) input.x -= 1f;
        if (keyboard.upArrowKey.isPressed) input.y += 1f;
        if (keyboard.downArrowKey.isPressed) input.y -= 1f;
        float speed = GameSettings.ArrowTurnSpeed * (IsAiming ? aimSensitivity / Mathf.Max(mouseSensitivity, 0.0001f) : 1f);
        return input * speed * Time.deltaTime;
    }

    
    void Look(Vector2 degrees, bool fromKeys = false)
    {
        if (GameSettings.InvertY && !fromKeys) degrees.y = -degrees.y;
        transform.Rotate(Vector3.up, degrees.x);
        pitch = Mathf.Clamp(pitch - degrees.y, -maxPitch, maxPitch);
        cameraPivot.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    void Move(Keyboard keyboard)
    {
        Vector2 input = Vector2.zero;
        if (keyboard.wKey.isPressed) input.y += 1f;
        if (keyboard.sKey.isPressed) input.y -= 1f;
        if (keyboard.dKey.isPressed) input.x += 1f;
        if (keyboard.aKey.isPressed) input.x -= 1f;
        input = Vector2.ClampMagnitude(input, 1f);

        float speed = walkSpeed;
        Vector3 move = (transform.right * input.x + transform.forward * input.y) * speed;

        if (controller.isGrounded)
        {
            
            if (verticalVelocity < 0f) verticalVelocity = -2f;
            if (keyboard.spaceKey.wasPressedThisFrame)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.TryGetComponent(out JumpPad pad) && hit.normal.y > 0.5f)
        {
            verticalVelocity = Mathf.Sqrt(pad.LaunchHeight * -2f * gravity);
            return;
        }

        Rigidbody body = hit.rigidbody;
        if (body == null || body.isKinematic || hit.moveDirection.y < -0.3f) return;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
        Vector3 pushVelocity = pushDir * pushStrength / Mathf.Max(body.mass, 0.1f);
        body.linearVelocity = new Vector3(pushVelocity.x, body.linearVelocity.y, pushVelocity.z);
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
