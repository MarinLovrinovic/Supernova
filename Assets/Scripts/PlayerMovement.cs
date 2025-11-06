using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    public float thrustForceAcceleration = 1f;
    public float thrustForceDeacceleration = .5f;
    public float rotationSpeed = 1f;
    public float maxVelocity = 10f;
    public SpriteRenderer jetSprite;

    private Rigidbody2D rb;
    private PlayerControls pc;

    private bool isThrusting = false;
    private bool isBraking = false;
    private float rotationInput = 0.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pc = new PlayerControls();

        pc.Gameplay.Rotate.performed += ctx => rotationInput = ctx.ReadValue<float>();
        pc.Gameplay.Rotate.canceled += ctx => rotationInput = 0.0f;

        pc.Gameplay.Thrust.performed += ctx => isThrusting = true;
        pc.Gameplay.Thrust.canceled += ctx => isThrusting = false;

        pc.Gameplay.Brake.performed += ctx => isBraking = true;
        pc.Gameplay.Brake.canceled += ctx => isBraking = false;
    }

    private void OnEnable() => pc.Enable();
    private void OnDisable() => pc.Disable();

    private void Start()
    {
        jetSprite.enabled = false;
    }

    private void FixedUpdate()
    {
        if (isThrusting)
        {
            rb.AddForce(transform.up * thrustForceAcceleration);
            jetSprite.enabled = true;
        }
        else if (isBraking)
        {
            rb.AddForce(-rb.velocity * thrustForceDeacceleration);
            jetSprite.enabled = false;
        }
        else
        {
            jetSprite.enabled = false;
        }

        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);

        rb.MoveRotation(rb.rotation + rotationInput * rotationSpeed * Time.fixedDeltaTime);
    }
}
