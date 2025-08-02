using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject DamageEffect;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Boomerang")]
    public GameObject boomerangPrefab;
    public Transform throwPoint;
    public float boomerangCooldown = 0.5f;

    [Header("Hopping")]
    [SerializeField] private float bounceAmplitude = 0.2f;
    [SerializeField] private Transform visualBounceRoot;
    private bool isBouncing = false;
    private bool isCatching;

    public int Health = 5;

    private float cooldownTimer;
    private float originalY;
    private Rigidbody rb;
    private Camera cam;
    private float dmgCD;
    private bool isDead;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        cooldownTimer = 0f;
        originalY = transform.position.y;
    }

    void Update()
    {
        if(isDead) return;

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

        // Smoothly return bounce to ground
        if (!isBouncing)
        {
            Vector3 localPos = visualBounceRoot.localPosition;
            localPos.y = Mathf.Lerp(localPos.y, 0f, Time.deltaTime * 10f);
            visualBounceRoot.localPosition = localPos;
        }

        if(dmgCD > 0) dmgCD -= Time.deltaTime;
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
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, 0.2f);
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

    public void TriggerHopBounce()
    {
        if (!animator.GetBool("Move")) return; //Prevent bounce if not moving

        isBouncing = true;
        StopCoroutine("BounceRoutine");
        StartCoroutine(BounceRoutine());
    }

    private IEnumerator BounceRoutine()
    {
        float duration = 0.3f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float normalized = timer / duration;
            float bounce = Mathf.Sin(normalized * Mathf.PI) * bounceAmplitude;

            Vector3 localPos = visualBounceRoot.localPosition;
            localPos.y = bounce;
            visualBounceRoot.localPosition = localPos;

            yield return null;
        }

        isBouncing = false;

        // Reset to ground level
        Vector3 resetPos = visualBounceRoot.localPosition;
        resetPos.y = 0f;
        visualBounceRoot.localPosition = resetPos;
    }


    public void TakeDamage(int damage)
    {
        if(dmgCD <= 0 && !isDead)
        {
            Health -= damage;
            Debug.Log("Take Damage" + Health);

            GameObject dmg = Instantiate(DamageEffect, transform.position + Vector3.up, Quaternion.identity);
            Destroy(dmg, 1f);
            animator.SetTrigger("Hit");

            UIManager.instance.UpdateHealth(Health);

            dmgCD = 1.5f;
        }

        if (Health <= 0 && !isDead)
        {
            isDead = true;
            animator.SetTrigger("Death");
        }
    }
    public void PlayCatchAnimation()
    {
        if (isCatching) return; // already catching
        animator.SetTrigger("Catch");
        isCatching = true;

        // reset after animation ends
        Invoke(nameof(ResetCatchState), 0.5f); // adjust time to match your animation length
    }

    private void ResetCatchState()
    {
        isCatching = false;
    }
}
