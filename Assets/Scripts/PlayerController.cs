using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject DamageEffect;
    [SerializeField] private GameObject CatchEffect;

    [Header("Boomerang")]
    public GameObject boomerangPrefab;
    public Transform throwPoint;


    [Header("Hopping")]
    [SerializeField] private float bounceAmplitude = 0.2f;
    [SerializeField] private Transform visualBounceRoot;
    private bool isBouncing = false;
    private bool isCatching;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.5f;

    private bool isDashing = false;
    private float dashCooldownTimer = 0f;
    private float mouseLookDelay = 0f;

    [Header("Upgrade-able Stats")]
    public int maxHealth = 5;
    private int currentHealth;
    public float baseMoveSpeed = 5f;
    private float moveSpeed;
    public float boomerangCooldown = 0.5f;
    public int Damage = 1;
    public float BoomerangBaseScale = 1.5f;


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

        moveSpeed = baseMoveSpeed;
        currentHealth = maxHealth;

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
                boomerangScript.forwardDistance = throwDistance;
                boomerangScript.Damage = Damage;// Set distance dynamically
                boomerang.transform.localScale = Vector3.one * BoomerangBaseScale;

                cooldownTimer = boomerangCooldown;


            }
        }

        if (mouseLookDelay > 0f)
            mouseLookDelay -= Time.deltaTime;

  UIManager.instance.UpdateBoomerangCooldown(cooldownTimer, boomerangCooldown);
UIManager.instance.UpdateDashCooldown(dashCooldownTimer, dashCooldown);

        // Smoothly return bounce to ground
        if (!isBouncing)
        {
            Vector3 localPos = visualBounceRoot.localPosition;
            localPos.y = Mathf.Lerp(localPos.y, 0f, Time.deltaTime * 10f);
            visualBounceRoot.localPosition = localPos;
        }

        if(dmgCD > 0) dmgCD -= Time.deltaTime;
        
        // Dash cooldown
        dashCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0f && !isDashing)
        {
            StartCoroutine(Dash());
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
        if (isDashing || mouseLookDelay > 0f) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            Vector3 lookDir = (point - transform.position).normalized;
            lookDir.y = 0;

            if (lookDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
    }

    public void ResetThrowCooldown()
    {
        cooldownTimer = 0.1f; // Normal cooldown if caught
        UIManager.instance.UpdateBoomerangCooldown(cooldownTimer, boomerangCooldown);
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

    private IEnumerator Dash()
    {
        isDashing = true;

        // Capture dash direction from input (or use forward fallback)
        Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        if (inputDir == Vector3.zero)
        {
            isDashing = false;
            yield break;
        }

        float originalSpeed = moveSpeed;
        moveSpeed = dashSpeed;

        // Rotate to face dash direction instantly
        Quaternion dashRotation = Quaternion.LookRotation(inputDir, Vector3.up);
        transform.rotation = dashRotation;

        animator.SetTrigger("Dash");

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.linearVelocity = inputDir * dashSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        moveSpeed = originalSpeed;
        isDashing = false;
        mouseLookDelay = 0.3f;
        dashCooldownTimer = dashCooldown;

        // Return to mouse facing on next frame
        RotateTowardsMouse();
    }



    public void TakeDamage(int damage)
    {
        if(dmgCD <= 0 && !isDead)
        {
            currentHealth -= damage;

            GameObject dmg = Instantiate(DamageEffect, transform.position + Vector3.up, Quaternion.identity);
            Destroy(dmg, 3f);
            animator.SetTrigger("Hit");

            UIManager.instance.UpdateHealth(currentHealth);

            dmgCD = 1.5f;
        }

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            animator.SetTrigger("Death");
        }
    }
    public void PlayCatchAnimation()
    {
        if (isCatching) return; // already catching
        GameObject catchFX = Instantiate(CatchEffect, transform.position + Vector3.up, Quaternion.identity);
        Destroy(catchFX, 3f);
        animator.SetTrigger("Catch");
        isCatching = true;

        // reset after animation ends
        Invoke(nameof(ResetCatchState), 0.5f); // adjust time to match your animation length
    }

    private void ResetCatchState()
    {
        isCatching = false;
    }


    public void ApplyUpgrades(int upgradeIndex)
    {
        switch (upgradeIndex)
        {
            case 0:
                Damage += 1;
                break;
            case 1:
                boomerangCooldown -= 0.1f;
                break;
            case 2:
                moveSpeed += 0.2f;
                break;
            case 3:
                maxHealth += 1;
                currentHealth += 1;
                UIManager.instance.UpdateHealth(currentHealth);
                break;
            case 4:
                BoomerangBaseScale += 0.1f;
                break;
        }
    }



}
