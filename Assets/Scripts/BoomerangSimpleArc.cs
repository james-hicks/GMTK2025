using UnityEngine;

public class BoomerangSimpleArc : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public PlayerController playerController;
    [SerializeField] public GameObject HitFX;

    [Header("Flight Settings")]
    public float forwardDistance = 10f;
    public float flightDuration = 1.0f;
    public float returnDuration = 1.0f;
    public float arcWidth = 2f;
    public float spinSpeed = 1080f;
    public float catchDistance = 2.5f;
    public float offscreenDestroyDistance = 50f;
    public float flyPastSpeed = 10f;
    public float catchGraceTime = 0.3f; // Extra time after passing player to still catch

    private Vector3 startPos;
    private Vector3 forwardDir;
    private Vector3 sideDir;
    private Vector3 apexPos;
    private Vector3 apexControl;
    private bool returning;
    private bool caught;
    private bool missed;
    private float timer;
    private float side;

    private Vector3 lastDirection;
    private float graceTimer;

    void Start()
    {
        startPos = player.position;
        forwardDir = new Vector3(player.forward.x, 0f, player.forward.z).normalized;
        sideDir = Vector3.Cross(Vector3.up, forwardDir).normalized;

        side = 1; // fixed arc side
        apexPos = startPos + forwardDir * forwardDistance;
        apexControl = apexPos + sideDir * arcWidth * side;

        timer = 0f;
        returning = false;
        caught = false;
        missed = false;
        graceTimer = 0f;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);

        if (!returning)
        {
            // Outward arc
            timer += Time.deltaTime / flightDuration;
            float t = Mathf.Clamp01(timer);
            transform.position = QuadraticBezier(startPos, apexControl, apexPos, t);

            if (t >= 1f)
            {
                returning = true;
                timer = 0f;
            }
        }
        else if (!caught && !missed)
        {
            // Return arc
            timer += Time.deltaTime / returnDuration;
            float t = Mathf.Clamp01(timer);
            Vector3 returnControl = apexPos + sideDir * arcWidth * -side;

            Vector3 newPos = QuadraticBezier(apexPos, returnControl, player.position, t);

            lastDirection = (newPos - transform.position).normalized;
            transform.position = newPos;

            // Catch check
            if (Vector3.Distance(transform.position, player.position) <= catchDistance && Input.GetKeyDown(KeyCode.E))
            {
                CatchBoomerang();
                return;
            }

            // When return arc completes, start grace period
            if (t >= 1f)
            {
                graceTimer = catchGraceTime;
                missed = true; // Transition to fly-past mode, but still catchable briefly
            }
        }
        else if (missed && !caught)
        {
            // Fly past while grace timer is active
            transform.position += lastDirection * flyPastSpeed * Time.deltaTime;

            if (graceTimer > 0f)
            {
                graceTimer -= Time.deltaTime;

                // Still catchable during grace time
                if (Vector3.Distance(transform.position, player.position) <= catchDistance && Input.GetKeyDown(KeyCode.E))
                {
                    CatchBoomerang();
                    return;
                }
            }
            else
            {

                // Destroy if far away
                if (Vector3.Distance(transform.position, player.position) > offscreenDestroyDistance)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    void LateUpdate()
    {
        // Lock height to player's throw point height
        Vector3 pos = transform.position;
        pos.y = player.position.y + 1f; // Adjust offset if needed
        transform.position = pos;
    }

    private void CatchBoomerang()
    {
        caught = true;

        if (playerController != null)
        {
            playerController.ResetThrowCooldown();
            playerController.PlayCatchAnimation();
        }

        Destroy(gameObject);
        Debug.Log("Boomerang caught!");
    }

    private Vector3 QuadraticBezier(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        return Mathf.Pow(1 - t, 2) * start +
               2 * (1 - t) * t * control +
               Mathf.Pow(t, 2) * end;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out IHitable hit))
        {
            hit.GetHit(1);

            //GameObject fx = Instantiate(HitFX, other.transform.position, Quaternion.identity);
            //Destroy(fx, 2f);
        }
    }
}
