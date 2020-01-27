using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeasantController : MonoBehaviour
{
    public float rotationSpeed = 80;
    private Vector3 direction = Vector3.zero;
    public float speed = 4;
    public float gravity = 8;
    private float rotation = 0;
    private CharacterController controller;
    Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        if (controller.isGrounded)
        {
            if(Input.GetKey(KeyCode.Z))
            {
                animator.SetFloat("walk", transform.localScale.y);
                direction = speed *  new Vector3(0, 0, 1);
                direction = transform.TransformDirection(direction);
            }
            if (Input.GetKeyUp(KeyCode.Z))
            {
                direction = Vector3.zero;
                animator.SetFloat("walk", 0);
            }
        }
        rotation += Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;
        direction.y = -gravity * Time.deltaTime;
        controller.Move(direction * Time.deltaTime);
        transform.eulerAngles = new Vector3(0, rotation, 0);
    }
}
