using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSController : MonoBehaviour
{
    // references
    CharacterController controller;
    [SerializeField] GameObject cam;
    [SerializeField] Transform gunHold;
    [SerializeField] Gun initialGun;

    // movement stats
    [SerializeField] float movementSpeed = 2.0f;
    [SerializeField] float lookSensitivityX = 1.0f;
    [SerializeField] float lookSensitivityY = 1.0f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpForce = 10;

    // runtime
    Vector3 velocity;
    float xRotation;

    // input state
    Vector2 moveInput;
    Vector2 lookInput;
    Vector2 scrollInput;

    bool altFirePressed;
    bool sprintHeld;
    bool jumpPressed;

    bool fireHeld;
    bool firePressed;

    // guns
    List<Gun> equippedGuns = new List<Gun>();
    int gunIndex = 0;
    Gun currentGun = null;

    Vector3 origin;

    public GameObject Cam => cam;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (initialGun != null)
            AddGun(initialGun);

        origin = transform.position;
    }

    void Update()
    {
        Movement();
        Look();
        HandleSwitchGun();
        FireGun();

        // smooth velocity decay (knockback etc.)
        Vector3 noVelocity = new Vector3(0, velocity.y, 0);
        velocity = Vector3.Lerp(velocity, noVelocity, 5 * Time.deltaTime);

        // reset one-frame inputs
        jumpPressed = false;
        altFirePressed = false;
        firePressed = false;

        Debug.Log($"fireHeld = {fireHeld}, firePressed = {firePressed}");
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
            jumpPressed = true;
    }

    public void OnFire(InputValue value)
    {
        Debug.Log("FIRE INPUT RECEIVED");

        if (currentGun != null)
        {
            currentGun.AttemptFire();
        }
    }

    public void OnAltFire(InputValue value)
    {
        if (value.isPressed)
            altFirePressed = true;
    }

    public void OnSprint(InputValue value)
    {
        sprintHeld = value.isPressed;
    }

    public void OnScrollWheel(InputValue value)
    {
        scrollInput = value.Get<Vector2>();
    }

    void Movement()
    {
        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0)
            velocity.y = -1;

        Vector3 move =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        float speed = movementSpeed * (sprintHeld ? 2f : 1f);

        controller.Move(move * speed * Time.deltaTime);

        if (jumpPressed && grounded)
        {
            velocity.y += Mathf.Sqrt(jumpForce * -1f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Look()
    {
        float lookX = lookInput.x * lookSensitivityX * Time.deltaTime;
        float lookY = lookInput.y * lookSensitivityY * Time.deltaTime;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookX);
    }

    void HandleSwitchGun()
    {
        if (equippedGuns.Count == 0)
            return;

        if (scrollInput.y > 0)
        {
            gunIndex = (gunIndex + 1) % equippedGuns.Count;
            EquipGun(equippedGuns[gunIndex]);
        }
        else if (scrollInput.y < 0)
        {
            gunIndex--;
            if (gunIndex < 0)
                gunIndex = equippedGuns.Count - 1;

            EquipGun(equippedGuns[gunIndex]);
        }

        scrollInput = Vector2.zero;
    }

    void FireGun()
    {
        if (currentGun == null)
            return;

        // single shot
        if (firePressed)
        {
            currentGun.AttemptFire();
        }

        // automatic
        if (fireHeld)
        {
            currentGun.TryFireHeld();
        }

        if (altFirePressed)
        {
            currentGun.AttemptAltFire();
        }

        firePressed = false;
    }

    void EquipGun(Gun g)
    {
        currentGun?.Unequip();
        if (currentGun != null)
            currentGun.gameObject.SetActive(false);

        g.gameObject.SetActive(true);
        g.transform.parent = gunHold;
        g.transform.localPosition = Vector3.zero;

        currentGun = g;
        g.Equip(this);
    }

    public void AddGun(Gun g)
    {
        equippedGuns.Add(g);
        gunIndex = equippedGuns.Count - 1;
        EquipGun(g);
    }

    public void IncreaseAmmo(int amount)
    {
        currentGun?.AddAmmo(amount);
    }

    public void Respawn()
    {
        transform.position = origin;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.GetComponent<Damager>())
        {
            var collisionPoint = hit.collider.ClosestPoint(transform.position);
            var knockbackAngle = (transform.position - collisionPoint).normalized;
            velocity = 20 * knockbackAngle;
        }

        if (hit.gameObject.GetComponent<KillZone>())
        {
            Respawn();
        }
    }
}