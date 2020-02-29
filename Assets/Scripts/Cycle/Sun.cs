using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    public float daySpeed = 1f;
    public Vector2 tresholds;

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(Vector3.zero, Vector3.right, daySpeed * Time.deltaTime);
        transform.LookAt(Vector3.zero);


    }
}
