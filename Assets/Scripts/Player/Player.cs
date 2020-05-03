﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player : MonoBehaviour
{
    // Unity Components
    [HideInInspector]
    public Rigidbody2D rb2d;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public InputControl inputControl;


    // Player Attributes
    [SerializeField]
    Character character; //mauzilla, schneider, maurer or tischler
    [HideInInspector]
    public float speed;
    Vector3 initialScale;

    // Action timer
    [HideInInspector]
    public float attackAnim = 0;
    public float cooldownTime = 5;
    public float abilityCooldown = -1;
    public float abilityActiveDuration = -1;

    // colliding / triggering
    public Building collidingBuilding; // The Artisan Mauzilla is near

    // 

    public virtual void Start() {
        this.speed = GameManager.instance.playerSpeed;

        inputControl = GetComponent<InputControl>();
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        initialScale = transform.localScale;
    }

    public virtual void Update() {
        movePlayer();

        animatePlayer();
        
        controlAbility(inputControl.isAbilityKeyPressed());

        if (character != Character.mauzilla)
        {
            // Artisan is near a destroyed Building and pressing Action Key
            if (collidingBuilding && inputControl.isActionKeyPressedInFrame() && collidingBuilding.state == 1) {

                // Check if all required Artisans are near the Building
                if (collidingBuilding.RepairConditionsMet()) {
                    collidingBuilding.adjustHealth(1);
                    gameObject.GetComponent<AudioSource>().Play(0);
                } else {
                    Debug.Log("You're missing the right skills to repair this building!");
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.gameObject.CompareTag("building")) {
            Debug.Log("Artisan collided with " + col.gameObject.name);
            collidingBuilding = col.gameObject.GetComponent<Building>();
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col.gameObject.CompareTag("building")) {
            Debug.Log("Artisan stopped colliding with " + col.gameObject.name);
            collidingBuilding = null;
        }
    }

    public virtual void movePlayer()
    {
        Vector2 actualFrameMovement = inputControl.getCurrentMovement() * speed;
        rb2d.MovePosition(rb2d.position + actualFrameMovement);
    }

    public virtual void animatePlayer()
    {
        float moveHorizontal = inputControl.getCurrentMovement().x;
        float moveVertical = inputControl.getCurrentMovement().y;

        if (Mathf.Abs(moveHorizontal) > Mathf.Abs(moveVertical))
        {
            FlipLeftRight(moveHorizontal);
        } else {
            FlipUpDown(moveVertical);
        }
    }

    public bool isUsingAbility()
    {
        return abilityActiveDuration > 0;
    }

    public void controlAbility(bool isKeyPressed)
    {
        if (abilityCooldown <= 0 && isKeyPressed)
        {
            abilityCooldown = cooldownTime;
            abilityActiveDuration = 3;
        }
        else
        {
            if (abilityCooldown > -1)
            {
                abilityCooldown -= Time.deltaTime;
            }
            if (abilityActiveDuration > -1)
            {
                abilityActiveDuration -= Time.deltaTime;
            }

            if (!isKeyPressed)
            {
                abilityActiveDuration = -1;
            }
        }
    }

    public void FlipLeftRight(float moveHorizontal)
    {
        animator.SetInteger("Direction", 2);
        Vector3 theCurrentScale = transform.localScale;

        if (moveHorizontal > 0.1)
        {
            theCurrentScale.x = -initialScale.x;
        }
        else
        {
            theCurrentScale.x = initialScale.x;
        }

        transform.localScale = theCurrentScale;
    }

    public void FlipUpDown(float moveVertical)
    {
        if (moveVertical > 0.1)
        {
            animator.SetInteger("Direction", 1);
        }

        if (moveVertical < -0.1)
        {
            animator.SetInteger("Direction", 3);
        }
    }
}