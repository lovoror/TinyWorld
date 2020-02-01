using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickController : MonoBehaviour
{
    public Transform target;
    public float gravity = 8;
    public float walkSpeed = 1f;
    public float aimingSpeed = 180f;
    public float distance = 0.1f;
    public float wanderingCooldown = 1f;
    public float wanderingStartup = 1f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 direction;
    public float wanderingStartDelay;
    public float wanderingDelay;
    private bool running = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // compute direction
        bool previouslyWalking = direction.x != 0f || direction.z != 0f;
        direction = Vector3.zero;
        float d = Vector3.ProjectOnPlane(target.position - transform.position, Vector3.up).magnitude;
        if (d > distance + (previouslyWalking ? -0.1f : 0.1f))
        {
            direction = walkSpeed * (target.position - transform.position).normalized;
        }

        // random animations
        if (direction.x == 0f && direction.z == 0f)
        {
            if (wanderingStartDelay <= 0 && wanderingDelay >= wanderingCooldown)
            {
                int rand = Random.Range(0, 10);
                if(rand < 7)
                {
                    animator.SetBool("Eat", true);
                }
                else
                {
                    animator.SetBool("Eat", false);
                    animator.SetTrigger("Jump");
                    wanderingDelay = 0.5f * wanderingCooldown;
                }
            }

            if (wanderingStartDelay <= 0 && wanderingDelay > 0)
                wanderingDelay -= Time.deltaTime;
            if (wanderingStartDelay > 0)
                wanderingStartDelay -= Time.deltaTime;
            if(wanderingDelay <= 0)
            {
                wanderingStartDelay = 0.5f;
                wanderingDelay = wanderingCooldown;
            }
        }
        else
        {
            wanderingStartDelay = wanderingStartup;
            wanderingDelay = wanderingCooldown;
            animator.SetBool("Eat", false);
            animator.SetBool("Jump", false);
        }

        // compute animation parameters
        if (direction.x == 0f && direction.z == 0f)
        {
            animator.SetBool("Run", false);
            animator.SetBool("Walk", false);
            running = false;
        }
        else
        {
            if (!running && d > 4 * distance)
            {
                running = true;
                animator.SetBool("Walk", false);
                animator.SetBool("Run", true);
            }
            else if(running && d < 2 * distance)
            {
                animator.SetBool("Walk", true);
                animator.SetBool("Run", false);
                running = false;
            }
        }

        // move 
        direction.y = -gravity * Time.deltaTime;
        controller.Move(direction * (running ? 8f : 1f) * Time.deltaTime);

        // aiming
        if (direction.x != 0f || direction.z != 0f)
        {
            Quaternion goal = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, aimingSpeed * Time.deltaTime);
        }
    }
}
