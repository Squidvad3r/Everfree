﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Transform cam;
    private World world;
    private Vector3 velocity;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.807f;

    public float playerWidth = 0.15f;

    private float horizontal;
    private float vertical;
    private float mouseX;
    private float mouseY;
    private float verticalMomentum = 0;

    public bool isJumping;
    public bool isGrounded;
    public bool isSprinting;
    public bool isCrouching;

    void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
    }

    void FixedUpdate()
    {
        CalculateVelocity();

        if(isJumping)
            Jump();

        transform.Rotate(Vector3.up * mouseX);
        cam.transform.Rotate(Vector3.right * -mouseY);
        transform.Translate(velocity, Space.World);
    }

    void Update()
    {
        GetPlayerInputs();
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        isJumping = false;
    }

    private void CalculateVelocity()
    {
        if(verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        if(isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;
        if((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        
        if(velocity.y < 0)
            velocity.y = CheckDownSpeed(velocity.y);
        else if(velocity.y > 0)
            velocity.y = CheckUpSpeed(velocity.y);
    }

    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        if(Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if(Input.GetButtonUp("Sprint"))
            isSprinting = false;
        
        if(isGrounded && Input.GetButtonDown("Jump"))
            isJumping = true;
    }

    private float CheckDownSpeed(float downSpeed)
    {
        if(world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth))
        || world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth))
        || world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
        || world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)))
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    private float CheckUpSpeed(float upSpeed)
    {
        if(world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1.8f + upSpeed, transform.position.z - playerWidth))
        || world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1.8f +  upSpeed, transform.position.z - playerWidth))
        || world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1.8f +  upSpeed, transform.position.z + playerWidth))
        || world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1.8f +  upSpeed, transform.position.z + playerWidth)))
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    public bool front
    {
        get
        {
            if(world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool back
    {
        get
        {
            if(world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool left
    {
        get
        {
            if(world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    
    public bool right
    {
        get
        {
            if(world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
