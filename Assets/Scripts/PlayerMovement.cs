using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;

public class PlayerMovement : NetworkBehaviour
{

    public float thrustForceAcceleration = 1f;
    public float thrustForceDeacceleration = .5f;
    public float rotationSpeed = 1f;
    [SerializeField]
    private float maxVelocity = 10f;

    private Rigidbody2D rb;
    [Networked] private NetworkRigidbody2D rbNetwork { get; set; }

    private bool isAccelerating = false;
    private bool isDeaccelerating = false;

    public SpriteRenderer jetSprite;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody2D>();
        rbNetwork = rb.GetComponent<NetworkRigidbody2D>();
        jetSprite.enabled = false;
    }
    public override void FixedUpdateNetwork()
    {
        if (Runner.TryGetInputForPlayer<NetworkInputData>(Object.InputAuthority, out var input))
        {
            AccelerateShip(input);
            RotateShip(input);
        }

        if (Object.HasStateAuthority)
        {
            if (isAccelerating)
                rb.AddForce(thrustForceAcceleration * transform.up);
            else if (isDeaccelerating)
                rb.AddForce(-thrustForceDeacceleration * rb.velocity);

            rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);
        }

        
    }

    private void AccelerateShip(NetworkInputData input)
    {
        if(input.up)
        {
            isAccelerating = true;
            isDeaccelerating = false;
            jetSprite.enabled = true;
        }
        else if (input.down)
        {
            isAccelerating = false;
            isDeaccelerating = true;
            jetSprite.enabled = false;
        }
        else
        {
            isAccelerating = false;
            isDeaccelerating = false;
            jetSprite.enabled = false;
        }
    }
    private void RotateShip(NetworkInputData input)
    {
        if (input.left)
            transform.Rotate(rotationSpeed * Runner.DeltaTime * transform.forward);
        else if (input.right)
            transform.Rotate(-rotationSpeed * Runner.DeltaTime * transform.forward);
    }
}
