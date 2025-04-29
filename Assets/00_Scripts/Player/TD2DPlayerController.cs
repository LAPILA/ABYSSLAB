//==============================================================================
// TD2DPlayerController (Odin Inspector + 디버그 버튼 버전)
//==============================================================================

using UnityEngine;
using UnityEngine.InputSystem;
using Spine.Unity;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Rigidbody))]
public class TD2DPlayerController : MonoBehaviour
{
    [Title("Spine / Animation")]
    [BoxGroup("Spine")]
    [Required] public SkeletonAnimation skelAnim;

    [BoxGroup("Spine")]
    [LabelText("Idle Animation")] public string idleAnim = "idle";

    [BoxGroup("Spine")]
    [LabelText("Walk Animation")] public string walkAnim = "walk";

    [BoxGroup("Spine")]
    [LabelText("Walk Start Animation")] public string walkStartAnim = "walkstart";

    [BoxGroup("Spine")]
    [Range(0f, 1f)][LabelText("Start → Walk Blend Time")] public float startBlendTime = 0.1f;

    private Spine.AnimationState animState;
    private bool wasMoving;

    [Title("Input System")]
    [BoxGroup("Input")]
    [Required] public PlayerInput playerInput;
    private IA_Player _inputActions;

    [Title("Camera Settings")]
    [BoxGroup("Camera")]
    [Required] public Camera playerCamera;

    [BoxGroup("Camera")]
    [Range(1f, 179f)] public float normalFOV = 60f;

    [BoxGroup("Camera")]
    public bool enableZoom = true;

    [BoxGroup("Camera")]
    public bool holdToZoom = false;

    [BoxGroup("Camera")]
    [Range(1f, 179f)] public float zoomFOV = 30f;

    [BoxGroup("Camera")]
    [Range(0.1f, 10f)] public float zoomStepTime = 5f;

    private bool isZoomed;
    private bool isZoomHeld;

    [Title("Movement Settings")]
    [BoxGroup("Movement")]
    public bool playerCanMove = true;

    [BoxGroup("Movement")]
    [Range(0.1f, 20f)] public float walkSpeed = 5f;

    [BoxGroup("Movement")]
    [Range(0.1f, 30f)] public float sprintSpeed = 8f;

    [BoxGroup("Movement")]
    [Tooltip("Max velocity change per frame.")]
    public float maxVelocityChange = 10f;

    [Title("Sprint Settings")]
    [BoxGroup("Sprint")]
    public bool enableSprint = true;

    [BoxGroup("Sprint")]
    public bool holdToSprint = true;

    private bool isSprinting;
    private bool isSprintHeld;
    private Vector3 _spawnPosition;

    private Vector2 moveInput;
    private Rigidbody rb;

    // ===== 디버그용 버튼 =====
    [Title("Debug Actions")]
    [BoxGroup("Debug Actions")]
    [Button("Reset to Spawn Position")]
    private void ResetPlayerPosition()
    {
        transform.position = _spawnPosition;
        rb.velocity = Vector3.zero;
        Debug.Log("[DEBUG] Player Reset to Spawn Position");
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        _spawnPosition = transform.position;

        if (playerCamera != null)
            playerCamera.fieldOfView = normalFOV;

        animState = skelAnim.AnimationState;
        animState.SetAnimation(0, idleAnim, true);
    }

    private void OnEnable()
    {
        if (playerInput == null)
        {
            Debug.LogWarning("PlayerInput not assigned.");
            return;
        }

        _inputActions = new IA_Player();
        _inputActions.Player.Enable();

        _inputActions.Player.Move.performed += OnMovePerformed;
        _inputActions.Player.Move.canceled += OnMoveCanceled;

        _inputActions.Player.Sprint.performed += OnSprintPerformed;
        _inputActions.Player.Sprint.canceled += OnSprintCanceled;

        _inputActions.Player.Zoom.performed += OnZoomPerformed;
        _inputActions.Player.Zoom.canceled += OnZoomCanceled;
    }

    private void OnDisable()
    {
        if (_inputActions != null)
        {
            _inputActions.Player.Disable();

            _inputActions.Player.Move.performed -= OnMovePerformed;
            _inputActions.Player.Move.canceled -= OnMoveCanceled;
            _inputActions.Player.Sprint.performed -= OnSprintPerformed;
            _inputActions.Player.Sprint.canceled -= OnSprintCanceled;
            _inputActions.Player.Zoom.performed -= OnZoomPerformed;
            _inputActions.Player.Zoom.canceled -= OnZoomCanceled;
        }
    }

    private void Update()
    {
        if (!playerCanMove) return;

        if (enableZoom) HandleZoom();
        if (enableSprint) HandleSprint();
        HandleAnimation();
    }

    private void FixedUpdate()
    {
        if (!playerCanMove) return;
        HandleMovement();
    }

    #region Input Handling

    private void OnMovePerformed(InputAction.CallbackContext ctx) =>
        moveInput = ctx.ReadValue<Vector2>();

    private void OnMoveCanceled(InputAction.CallbackContext ctx) =>
        moveInput = Vector2.zero;

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        if (holdToSprint) isSprintHeld = true;
        else isSprinting = !isSprinting;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        if (holdToSprint) isSprintHeld = false;
    }

    private void OnZoomPerformed(InputAction.CallbackContext ctx)
    {
        if (holdToZoom) isZoomHeld = true;
        else isZoomed = !isZoomed;
    }

    private void OnZoomCanceled(InputAction.CallbackContext ctx)
    {
        if (holdToZoom) isZoomHeld = false;
    }

    #endregion

    #region Core Logic

    private void HandleMovement()
    {
        Vector3 inputVec = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        float speed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 targetVelocity = inputVec * speed;

        Vector3 velocity = rb.velocity;
        Vector3 velocityChange = targetVelocity - velocity;
        velocityChange.y = 0f;

        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void HandleSprint()
    {
        if (!enableSprint) return;

        if (holdToSprint)
        {
            isSprinting = isSprintHeld;
        }
    }

    private void HandleZoom()
    {
        if (holdToZoom) isZoomed = isZoomHeld;

        if (playerCamera != null)
        {
            float targetFOV = isZoomed ? zoomFOV : normalFOV;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomStepTime);
        }
    }

    private void HandleAnimation()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        if (isMoving)
            skelAnim.Skeleton.ScaleX = (moveInput.x < 0) ? 1f : -1f;

        if (isMoving && !wasMoving)
        {
            animState.SetAnimation(0, walkStartAnim, false);
            animState.AddAnimation(0, walkAnim, true, startBlendTime);
        }
        else if (!isMoving && wasMoving)
        {
            animState.SetAnimation(0, idleAnim, true);
        }

        animState.TimeScale = isSprinting ? 1.4f : 1f;
        wasMoving = isMoving;
    }

    #endregion
}
