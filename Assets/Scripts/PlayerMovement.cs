using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float thrustForceAcceleration = 1f;
    public float thrustForceDeacceleration = .5f;
    public float rotationSpeed = 1f;
    [SerializeField]
    private float maxVelocity = 10f;

    private Rigidbody2D rb;
    private bool isAccelerating = false;
    private bool isDeaccelerating = false;
    private float rotationInput = 0.0f;

    public SpriteRenderer jetSprite;
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jetSprite.enabled = false;
    }

    void Update()
    {
        AccelerateShip();
        RotateShip();
    }
    private void FixedUpdate()
    {
        if (isAccelerating)
            rb.AddForce(thrustForceAcceleration * transform.up);
        else if (isDeaccelerating)
            rb.AddForce(-thrustForceDeacceleration * rb.velocity);
            
        rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);

        rb.MoveRotation(rb.rotation + rotationInput * rotationSpeed * Time.fixedDeltaTime);
    }
    private void AccelerateShip()
    {
        if(Input.GetKey(KeyCode.UpArrow))
        {
            isAccelerating = true;
            isDeaccelerating = false;
            jetSprite.enabled = true;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
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
    private void RotateShip()
    {
        rotationInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow))
            rotationInput = 1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            rotationInput = -1f;
    }
}
