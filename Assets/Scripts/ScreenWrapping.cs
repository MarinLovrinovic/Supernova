using UnityEngine;
using Fusion.Addons.Physics;


public class ScreenWrapping : MonoBehaviour
{
    private const float offsetRatio = 0.025f / 0.6f;
    private float offset;
    private Rigidbody2D rb;

    private void Start()
    {
        offset = transform.localScale.y * offsetRatio;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(rb.position);

        Vector3 moveAdjustment = Vector3.zero;
        if (viewportPosition.x < -offset)
            moveAdjustment.x += 1 + 2 * offset;
        else if (viewportPosition.x > 1 + offset)
            moveAdjustment.x -= 1 + 2 * offset;

        if (viewportPosition.y < -offset)
            moveAdjustment.y += 1 + 2 * offset;
        else if (viewportPosition.y > 1 + offset)
            moveAdjustment.y -= 1 + 2 * offset;


        // dodano jer inace ne radi s NetworkRigidbody2D
        if (moveAdjustment != Vector3.zero)
        {
            Vector3 newPos = Camera.main.ViewportToWorldPoint(viewportPosition + moveAdjustment);
            newPos.z = 0;

            var nrb = GetComponent<NetworkRigidbody2D>();
            if (nrb != null)
            {
                Vector2 savedVelocity = rb.velocity;
                float currentZRotation = rb.rotation;
                Quaternion rotation = Quaternion.Euler(0, 0, currentZRotation);

                nrb.Teleport(newPos, rotation);

                rb.velocity = savedVelocity;
            }
            else
            {
                rb.position = newPos;
            }
        }
    }
}
