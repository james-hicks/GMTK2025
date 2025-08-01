using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Boomerang")]
    public GameObject boomerangPrefab;
    public Transform throwPoint; // where the boomerang spawns from
    public float boomerangCooldown = 0.5f;

    private float cooldownTimer;

    private Rigidbody rb;
    private Camera cam;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        cam = Camera.main;
        cooldownTimer = 0f;
    }

    void Update()
    {
        // Handle rotation to face mouse
        RotateTowardsMouse();

        // Handle boomerang throwing
        cooldownTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0f)
        {
            // Find mouse world position
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Ground at y=0

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 mouseWorldPos = ray.GetPoint(distance);

                // Calculate throw distance from player to mouse point
                float throwDistance = Vector3.Distance(transform.position, mouseWorldPos);
                throwDistance = Mathf.Clamp(throwDistance, 5, 20);

                // Animation trigger throw
                animator.SetTrigger("Throw");

                // Spawn boomerang
                var boomerang = Instantiate(boomerangPrefab, throwPoint.position, throwPoint.rotation);
                var boomerangScript = boomerang.GetComponent<BoomerangSimpleArc>();
                boomerangScript.player = this.transform;
                boomerangScript.playerController = this;
                boomerangScript.forwardDistance = throwDistance; // Set distance dynamically

                cooldownTimer = boomerangCooldown;


            }
        }

        UIManager.instance.UpdateCooldownGraphic(cooldownTimer);
    }

    void FixedUpdate()
    {
        // Get input
        float h = Input.GetAxisRaw("Horizontal"); // Raw = instant 1/-1 (snappy)
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v).normalized;

        // Calculate velocity
        Vector3 velocity = input * moveSpeed;

        // Animation toggle move bool
        animator.SetBool("Move", input != Vector3.zero);

        // Apply directly to Rigidbody (no MovePosition)
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        // Rotate towards movement direction if moving
        if (input != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(input, Vector3.up);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, 0.2f); // Smooth turn
        }
        else
        {
            // Stop horizontal drift when no input
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    void RotateTowardsMouse()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            Vector3 lookDir = (point - transform.position).normalized;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
                transform.forward = lookDir;
        }
    }

    public void ResetThrowCooldown()
    {
        cooldownTimer = 0.1f; // Normal cooldown if caught
        UIManager.instance.UpdateCooldownGraphic(cooldownTimer);
    }
}
