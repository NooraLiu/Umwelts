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
    public ParticleSystem[] dogParticles; // Multiple Particle Systems for Dog Mode

    public GameObject hiddenObjects;
    public GameObject personObject;
    public GameObject dogObject;

    [Header("Skybox Materials")]
    public Material personSkybox;
    public Material dogSkybox;
    public Material birdSkybox;

    [Header("Lighting Settings")]
    public Light directionalLight; // Assign the Directional Light in the Inspector

    [Header("Mode-Specific Spawn Points")]
    public Transform dogSpawnPoint; // Assign in Inspector

    [Header("Bird Area Control")]
    public Collider slowZoneCollider;
    public float birdSpeedOutsideZone = 6f;

    [Header("Spherical Gravity Settings")]
    public float planetWalkSpeed = 2f;   // Walking speed while on the planet surface
    public float planetGravity = 9.81f;  // Gravity pulling toward the planet's center


    [Header("UI Hints")]
    public GameObject landHintText;

    private bool isOnPlanet = false;
    private Transform currentPlanet;
    private bool canLand = false;
    public float landingCheckRadius = 10f; // Radius to check for nearby planets


    public bool ending = false;
    private Rigidbody rb;
    private bool usingRigidbody = false;


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

    private bool rotatingUpright = false;
    private Quaternion targetUprightRotation;
    private float uprightRotateSpeed = 3f; // Adjust to control how fast it rotates



    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (!controller) Debug.LogError("Missing CharacterController!");

        defaultFOV = playerCamera.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        SetMode(Mode.Person);

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void Update()
    {
        HandleMouseLook();
        HandleModeSwitching();
        //CheckInteractionZones();
        if (currentMode == Mode.Bird && isHovering)
        {
            CheckLandingZone();

            if (canLand && Input.GetKeyDown(KeyCode.R))
            {
                BeginPlanetLanding();
            }
        }
        else
        {
            if (landHintText != null) landHintText.SetActive(false);
        }

        switch (currentMode)
        {
            case Mode.Person: HandlePersonMovement(); break;
            case Mode.Dog: HandleDogMovement(); break;
            case Mode.Bird: HandleBirdMovement(); break;
        }
        if (rotatingUpright)
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetUprightRotation, Time.deltaTime * uprightRotateSpeed);

        // Stop rotating when close enough
        if (Quaternion.Angle(transform.rotation, targetUprightRotation) < 0.5f)
        {
            transform.rotation = targetUprightRotation;
            rotatingUpright = false;
        }
    }

    }

    void FixedUpdate()
{
    if (!usingRigidbody || currentPlanet == null) return;

    Vector3 gravityDir = (currentPlanet.position - transform.position).normalized;
    //rb.AddForce(gravityDir * planetGravity, ForceMode.Acceleration);

    Quaternion targetRot = Quaternion.FromToRotation(transform.up, -gravityDir) * transform.rotation;
    rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 5f));

    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");

    Vector3 camForward = Vector3.ProjectOnPlane(playerCamera.transform.forward, -gravityDir).normalized;
    Vector3 camRight = Vector3.Cross(-gravityDir, camForward).normalized;
    Vector3 move = (camForward * v + camRight * h).normalized;

    rb.MovePosition(rb.position + move * planetWalkSpeed * Time.fixedDeltaTime);
}

    #region Mode Configuration
   public void SetMode(Mode mode)
    {
        _currentMode = mode;
        currentMode = mode;
        isHovering = false;

        UmweltsManager.Instance?.ApplyEffect((UmweltsManager.EffectMode)mode);
        
        //Skybox and lighting
        if (mode == Mode.Person)
        {
            RenderSettings.ambientIntensity = 0.7f;
            RenderSettings.skybox = personSkybox;
            RenderSettings.reflectionIntensity = 0.4f;
        }
        else if (mode == Mode.Dog)
        {
            RenderSettings.ambientIntensity = 2f; // Normal brightness for Person mode
            RenderSettings.skybox = dogSkybox;
            directionalLight.transform.rotation = Quaternion.Euler(0f, 3f, 0f);

            // Move player to dog mode position
            if (dogSpawnPoint != null)
            {
                transform.position = dogSpawnPoint.position;
                transform.rotation = dogSpawnPoint.rotation; // Adjust rotation if needed
            }
            else
            {
                Debug.LogWarning("Dog Spawn Point not set in Inspector!");
            }
            if (interactionHintText != null) interactionHintText.SetActive(false);
        }
        else if (mode == Mode.Bird)
        {
            RenderSettings.ambientIntensity = 1.2f;
            RenderSettings.skybox = birdSkybox;
            directionalLight.transform.rotation = Quaternion.Euler(88f, -98f, 0f);
            RenderSettings.reflectionIntensity = 1f;
        }

        //Mode specific Models
        if (dogModel != null)
        {
            dogModel.SetActive(mode == Mode.Dog);
        }

        if (dogParticles != null)
        {
            foreach (var particle in dogParticles)
            {
                if (particle != null)
                {
                    particle.gameObject.SetActive(mode == Mode.Dog);
                }
            }
        }

        if (hiddenObjects != null)
        {
            hiddenObjects.SetActive(mode != Mode.Person || ending);
        }

        if (personObject != null)
        {
            personObject.SetActive(mode == Mode.Person);
        }

        if (dogObject != null)
        {
            dogObject.SetActive(mode == Mode.Dog);
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(dogJumpForce * 2f * gravity);
        }

        ApplyGravity();
    }

    // void HandleBirdMovement()
    // {
    //     HandleTakeoffAndLanding();
    //     HandleAltitudeControl();

    //     if (isHovering)
    //     {
    //        float currentSpeed = IsInsideSlowZone() ? avianSettings.flySpeed : birdSpeedOutsideZone;
    //         var horizontalMove = GetMovementVector() * currentSpeed * Time.deltaTime;
    //         var verticalMove = Vector3.up * verticalSpeed * Time.deltaTime;
    //         controller.Move(horizontalMove + verticalMove);
    //     }
    //     else
    //     {
    //         MoveCharacter(avianSettings.groundSpeed);
    //         ApplyGravity();
    //     }
    // }
    void HandleBirdMovement()
{
    if (isOnPlanet)
    {

        if (isOnPlanet && Input.GetKeyDown(KeyCode.Space))
        {
            isOnPlanet = false;
            isHovering = true;
            verticalSpeed = 0f;

            rb.isKinematic = true;            // Stop using physics
            usingRigidbody = false;
            controller.enabled = true;        // Resume CharacterController movement
            targetUprightRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            rotatingUpright = true;
        }

    return;
    }

    HandleTakeoffAndLanding();
    HandleAltitudeControl();

    if (isHovering)
    {
        float currentSpeed = IsInsideSlowZone() ? avianSettings.flySpeed : birdSpeedOutsideZone;
        var horizontalMove = GetMovementVector() * currentSpeed * Time.deltaTime;
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
    if (!isHovering) return;

    // Handle ascending
    if (isAscending)
    {
        if (transform.position.y >= targetHoverY)
        {
            transform.position = new Vector3(transform.position.x, targetHoverY, transform.position.z);
            isAscending = false;
            verticalSpeed = 0f;
        }
    }

    // Interrupt fall and start flying up when outside slow zone and player presses SPACE
    if (isDescending && Input.GetKeyDown(KeyCode.Space) && !IsInsideSlowZone())
    {
        isDescending = false;
        StartAscending();
    }

    // Descent logic
    if (isDescending)
    {
        verticalSpeed = -avianSettings.descentSpeed;
    }
    else if (!isAscending)
    {
        verticalSpeed = 0f; // Maintain altitude
    }

    // Landing check
    if (verticalSpeed < 0 && controller.isGrounded)
    {
        CompleteLanding();
    }
}

    // void StartAscending()
    // {
    //     if (isHovering) return;
    //     isHovering = true;
    //     isAscending = true;
    //     targetHoverY = transform.position.y + avianSettings.hoverHeight;
    //     verticalSpeed = avianSettings.ascentSpeed;
    // }
    void StartAscending()
{
    if (!isHovering)
    {
        // First time takeoff
        isHovering = true;
    }

    // Additional ascension while hovering
    RaycastHit hit;
    float maxRise = avianSettings.hoverHeight;

    if (Physics.Raycast(transform.position, Vector3.up, out hit, maxRise))
    {
        maxRise = hit.distance - 0.1f;
        if (maxRise <= 0f) return; // Too close to ceiling
    }

    targetHoverY = transform.position.y + maxRise;
    isAscending = true;
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

    void CheckLandingZone()
{
    canLand = false;
    if (landHintText != null) landHintText.SetActive(false);

    Collider[] hits = Physics.OverlapSphere(transform.position, landingCheckRadius);
    foreach (var hit in hits)
    {
        if (hit.CompareTag("Planet"))
        {
            canLand = true;
            currentPlanet = hit.transform;
            if (landHintText != null) landHintText.SetActive(true);
            return;
        }
    }
}

void BeginPlanetLanding()
{
    if (currentPlanet == null) return;

    isHovering = false;
    isOnPlanet = true;
    verticalSpeed = 0f;

    controller.enabled = false;
    rb.isKinematic = false;
    usingRigidbody = true;

    if (landHintText != null) landHintText.SetActive(false);
    PlanetInteraction planetInteraction = currentPlanet.GetComponent<PlanetInteraction>();
if (planetInteraction != null)
{
    planetInteraction.TriggerLandingNarrative();
}

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
    bool IsInsideSlowZone()
    {
        return slowZoneCollider != null && slowZoneCollider.bounds.Contains(transform.position);
    }

    #endregion

    #region Physics
    void ApplyGravity()
{
    if (isOnPlanet || usingRigidbody) return;
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