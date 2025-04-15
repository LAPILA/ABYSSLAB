//------------------------------------------------------------------------------
// CHANGE LOG
// version 1.1.0
// "Integrated IA_Player (New Input System) with custom editor improvements."
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonController : MonoBehaviour
{
    #region ==== Fields & References ====

    [Header("Input System")]
    [Tooltip("Attach the PlayerInput component that uses IA_Player input actions.")]
    public PlayerInput playerInput;
    // We'll subscribe to events in OnEnable()

    [Header("References")]
    public Camera playerCamera;
    private Rigidbody rb;

    [Header("Camera Settings")]
    [Tooltip("Initial Field of View.")]
    public float fov = 60f;
    public bool invertCamera = false;
    [Tooltip("If false, camera won't rotate by mouse input.")]
    public bool cameraCanMove = true;
    [Range(0.1f, 10f)]
    public float mouseSensitivity = 1f;
    [Range(20f, 90f)]
    public float maxLookAngle = 50f;
    [Tooltip("Lock cursor on start?")]
    public bool lockCursor = true;
    public bool smoothCameraRotation = false;
    [Range(0f, 20f)] public float cameraRotationSmoothTime = 10f;

    // Crosshair
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;
    private Image crosshairObject;

    // Internals
    private float yawRaw, pitchRaw;
    private float yawSmooth, pitchSmooth;

    #region Zoom
    [Header("Zoom Settings")]
    public bool enableZoom = true;
    [Tooltip("True = hold to zoom, false = toggle")]
    public bool holdToZoom = false;
    [Range(1f, 179f)]
    public float zoomFOV = 30f;
    [Range(0.1f, 10f)]
    public float zoomStepTime = 5f;
    private bool isZoomed;
    private bool isZoomHeld; // for hold logic
    #endregion

    #region Movement
    [Header("Movement Settings")]
    public bool playerCanMove = true;
    [Range(0.1f, 20f)]
    public float walkSpeed = 5f;
    [Tooltip("Max velocity change in x/z per frame.")]
    public float maxVelocityChange = 10f;
    private bool isWalking;
    private bool isGrounded;

    // Move input from IA_Player
    private Vector2 moveInput;
    private Vector2 lookInput;
    #endregion

    #region Sprint
    [Header("Sprint Settings")]
    public bool enableSprint = true;
    [Tooltip("If true, unlimited sprint time.")]
    public bool unlimitedSprint = false;
    [Range(0.1f, 30f)]
    public float sprintSpeed = 7f;
    [Range(1f, 30f)]
    public float sprintDuration = 5f;
    [Range(0.1f, 5f)]
    public float sprintCooldown = 0.5f;
    [Range(60f, 179f)]
    public float sprintFOV = 80f;
    [Range(0.1f, 20f)]
    public float sprintFOVStepTime = 10f;

    [Tooltip("Hold = true, Toggle = false")]
    public bool holdToSprint = true;
    private bool isSprinting;
    private float sprintRemaining;
    private bool sprintOnCooldown;
    private float sprintCooldownTimer;

    // SprintBar UI
    public bool useSprintBar = true;
    [Tooltip("If sprint is full, hide bar?")]
    public bool hideBarWhenFull = true;
    public Image sprintBarBG;
    public Image sprintBar;
    [Range(0.1f, 0.5f)]
    public float sprintBarWidthPercent = .3f;
    [Range(0.001f, 0.05f)]
    public float sprintBarHeightPercent = .015f;

    private CanvasGroup sprintBarCG;
    private bool isSprintHeld; // for hold logic
    #endregion

    #region Jump
    [Header("Jump Settings")]
    public bool enableJump = true;
    [Tooltip("true = hold jump, false = single press (toggle not typical, but included).")]
    public bool holdToJump = false;
    [Range(1f, 30f)]
    public float jumpPower = 5f;
    private bool jumpPressed;
    #endregion

    #region Crouch
    [Header("Crouch Settings")]
    public bool enableCrouch = true;
    [Tooltip("Hold = true, Toggle = false")]
    public bool holdToCrouch = true;
    [Range(0.1f, 1f)]
    public float crouchHeight = .75f;
    [Range(0.1f, 1f)]
    public float speedReduction = .5f;
    [Range(1f, 20f)] 
    public float crouchTransitionSpeed = 8f;
    private bool isCrouched;
    private bool isCrouchHeld;
    private Vector3 originalScale;
    private float targetScaleY;
    #endregion

    #region HeadBob
    [Header("Head Bob Settings")]
    public bool enableHeadBob = true;
    public Transform joint;
    [Range(1f, 20f)]
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);
    private float bobTimer;
    private Vector3 jointOriginalPos;
    #endregion

    // For the auto-generated input class
    private IA_Player inputActions;

    #endregion

    #region === Unity Callbacks ===

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (playerCamera) playerCamera.fieldOfView = fov;

        originalScale = transform.localScale;
        targetScaleY = originalScale.y;
        if (joint) jointOriginalPos = joint.localPosition;

        // Sprint init
        sprintRemaining = sprintDuration;
        sprintCooldownTimer = sprintCooldown;

        // Crosshair
        crosshairObject = GetComponentInChildren<Image>();
    }

    private void Start()
    {
        // Lock cursor
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Setup crosshair
        if (crosshair && crosshairObject)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else if (crosshairObject)
        {
            crosshairObject.gameObject.SetActive(false);
        }

        // SprintBar
        sprintBarCG = GetComponentInChildren<CanvasGroup>();
        if (useSprintBar && sprintBarBG && sprintBar && sprintBarCG)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float sw = Screen.width * sprintBarWidthPercent;
            float sh = Screen.height * sprintBarHeightPercent;

            sprintBarBG.rectTransform.sizeDelta = new Vector2(sw, sh);
            sprintBar.rectTransform.sizeDelta = new Vector2(sw - 2, sh - 2);

            if (hideBarWhenFull)
            {
                sprintBarCG.alpha = 0f;
            }
        }
        else
        {
            if (sprintBarBG) sprintBarBG.gameObject.SetActive(false);
            if (sprintBar) sprintBar.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (!playerInput)
        {
            Debug.LogWarning("PlayerInput not assigned on " + name);
            return;
        }
        // Create inputActions instance
        inputActions = new IA_Player();
        inputActions.Player.Enable();

        // Subscribe: Move, Look
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        // Sprint
        inputActions.Player.Sprint.performed += OnSprintPerformed;
        inputActions.Player.Sprint.canceled += OnSprintCanceled;

        // Crouch
        inputActions.Player.Crouch.performed += OnCrouchPerformed;
        inputActions.Player.Crouch.canceled += OnCrouchCanceled;

        // Jump
        inputActions.Player.Jump.performed += OnJumpPerformed;
        inputActions.Player.Jump.canceled += OnJumpCanceled;

        // Zoom
        inputActions.Player.Zoom.performed += OnZoomPerformed;
        inputActions.Player.Zoom.canceled += OnZoomCanceled;
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();

        // Unsubscribe
        if (inputActions != null)
        {
            inputActions.Player.Move.performed -= null;
            inputActions.Player.Move.canceled -= null;
            inputActions.Player.Look.performed -= null;
            inputActions.Player.Look.canceled -= null;
            inputActions.Player.Sprint.performed -= OnSprintPerformed;
            inputActions.Player.Sprint.canceled -= OnSprintCanceled;
            inputActions.Player.Crouch.performed -= OnCrouchPerformed;
            inputActions.Player.Crouch.canceled -= OnCrouchCanceled;
            inputActions.Player.Jump.performed -= OnJumpPerformed;
            inputActions.Player.Jump.canceled -= OnJumpCanceled;
            inputActions.Player.Zoom.performed -= OnZoomPerformed;
            inputActions.Player.Zoom.canceled -= OnZoomCanceled;
        }
    }

    private void Update()
    {
        if (!playerCanMove) return;

        // Camera look
        if (cameraCanMove && playerCamera)
            HandleCameraLook();

        // Zoom
        if (enableZoom)
            HandleZoom();

        // Sprint
        if (enableSprint)
            HandleSprint();

        // Jump
        if (enableJump)
            HandleJump();

        // Crouch
        if (enableCrouch)
            SmoothCrouchTransition();
            HandleCrouch();

        // Ground check
        CheckGround();

        // Headbob
        if (enableHeadBob && joint)
            HandleHeadBob();
    }

    private void FixedUpdate()
    {
        // Movement
        if (playerCanMove)
            HandleMovement();
    }

    #endregion

    #region === Input Callbacks ===

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
        {
            isSprintHeld = false;
        }
    }

    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        if (holdToCrouch)
        {
            isCrouchHeld = true;
        }
        else
        {
            // toggle
            if (!isCrouched) CrouchDown();
            else StandUp();
        }
    }
    private void OnCrouchCanceled(InputAction.CallbackContext ctx)
    {
        if (holdToCrouch)
        {
            isCrouchHeld = false;
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (!holdToJump)
        {
            jumpPressed = true;
        }
        else
        {
            jumpPressed = true;
        }
    }
    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        if (holdToJump)
        {
            jumpPressed = false;
        }
    }

    private void OnZoomPerformed(InputAction.CallbackContext ctx)
    {
        if (holdToZoom)
        {
            isZoomHeld = true;
        }
        else
        {
            isZoomed = !isZoomed;
        }
    }
    private void OnZoomCanceled(InputAction.CallbackContext ctx)
    {
        if (holdToZoom)
        {
            isZoomHeld = false;
        }
    }

    #endregion

    #region === Camera Look ===

    private void HandleCameraLook()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        yawRaw += mouseX;
        if (!invertCamera) pitchRaw -= mouseY;
        else pitchRaw += mouseY;

        pitchRaw = Mathf.Clamp(pitchRaw, -maxLookAngle, maxLookAngle);

        if (smoothCameraRotation)
        {
            yawSmooth = Mathf.Lerp(yawSmooth, yawRaw, Time.deltaTime * cameraRotationSmoothTime);
            pitchSmooth = Mathf.Lerp(pitchSmooth, pitchRaw, Time.deltaTime * cameraRotationSmoothTime);
        }
        else
        {
            yawSmooth = yawRaw;
            pitchSmooth = pitchRaw;
        }

        // 최종 적용
        transform.localEulerAngles = new Vector3(0f, yawSmooth, 0f);
        playerCamera.transform.localEulerAngles = new Vector3(pitchSmooth, 0f, 0f);
    }

    #endregion

    #region === Movement ===

    private void HandleMovement()
    {
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y);

        if (isGrounded && targetVelocity.sqrMagnitude > 0.01f)
            isWalking = true;
        else
            isWalking = false;

        float currentSpeed = (isSprinting) ? sprintSpeed : walkSpeed;
        if (isCrouched) currentSpeed *= speedReduction;

        targetVelocity = transform.TransformDirection(targetVelocity) * currentSpeed;

        Vector3 velocity = rb.velocity;
        Vector3 velocityChange = targetVelocity - velocity;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    #endregion

    #region === Sprint ===

    private void HandleSprint()
    {
        if (isSprinting)
        {
            // Drain stamina
            if (!unlimitedSprint)
            {
                sprintRemaining -= Time.deltaTime;
                if (sprintRemaining <= 0f)
                {
                    isSprinting = false;
                    sprintOnCooldown = true;
                }
            }
            // FOV
            if (playerCamera)
            {
                float target = sprintFOV;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, target, sprintFOVStepTime * Time.deltaTime);
            }
        }
        else
        {
            // recover
            if (!unlimitedSprint && sprintRemaining < sprintDuration)
            {
                sprintRemaining += Time.deltaTime;
                if (sprintRemaining > sprintDuration) sprintRemaining = sprintDuration;
            }
            // revert FOV if not zoom
            if (playerCamera && !isZoomed)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, sprintFOVStepTime * Time.deltaTime);
            }
        }

        // cooldown
        if (sprintOnCooldown)
        {
            sprintCooldownTimer -= Time.deltaTime;
            if (sprintCooldownTimer <= 0f)
            {
                sprintOnCooldown = false;
                sprintCooldownTimer = sprintCooldown;
            }
        }

        // if hold
        if (holdToSprint)
        {
            if (isSprintHeld && !sprintOnCooldown && sprintRemaining > 0f && !isCrouched)
                isSprinting = true;
            else if (!isSprintHeld)
                isSprinting = false;
        }

        // SprintBar
        if (useSprintBar && sprintBar && !unlimitedSprint)
        {
            float pct = sprintRemaining / sprintDuration;
            sprintBar.transform.localScale = new Vector3(pct, 1f, 1f);

            if (hideBarWhenFull && sprintBarCG)
            {
                if (Mathf.Approximately(pct, 1f))
                {
                    sprintBarCG.alpha = Mathf.Lerp(sprintBarCG.alpha, 0f, 3f * Time.deltaTime);
                }
                else
                {
                    sprintBarCG.alpha = Mathf.Lerp(sprintBarCG.alpha, 1f, 5f * Time.deltaTime);
                }
            }
        }
    }

    #endregion

    #region === Jump ===

    private void HandleJump()
    {
        if (jumpPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            isGrounded = false;

            // if crouched in toggle mode, stand up
            if (isCrouched && !holdToCrouch)
            {
                StandUp();
            }

            if (!holdToJump)
                jumpPressed = false;
        }
    }

    #endregion

    #region === Crouch ===

    private void HandleCrouch()
    {
        if (holdToCrouch)
        {
            if (isCrouchHeld && !isCrouched)
            {
                CrouchDown();
            }
            else if (!isCrouchHeld && isCrouched)
            {
                StandUp();
            }
        }
        // else toggle is handled in OnCrouchPerformed
    }

    private void CrouchDown()
    {
        targetScaleY = crouchHeight;
        isCrouched = true;
        isSprinting = false;
    }

    private void StandUp()
    {
        targetScaleY = originalScale.y;
        isCrouched = false;
    }
    private void SmoothCrouchTransition()
    {
        Vector3 currentScale = transform.localScale;
        float newY = Mathf.Lerp(currentScale.y, targetScaleY, Time.deltaTime * crouchTransitionSpeed);

        transform.localScale = new Vector3(currentScale.x, newY, currentScale.z);
    }
    #endregion

    #region === Zoom ===

    private void HandleZoom()
    {
        if (isSprinting)
        {
            isZoomed = false;
        }
        else
        {
            // if hold
            if (holdToZoom)
            {
                isZoomed = isZoomHeld;
            }
        }
        float targetFOV = (isZoomed) ? zoomFOV : fov;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, zoomStepTime * Time.deltaTime);
    }

    #endregion

    #region === Ground Check ===

    private void CheckGround()
    {
        float distance = 0.75f;
        Vector3 origin = transform.position + Vector3.down * (transform.localScale.y * 0.5f);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    #endregion

    #region === HeadBob ===

    private void HandleHeadBob()
    {
        if (isWalking)
        {
            float currentBobSpeed = bobSpeed;
            if (isSprinting) currentBobSpeed += sprintSpeed;
            if (isCrouched) currentBobSpeed *= speedReduction;

            bobTimer += Time.deltaTime * currentBobSpeed;
            Vector3 offset = new Vector3(
                Mathf.Sin(bobTimer) * bobAmount.x,
                Mathf.Sin(bobTimer) * bobAmount.y,
                Mathf.Sin(bobTimer) * bobAmount.z
            );
            joint.localPosition = jointOriginalPos + offset;
        }
        else
        {
            bobTimer = 0f;
            joint.localPosition = Vector3.Lerp(
                joint.localPosition,
                jointOriginalPos,
                Time.deltaTime * bobSpeed
            );
        }
    }

    #endregion
}


// ====================== CUSTOM EDITOR ======================
#if UNITY_EDITOR
[CustomEditor(typeof(FirstPersonController)), InitializeOnLoadAttribute]
public class FirstPersonControllerEditor : Editor
{
    FirstPersonController fpc;
    SerializedObject SerFPC;

    private void OnEnable()
    {
        fpc = (FirstPersonController)target;
        SerFPC = new SerializedObject(fpc);
    }

    public override void OnInspectorGUI()
    {
        SerFPC.Update();

        EditorGUILayout.Space();
        GUILayout.Label("Modular First Person Controller (New Input System)",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 16 });
        GUILayout.Label("version 1.1.0",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
        EditorGUILayout.Space();

        // 1) Input System Setup
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Input System Setup",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.playerInput = (PlayerInput)EditorGUILayout.ObjectField(
            new GUIContent("Player Input", "Attach the PlayerInput with IA_Player Actions"),
            fpc.playerInput, typeof(PlayerInput), true
        );

        EditorGUILayout.Space();

        #region Camera Setup

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Camera Setup",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.playerCamera = (Camera)EditorGUILayout.ObjectField(
            new GUIContent("Camera", "Camera attached to the controller."),
            fpc.playerCamera, typeof(Camera), true
        );

        fpc.fov = EditorGUILayout.Slider(new GUIContent("Field of View",
            "The camera’s view angle. Changes the player camera directly."),
            fpc.fov, 1f, 179f
        );
        fpc.cameraCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Camera Rotation",
            "Determines if the camera is allowed to move."), fpc.cameraCanMove);

        fpc.smoothCameraRotation = EditorGUILayout.ToggleLeft("Smooth Camera Rotation",
        fpc.smoothCameraRotation
        );

        if (fpc.smoothCameraRotation)
        {
            EditorGUI.indentLevel++;
            fpc.cameraRotationSmoothTime = EditorGUILayout.Slider(
                "Rotation Smooth Time",
                fpc.cameraRotationSmoothTime,
                1f, 10f
            );
            EditorGUI.indentLevel--;
        }

        if (fpc.cameraCanMove)
        {
            EditorGUI.indentLevel++;
            fpc.invertCamera = EditorGUILayout.ToggleLeft(
                new GUIContent("Invert Camera Rotation", "Inverts the up/down camera movement."),
                fpc.invertCamera
            );
            fpc.mouseSensitivity = EditorGUILayout.Slider(
                new GUIContent("Look Sensitivity"), fpc.mouseSensitivity, 0.1f, 1f
            );
            fpc.maxLookAngle = EditorGUILayout.Slider(
                new GUIContent("Max Look Angle"), fpc.maxLookAngle, 20f, 90f
            );
            EditorGUI.indentLevel--;
        }

        fpc.lockCursor = EditorGUILayout.ToggleLeft(
            new GUIContent("Lock and Hide Cursor", "Hides the cursor and locks it to center."),
            fpc.lockCursor
        );

        fpc.crosshair = EditorGUILayout.ToggleLeft(
            new GUIContent("Enable Crosshair"), fpc.crosshair
        );
        if (fpc.crosshair)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Crosshair Image");
            fpc.crosshairImage = (Sprite)EditorGUILayout.ObjectField(fpc.crosshairImage, typeof(Sprite), false);
            EditorGUILayout.EndHorizontal();

            fpc.crosshairColor = EditorGUILayout.ColorField("Crosshair Color", fpc.crosshairColor);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Zoom
        GUILayout.Label("Zoom Settings", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 });
        fpc.enableZoom = EditorGUILayout.ToggleLeft("Enable Zoom", fpc.enableZoom);

        if (fpc.enableZoom)
        {
            EditorGUI.indentLevel++;
            fpc.holdToZoom = EditorGUILayout.ToggleLeft("Hold To Zoom", fpc.holdToZoom);
            fpc.zoomFOV = EditorGUILayout.Slider("Zoom FOV", fpc.zoomFOV, 1f, fpc.fov);
            fpc.zoomStepTime = EditorGUILayout.Slider("Zoom Step Time", fpc.zoomStepTime, 0.1f, 10f);
            EditorGUI.indentLevel--;
        }

        #endregion

        #region Movement Setup

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Movement Setup",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.playerCanMove = EditorGUILayout.ToggleLeft("Enable Player Movement", fpc.playerCanMove);
        if (fpc.playerCanMove)
        {
            EditorGUI.indentLevel++;
            fpc.walkSpeed = EditorGUILayout.Slider("Walk Speed", fpc.walkSpeed, 0.1f, 20f);
            fpc.maxVelocityChange = EditorGUILayout.FloatField("Max Velocity Change", fpc.maxVelocityChange);
            EditorGUI.indentLevel--;
        }

        // Sprint
        EditorGUILayout.Space();
        GUILayout.Label("Sprint",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        fpc.enableSprint = EditorGUILayout.ToggleLeft("Enable Sprint", fpc.enableSprint);

        if (fpc.enableSprint)
        {
            EditorGUI.indentLevel++;
            fpc.unlimitedSprint = EditorGUILayout.ToggleLeft("Unlimited Sprint", fpc.unlimitedSprint);
            fpc.holdToSprint = EditorGUILayout.ToggleLeft("Hold To Sprint", fpc.holdToSprint);

            fpc.sprintSpeed = EditorGUILayout.Slider("Sprint Speed", fpc.sprintSpeed, fpc.walkSpeed, 30f);
            fpc.sprintDuration = EditorGUILayout.Slider("Sprint Duration", fpc.sprintDuration, 1f, 30f);
            fpc.sprintCooldown = EditorGUILayout.Slider("Sprint Cooldown", fpc.sprintCooldown, 0.1f, 5f);
            fpc.sprintFOV = EditorGUILayout.Slider("Sprint FOV", fpc.sprintFOV, fpc.fov, 179f);
            fpc.sprintFOVStepTime = EditorGUILayout.Slider("Sprint FOV Step", fpc.sprintFOVStepTime, 0.1f, 20f);

            fpc.useSprintBar = EditorGUILayout.ToggleLeft("Use Sprint Bar", fpc.useSprintBar);
            if (fpc.useSprintBar)
            {
                EditorGUI.indentLevel++;
                fpc.hideBarWhenFull = EditorGUILayout.ToggleLeft("Hide Full Bar", fpc.hideBarWhenFull);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Bar BG");
                fpc.sprintBarBG = (Image)EditorGUILayout.ObjectField(fpc.sprintBarBG, typeof(Image), true);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Bar Foreground");
                fpc.sprintBar = (Image)EditorGUILayout.ObjectField(fpc.sprintBar, typeof(Image), true);
                EditorGUILayout.EndHorizontal();

                fpc.sprintBarWidthPercent = EditorGUILayout.Slider("Bar Width %", fpc.sprintBarWidthPercent, 0.1f, 0.5f);
                fpc.sprintBarHeightPercent = EditorGUILayout.Slider("Bar Height %", fpc.sprintBarHeightPercent, 0.001f, 0.05f);
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
        }

        // Jump
        EditorGUILayout.Space();
        GUILayout.Label("Jump",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        fpc.enableJump = EditorGUILayout.ToggleLeft("Enable Jump", fpc.enableJump);

        if (fpc.enableJump)
        {
            EditorGUI.indentLevel++;
            fpc.holdToJump = EditorGUILayout.ToggleLeft("Hold To Jump", fpc.holdToJump);
            fpc.jumpPower = EditorGUILayout.Slider("Jump Power", fpc.jumpPower, 1f, 30f);
            EditorGUI.indentLevel--;
        }

        // Crouch
        EditorGUILayout.Space();
        GUILayout.Label("Crouch",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        fpc.enableCrouch = EditorGUILayout.ToggleLeft("Enable Crouch", fpc.enableCrouch);

        if (fpc.enableCrouch)
        {
            EditorGUI.indentLevel++;
            fpc.holdToCrouch = EditorGUILayout.ToggleLeft("Hold To Crouch", fpc.holdToCrouch);
            fpc.crouchHeight = EditorGUILayout.Slider("Crouch Height", fpc.crouchHeight, 0.1f, 1f);
            fpc.speedReduction = EditorGUILayout.Slider("Speed Reduction", fpc.speedReduction, 0.1f, 1f);
            EditorGUI.indentLevel--;
        }

        fpc.crouchTransitionSpeed = EditorGUILayout.Slider("Crouch Transition Speed",
        fpc.crouchTransitionSpeed,1f,20f);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Head Bob Setup",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.enableHeadBob = EditorGUILayout.ToggleLeft("Enable Head Bob", fpc.enableHeadBob);
        if (fpc.enableHeadBob)
        {
            EditorGUI.indentLevel++;
            fpc.joint = (Transform)EditorGUILayout.ObjectField("Camera Joint", fpc.joint, typeof(Transform), true);
            fpc.bobSpeed = EditorGUILayout.Slider("Bob Speed", fpc.bobSpeed, 1f, 20f);
            fpc.bobAmount = EditorGUILayout.Vector3Field("Bob Amount", fpc.bobAmount);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(fpc);
            Undo.RecordObject(fpc, "FPC Change");
            SerFPC.ApplyModifiedProperties();
        }
        #endregion
    }
}
#endif