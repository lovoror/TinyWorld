using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChikenController : MonoBehaviour
{
    public float gravity = 8;
    public float walkSpeed = 1f;
    public float aimingSpeed = 180f;
    public float wanderingCooldown;
    public float wanderingTimeDispersion;
    public float wanderingRadius = 1f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 spawnPoint;
    private float wanderingDelay = 0f;
    private Vector3 direction;
    private Vector3 target;

    void Start()
    {
        spawnPoint = transform.position;
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // choose new target point
        if(wanderingDelay <= 0f)
        {
            wanderingDelay = wanderingCooldown + Random.Range(0f, wanderingTimeDispersion);

            int rand = Random.Range(0, 10);
            if (rand < 7)
            {
                Vector3 d = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                target = spawnPoint + Random.Range(0.4f, wanderingRadius) * d;
                animator.SetBool("Eat", false);
                animator.SetBool("Turn Head", false);
            }
            else if(rand < 9)
            {
                animator.SetBool("Eat", true);
                animator.SetBool("Turn Head", false);
                wanderingDelay -= 0.5f * wanderingCooldown;
            }
            else
            {
                animator.SetBool("Eat", false);
                animator.SetBool("Turn Head", true);
                wanderingDelay = 1f;
            }
        }

        // compute direction
        direction = Vector3.zero;
        if(Vector3.ProjectOnPlane(target - transform.position, Vector3.up).magnitude > 0.2f)
        {
            direction = walkSpeed * (target - transform.position).normalized;
        }

        // compute animation parameters
        if (direction.x == 0f && direction.z == 0f)
            animator.SetBool("Walk", false);
        else
            animator.SetBool("Walk", true);

        // move 
        direction.y = -gravity * Time.deltaTime;
        controller.Move(direction * Time.deltaTime);

        // aiming
        if (direction.x != 0f || direction.z != 0f)
        {
            Quaternion goal = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, aimingSpeed * Time.deltaTime);
        }
        
        // update timer
        if (wanderingDelay > 0)
            wanderingDelay -= Time.deltaTime;
    }
}
