using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float aimingSpeed = 4;
    public float aimingAttackSpeed = 20;
    public float runSpeed = 4;
    public float gravity = 8;
    public float attackCooldown = 0.8f;

    public bool attacking;

    private CharacterController controller;
    private Animator animator;
    private ParticleSystem particles;

    private Vector3 direction = Vector3.zero;
    private float attackDelay = 0f;
    private Vector3 target;
    private ParticleSystem.EmitParams emitParams;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        particles = GetComponent<ParticleSystem>();
        attacking = false;
        emitParams = new ParticleSystem.EmitParams();
    }


    void Update()
    {
        float speedFactor = Input.GetKey(KeyCode.LeftShift) ? 2 : 1;
        direction = Vector3.zero;

        // action or attack
        if (Input.GetKeyDown(KeyCode.Mouse0) && attackDelay <= 0)
        {
            animator.SetTrigger("attack");
            attackDelay = attackCooldown;
            attacking = true;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                target = hit.point;
            }
        }

        if (controller.isGrounded && !attacking)
        {
            // compute direction
            if (Input.GetKey(KeyCode.Z))
                direction = new Vector3(0, 0, 1);
            else if (Input.GetKey(KeyCode.S))
                direction = new Vector3(0, 0, -1);
            if (Input.GetKey(KeyCode.D))
                direction += new Vector3(1, 0, 0);
            else if (Input.GetKey(KeyCode.Q))
                direction += new Vector3(-1, 0, 0);
            direction.Normalize();

            // compute animation parameters
            if(direction.x == 0f && direction.z == 0f)
                animator.SetFloat("run", 0f);
            else
                animator.SetFloat("run", speedFactor);

            // update position
            direction = speedFactor * runSpeed * direction;
            direction = direction.x * Camera.main.transform.right + direction.z * Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        }
        
        // move 
        direction.y = -gravity * Time.deltaTime;
        controller.Move(direction * Time.deltaTime);

        // aiming
        if (attacking)
        {
            direction = (target - transform.position).normalized;
        }
        if (direction.x != 0f || direction.z != 0f)
        {
            Quaternion goal = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goal, (attacking ? aimingAttackSpeed : aimingSpeed) * Time.deltaTime);
        }

        if ((direction.x != 0f || direction.z != 0f) && Input.GetKey(KeyCode.LeftShift) && Random.Range(0,4) == 0)
        {
            particles.Emit(emitParams, 1);
        }
        if (attackDelay > 0)
            attackDelay -= Time.deltaTime;
    }

    public void AttackEnd()
    {
        attacking = false;
    }
}
