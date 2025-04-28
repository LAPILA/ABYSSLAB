//------------------------------------------------------------------------------
// CHANGE LOG
// version 1.0.0
// "IA_Player-based 2.5D/2D Controller (8-direction movement, shift sprint, zoom) - NO sprint bar."
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;   // For PlayerInput, InputAction
using Spine;
using Spine.Unity;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))]
public class TD2DPlayerController : MonoBehaviour
{
    #region === Fields & References ===

    [Header("Spine / Animation")]
    public SkeletonAnimation skelAnim;

    [SerializeField] private string idleAnim = "idle";
    [SerializeField] private string walkAnim = "walk";
    [SerializeField] private string walkStartAnim = "walkstart";
    [SerializeField] private float startBlendTime = 0.1f;
    private Spine.AnimationState animState;
    private bool wasMoving;

    [Header("Input System")]
    [Tooltip("Attach the PlayerInput component that uses IA_Player input actions.")]
    public PlayerInput playerInput;

    private IA_Player _inputActions;  // Generated class from IA_Player.inputactions

    [Header("Camera Settings")]
    public Camera playerCamera;
    [Range(1f, 179f)] public float normalFOV = 60f;
    public bool enableZoom = true;
    public bool holdToZoom = false;
    [Range(1f, 179f)] public float zoomFOV = 30f;
    [Range(0.1f, 10f)] public float zoomStepTime = 5f;

    // Internal
    private bool isZoomed;
    private bool isZoomHeld;

    [Header("Movement Settings")]
    public bool playerCanMove = true;
    [Range(0.1f, 20f)] public float walkSpeed = 5f;
    [Range(0.1f, 30f)] public float sprintSpeed = 8f;
    [Tooltip("Max velocity change in x/z per frame.")]
    public float maxVelocityChange = 10f;

    // Sprint
    public bool enableSprint = true;
    [Tooltip("If true, shift를 누르는 동안만, false=Toggle")]
    public bool holdToSprint = true;
    [Tooltip("If true, sprint는 무제한.")]
    public bool unlimitedSprint = false;
    [Range(1f, 30f)]
    public float sprintDuration = 5f;
    [Range(0.1f, 5f)]
    public float sprintCooldown = 0.5f;

    private bool isSprinting;
    private bool isSprintHeld;
    private float sprintRemaining;
    private bool sprintOnCooldown;
    private float sprintCooldownTimer;

    // Move input
    private Vector2 moveInput;
    private float fixedY;

    // Components
    private Rigidbody rb;

    #endregion

    #region === Unity Callbacks ===

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

        if (playerCamera)
            playerCamera.fieldOfView = normalFOV;

        sprintRemaining = sprintDuration;
        sprintCooldownTimer = sprintCooldown;
        animState = skelAnim.AnimationState;
        animState.SetAnimation(0, idleAnim, true);
    }

    private void OnEnable()
    {
        if (!playerInput)
        {
            Debug.LogWarning("PlayerInput not assigned in inspector!");
            return;
        }

        // 생성된 IA_Player 클래스 인스턴스화 & 활성화
        _inputActions = new IA_Player();
        _inputActions.Player.Enable();

        // 구독 (Move, Sprint, Zoom)
        _inputActions.Player.Move.performed += OnMovePerformed;
        _inputActions.Player.Move.canceled += OnMoveCanceled;

        _inputActions.Player.Sprint.performed += OnSprintPerformed;
        _inputActions.Player.Sprint.canceled += OnSprintCanceled;

        _inputActions.Player.Zoom.performed += OnZoomPerformed;
        _inputActions.Player.Zoom.canceled += OnZoomCanceled;
    }

    private void OnDisable()
    {
        _inputActions?.Player.Disable();

        if (_inputActions != null)
        {
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

        // Zoom
        if (enableZoom)
            HandleZoom();

        // Sprint
        if (enableSprint)
            HandleSprint();

        HandleAnimation();
    }

    private void FixedUpdate()
    {
        if (!playerCanMove) return;

        HandleMovement();
    }

    #endregion

    #region === Input Callbacks ===

    // Move
    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }
    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    // Sprint
    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        if (holdToSprint)
        {
            isSprintHeld = true;
        }
        else
        {
            // toggle
            isSprinting = !isSprinting;
        }
    }
    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        if (holdToSprint)
            isSprintHeld = false;
    }

    // Zoom
    private void OnZoomPerformed(InputAction.CallbackContext ctx)
    {
        if (holdToZoom)
        {
            isZoomHeld = true;
        }
        else
        {
            // toggle
            isZoomed = !isZoomed;
        }
    }
    private void OnZoomCanceled(InputAction.CallbackContext ctx)
    {
        if (holdToZoom)
            isZoomHeld = false;
    }

    #endregion

    #region === Movement ===

    private void HandleMovement()
    {
        Vector3 inputVec = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 targetVelocity = inputVec * currentSpeed;

        Vector3 velocity = rb.velocity;
        Vector3 velocityChange = targetVelocity - velocity;
        velocityChange.y = 0f;

        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

    }


    #endregion

    #region === Sprint ===

    private void HandleSprint()
    {
        // hold
        if (holdToSprint)
        {
            if (isSprintHeld && !isSprinting)
                isSprinting = true;
            else if (!isSprintHeld && isSprinting)
                isSprinting = false;
        }

        // Unlimited or not
        if (isSprinting)
        {
            if (!unlimitedSprint)
            {
                sprintRemaining -= Time.deltaTime;
                if (sprintRemaining <= 0f)
                {
                    // 스프린트 끔 + 쿨다운
                    isSprinting = false;
                    sprintOnCooldown = true;
                }
            }
        }
        else
        {
            // 회복
            if (!unlimitedSprint && sprintRemaining < sprintDuration)
            {
                sprintRemaining += Time.deltaTime;
                if (sprintRemaining > sprintDuration)
                    sprintRemaining = sprintDuration;
            }
        }

        // 쿨다운
        if (sprintOnCooldown)
        {
            sprintCooldownTimer -= Time.deltaTime;
            if (sprintCooldownTimer <= 0f)
            {
                sprintOnCooldown = false;
                sprintCooldownTimer = sprintCooldown;
            }
        }
    }

    #endregion

    #region === Zoom ===

    private void HandleZoom()
    {
        // hold
        if (holdToZoom)
        {
            isZoomed = isZoomHeld;
        }

        // FOV 보간
        if (playerCamera)
        {
            float targetFOV = (isZoomed) ? zoomFOV : normalFOV;
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                Time.deltaTime * zoomStepTime
            );
        }
    }

    #endregion

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

}


//--------------------- CUSTOM EDITOR ---------------------
#if UNITY_EDITOR
[CustomEditor(typeof(TD2DPlayerController)), InitializeOnLoadAttribute]
public class TD2DPlayerControllerEditor : Editor
{
    private TD2DPlayerController controller;
    private SerializedObject serializedObj;

    private void OnEnable()
    {
        controller = (TD2DPlayerController)target;
        serializedObj = new SerializedObject(controller);
    }

    public override void OnInspectorGUI()
    {
        serializedObj.Update();

        EditorGUILayout.Space();
        GUILayout.Label("2D/2.5D Player Controller (IA_Player)",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 14 });
        GUILayout.Label("version 1.0.0",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic, fontSize = 10 });
        EditorGUILayout.Space();

        // 1) Input System
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Input System",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObj.FindProperty("playerInput"),
            new GUIContent("Player Input", "Attach the PlayerInput with IA_Player"));

        // 2) Camera
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Camera Settings",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObj.FindProperty("playerCamera"),
            new GUIContent("Camera"));
        EditorGUILayout.PropertyField(serializedObj.FindProperty("normalFOV"),
            new GUIContent("Normal FOV"));
        EditorGUILayout.PropertyField(serializedObj.FindProperty("enableZoom"),
            new GUIContent("Enable Zoom"));
        if (controller.enableZoom)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObj.FindProperty("holdToZoom"),
                new GUIContent("Hold To Zoom"));
            EditorGUILayout.PropertyField(serializedObj.FindProperty("zoomFOV"),
                new GUIContent("Zoom FOV"));
            EditorGUILayout.PropertyField(serializedObj.FindProperty("zoomStepTime"),
                new GUIContent("Zoom Step Time"));
            EditorGUI.indentLevel--;
        }

        // 3) Movement
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Movement Settings",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObj.FindProperty("playerCanMove"),
            new GUIContent("Player Can Move"));
        if (controller.playerCanMove)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObj.FindProperty("walkSpeed"),
                new GUIContent("Walk Speed"));
            EditorGUILayout.PropertyField(serializedObj.FindProperty("sprintSpeed"),
                new GUIContent("Sprint Speed"));
            EditorGUILayout.PropertyField(serializedObj.FindProperty("maxVelocityChange"),
                new GUIContent("Max Velocity Change"));
            EditorGUI.indentLevel--;
        }

        // 4) Sprint
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Sprint Settings",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold });
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObj.FindProperty("enableSprint"),
            new GUIContent("Enable Sprint"));
        if (controller.enableSprint)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObj.FindProperty("holdToSprint"),
                new GUIContent("Hold To Sprint"));
            EditorGUILayout.PropertyField(serializedObj.FindProperty("unlimitedSprint"),
                new GUIContent("Unlimited Sprint"));
            EditorGUILayout.PropertyField(serializedObj.FindProperty("sprintDuration"),
                new GUIContent("Sprint Duration"));
            EditorGUILayout.PropertyField(serializedObj.FindProperty("sprintCooldown"),
                new GUIContent("Sprint Cooldown"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Spine / Animation",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObj.FindProperty("skelAnim"),
            new GUIContent("SkeletonAnimation", "Spine SkeletonAnimation 컴포넌트"));

        // SkeletonAnimation이 지정돼 있을 때만 세부 클립 노출
        if (controller.skelAnim != null)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObj.FindProperty("idleAnim"),
                new GUIContent("Idle Clip"));
            EditorGUILayout.PropertyField(serializedObj.FindProperty("walkAnim"),
                new GUIContent("Walk Clip"));
            EditorGUILayout.PropertyField(serializedObj.FindProperty("walkStartAnim"),
                new GUIContent("Walk-Start Clip"));
            EditorGUILayout.PropertyField(serializedObj.FindProperty("startBlendTime"),
                new GUIContent("Start→Walk Blend Time"));
            EditorGUI.indentLevel--;
        }

        // Save changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(controller);
            Undo.RecordObject(controller, "TD2DPlayerController changed");
            serializedObj.ApplyModifiedProperties();
        }
    }
}
#endif
