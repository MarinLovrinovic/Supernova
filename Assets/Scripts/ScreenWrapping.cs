using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;

public class ScreenWrapping : NetworkBehaviour
{
    private const float offsetRatio = 0.7f, margin = 0.05f;
    private float offset;
    private float minX, maxX, minY, maxY;
    private NetworkRigidbody2D rbNetwork;
    public override void Spawned()
    {
        rbNetwork = GetComponent<NetworkRigidbody2D>();

        offset = transform.localScale.y * offsetRatio;

        if (Camera.main)
        {
            float camHeight = 2f * Camera.main.orthographicSize;
            float camWidth = camHeight * Camera.main.aspect;

            minX = Camera.main.transform.position.x - camWidth / 2f;
            maxX = Camera.main.transform.position.x + camWidth / 2f;
            minY = Camera.main.transform.position.y - camHeight / 2f;
            maxY = Camera.main.transform.position.y + camHeight / 2f;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        Vector3 moveAdjustment = transform.position;

        if (transform.position.x < minX - offset)
            moveAdjustment.x = maxX + offset - margin;
        else if (transform.position.x > maxX + offset)
            moveAdjustment.x = minX - offset + margin;

        if (transform.position.y < minY - offset)
            moveAdjustment.y = maxY + offset - margin ;
        else if (transform.position.y > maxY + offset)
            moveAdjustment.y = minY - offset + margin ;

        if(transform.position != moveAdjustment)
        {
            if (rbNetwork)
                rbNetwork.Teleport(moveAdjustment);
            else
                transform.SetPositionAndRotation(moveAdjustment, transform.rotation);
        }
    }
}
