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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
            var boomerang = Instantiate(boomerangPrefab, throwPoint.position, throwPoint.rotation);
            boomerang.GetComponent<BoomerangSimpleArc>().player = this.transform;
            boomerang.GetComponent<BoomerangSimpleArc>().playerController = this; // link back for cooldown handling
            cooldownTimer = boomerangCooldown; // Start cooldown
        }
    }

    void FixedUpdate()
    {
        // Get input
        float h = Input.GetAxisRaw("Horizontal"); // Raw = instant 1/-1 (snappy)
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v).normalized;

        // Calculate velocity
        Vector3 velocity = input * moveSpeed;

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

    void ThrowBoomerang()
    {
        if (boomerangPrefab != null && throwPoint != null)
        {
            GameObject Boom = Instantiate(boomerangPrefab, throwPoint.position, throwPoint.rotation);
            Boom.GetComponent<BoomerangSimpleArc>().player = throwPoint;
            Boom.GetComponent<BoomerangSimpleArc>().playerController = this;
        }
    }

    public void ResetThrowCooldown()
    {
        cooldownTimer = boomerangCooldown; // Normal cooldown if caught
    }

    public void ExtendThrowCooldown(float penaltyMultiplier)
    {
        cooldownTimer = boomerangCooldown * penaltyMultiplier; // Longer if missed
    }
}
