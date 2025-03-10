using UnityEngine;

public class UmweltCameraController : MonoBehaviour
{
    public enum Mode { Person, Dog, Bird }

    [System.Serializable]
    public class MovementSettings
    {
        public float height = 1f;
        public float walkSpeed = 3f;
        public float runSpeed = 5f;
        public float radius = 0.3f;
    }

    [System.Serializable]
    public class BirdSettings
    {
        public float groundSpeed = 2f;
        public float flySpeed = 4f;
        public float hoverHeight = 0.5f;
        public float ascentSpeed = 3f;
        public float descentSpeed = 3f;
        public float fovMultiplier = 1.5f;
    }

    // Mode Settings
    [Header("Mode Settings")]
    public MovementSettings personSettings;
    public MovementSettings dogSettings;
    public MovementSettings birdSettings;
    public BirdSettings avianSettings;

    [Header("Common Settings")]
    public float interactionRadius = 2f;
    public float mouseSensitivity = 2f;
    public float jumpForce = 2.5f;
    public float dogJumpForce = 4f;
    public float gravity = 9.81f;

    [Header("References")]
    public Camera playerCamera;
    public Camera[] stackedCameras;
    public Transform[] interactionZones;
    public Transform[] dogJumpRegions;
    public GameObject interactionHintText;
    public GameObject jumpHintText;

    [Header("Mode-Specific Models")]
    public GameObject dogModel; // Assign in Inspector
    public ParticleSystem dogParticles;

    // State variables
    private Mode currentMode = Mode.Person;
    private CharacterController controller;
    private float defaultFOV;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isHovering;

    // Bird-specific state
    private float targetHoverY;
    private float verticalSpeed;
    private bool isAscending;
    private bool isDescending;

    private bool canInteract;
    private bool canDogJump;

    private Mode _currentMode;
    public Mode CurrentMode => _currentMode;
    // Add these to UmweltCameraController
    public bool IsGrounded => controller.isGrounded;
    public bool IsSprinting => currentMode == Mode.Person && Input.GetKey(KeyCode.LeftShift);
    public Vector3 Velocity => controller.velocity;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (!controller) Debug.LogError("Missing CharacterController!");

        defaultFOV = playerCamera.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        SetMode(Mode.Person);
    }

    void Update()
    {
        HandleMouseLook();
        HandleModeSwitching();
        //CheckInteractionZones();

        switch (currentMode)
        {
            case Mode.Person: HandlePersonMovement(); break;
            case Mode.Dog: HandleDogMovement(); break;
            case Mode.Bird: HandleBirdMovement(); break;
        }
    }

    #region Mode Configuration
    void SetMode(Mode mode)
    {
        _currentMode = mode;
        currentMode = mode;
        isHovering = false;

        UmweltsManager.Instance?.ApplyEffect((UmweltsManager.EffectMode)mode);

        if (dogModel != null)
        {
            dogModel.SetActive(mode == Mode.Dog);
        }

        if (dogParticles != null)
        {
            dogParticles.gameObject.SetActive(mode == Mode.Dog);
        }

        switch (mode)
        {
            case Mode.Person:
                ConfigureController(personSettings);
                ResetFOV();
                break;

            case Mode.Dog:
                ConfigureController(dogSettings);
                ResetFOV();
                break;

            case Mode.Bird:
                ConfigureController(birdSettings);
                AdjustFOV(avianSettings.fovMultiplier);
                break;
        }

        Debug.Log($"Switched to {mode}");
    }

    void ConfigureController(MovementSettings settings)
    {
        controller.height = settings.height;
        controller.radius = settings.radius;
        controller.center = new Vector3(0, settings.height / 2f, 0);
        playerCamera.transform.localPosition = new Vector3(0, settings.height, 0);
    }
    #endregion

    #region Camera Controls
    void HandleMouseLook()
    {
        var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        playerCamera.transform.Rotate(Vector3.left * mouseY);
    }

    void AdjustFOV(float multiplier)
    {
        playerCamera.fieldOfView = defaultFOV * multiplier;
        foreach (var cam in stackedCameras) cam.fieldOfView = defaultFOV * multiplier;
    }

    void ResetFOV()
    {
        playerCamera.fieldOfView = defaultFOV;
        foreach (var cam in stackedCameras) cam.fieldOfView = defaultFOV;
    }
    #endregion

    #region Movement Systems
    void HandlePersonMovement()
    {
        isGrounded = controller.isGrounded;
        var speed = Input.GetKey(KeyCode.LeftShift) ? personSettings.runSpeed : personSettings.walkSpeed;
        MoveCharacter(speed);

        CheckInteractionZones();

        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && canInteract)
        {
            Debug.Log("Interacting...");
        }

        ApplyGravity();
    }

    void HandleDogMovement()
    {
        isGrounded = controller.isGrounded;
        MoveCharacter(dogSettings.walkSpeed);

        CheckDogJumpRegions();
        if (canDogJump && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(dogJumpForce * 2f * gravity);
        }

        ApplyGravity();
    }

    void HandleBirdMovement()
    {
        HandleTakeoffAndLanding();
        HandleAltitudeControl();

        if (isHovering)
        {
            var horizontalMove = GetMovementVector() * avianSettings.flySpeed * Time.deltaTime;
            var verticalMove = Vector3.up * verticalSpeed * Time.deltaTime;
            controller.Move(horizontalMove + verticalMove);
        }
        else
        {
            MoveCharacter(avianSettings.groundSpeed);
            ApplyGravity();
        }
    }

    Vector3 GetMovementVector()
    {
        return transform.forward * Input.GetAxis("Vertical") +
               transform.right * Input.GetAxis("Horizontal");
    }

    void MoveCharacter(float speed)
    {
        controller.Move(GetMovementVector() * speed * Time.deltaTime);
    }
    #endregion

    #region Bird Flight System
    void HandleTakeoffAndLanding()
    {
        if (Input.GetKeyDown(KeyCode.Space)) StartAscending();
        if (Input.GetKeyDown(KeyCode.S)) StartDescending();
    }

    void HandleAltitudeControl()
    {
        if (isAscending && transform.position.y >= targetHoverY)
        {
            isAscending = false;
            verticalSpeed = 0f;
        }

        if (isDescending && controller.isGrounded)
        {
            CompleteLanding();
        }
    }

    void StartAscending()
    {
        if (isHovering) return;
        isHovering = true;
        isAscending = true;
        targetHoverY = transform.position.y + avianSettings.hoverHeight;
        verticalSpeed = avianSettings.ascentSpeed;
    }

    void StartDescending()
    {
        isDescending = true;
        verticalSpeed = -avianSettings.descentSpeed;
    }

    void CompleteLanding()
    {
        isHovering = false;
        isDescending = false;
        verticalSpeed = 0f;
        velocity.y = 0f;
    }
    #endregion

    #region Environment Interactions
    void CheckInteractionZones()
    {
        canInteract = CheckProximity(interactionZones);
        interactionHintText?.SetActive(canInteract);
    }

    void CheckDogJumpRegions()
    {
        canDogJump = CheckProximity(dogJumpRegions);
        jumpHintText?.SetActive(canDogJump);
    }

    bool CheckProximity(Transform[] zones)
    {
        if (zones == null) return false;

        foreach (var zone in zones)
        {
            if (Vector3.Distance(transform.position, zone.position) < interactionRadius)
                return true;
        }
        return false;
    }
    #endregion

    #region Physics
    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0) velocity.y = -0.1f;
        velocity.y -= gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    #endregion

    #region Input Handling
    void HandleModeSwitching()
    {
        if (Input.GetKeyDown(KeyCode.P)) SetMode(Mode.Person);
        if (Input.GetKeyDown(KeyCode.G)) SetMode(Mode.Dog);
        if (Input.GetKeyDown(KeyCode.B)) SetMode(Mode.Bird);
    }
    #endregion
}