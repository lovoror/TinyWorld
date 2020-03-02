using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionHelperArrowAnim : MonoBehaviour
{
    public float amplitude;
    public float speed;
    private float t;
    private Vector3 initialPos;

    void Start()
    {
        t = 0;
        initialPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        t += speed * Time.deltaTime;
        if (t > 6.283f)
            t = 0f;
        transform.localPosition = initialPos + Mathf.Sin(t) * amplitude * Vector3.right;
        
    }
    private void LateUpdate()
    {
        Vector3 s = transform.parent.localScale;
        transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);
    }
}
