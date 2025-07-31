using UnityEngine;

public class BoomerangFuStyle : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Flight Settings")]
    public float radius = 6f;                // Arc radius
    public float flightSpeed = 180f;         // Degrees per second
    public float arcAngle = 180f;            // Outward arc angle before return
    public float catchDistance = 0.5f;       // Distance to catch/destroy

    [Header("Visual Settings")]
    public float spinSpeed = 1080f;

    private Vector3 outwardCenter;           // Outward arc center
    private Vector3 returnCenter;            // Return arc center
    private float currentAngle;
    private bool returning;
    private float returnProgress;

    void Start()
    {
        // Lock player's facing at throw time
        Vector3 forward = new Vector3(player.forward.x, 0f, player.forward.z).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        // Define arc centers (left for outward, right for return)
        outwardCenter = player.position - right * radius;
        returnCenter = player.position + right * radius;

        // Initial angle relative to outward center
        Vector3 startOffset = player.position - outwardCenter;
        currentAngle = Mathf.Atan2(startOffset.z, startOffset.x) * Mathf.Rad2Deg;

        // Spawn slightly left of player
        transform.position = player.position - right * 0.5f;

        returning = false;
        returnProgress = 0f;
    }

    void Update()
    {
        // Spin visual
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);

        if (!returning)
        {
            // Outward arc motion
            currentAngle -= flightSpeed * Time.deltaTime;
            Vector3 offset = AngleToOffset(outwardCenter, currentAngle);
            transform.position = offset;

            // Switch phase when outward arc is done
            if (Mathf.Abs(currentAngle) >= arcAngle)
            {
                returning = true;
                returnProgress = 0f;

                // Recalculate angle for return center (preserve current position)
                currentAngle = Mathf.Atan2(
                    (transform.position - returnCenter).z,
                    (transform.position - returnCenter).x
                ) * Mathf.Rad2Deg;
            }
        }
        else
        {
            // Return arc motion
            currentAngle -= flightSpeed * Time.deltaTime;
            Vector3 offset = AngleToOffset(returnCenter, currentAngle);
            transform.position = offset;

            // Track return arc progress
            returnProgress += (flightSpeed * Time.deltaTime) / arcAngle;

            // Only allow catch near end of return arc
            if (returnProgress > 0.8f && Vector3.Distance(transform.position, player.position) <= catchDistance)
            {
                Destroy(gameObject);
            }
        }
    }

    private Vector3 AngleToOffset(Vector3 center, float angle)
    {
        Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
        return center + offset;
    }
}
