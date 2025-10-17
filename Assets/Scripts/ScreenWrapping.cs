using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenWrapping : MonoBehaviour
{
    private const float offsetRatio = 0.025f/0.6f;
    private float offset;
    private void Start()
    {
        offset = transform.localScale.y * offsetRatio;
    }

    private void Update()
    {
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(transform.position);

        Vector3 moveAdjustment = Vector3.zero;
        if(viewportPosition.x < -offset)
            moveAdjustment.x += 1 + 2*offset;
        else if(viewportPosition.x > 1 + offset)
            moveAdjustment.x -= 1 + 2*offset;

        if (viewportPosition.y < -offset)
            moveAdjustment.y += 1 + 2*offset;
        else if (viewportPosition.y > 1 + offset)
            moveAdjustment.y -= 1 + 2*offset;

        transform.position = Camera.main.ViewportToWorldPoint(viewportPosition + moveAdjustment);
    }
}
