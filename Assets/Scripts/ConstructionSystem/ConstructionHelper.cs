using UnityEngine;

public class ConstructionHelper : MonoBehaviour
{
    [Header("Helper mode")]
    public int mode;

    [Header("Building acess helper anim")]
    public GameObject arrow;
    public float amplitude;
    public float speed;
    private float t;
    private Vector3 initialArrowPos;

    [Header("")]
    public GameObject delete;

    void Start()
    {
        mode = 0;
        t = 0;
        initialArrowPos = arrow.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        arrow.SetActive(mode == 1);
        delete.SetActive(mode == 2);

        if (mode == 1)
        {
            t += speed * Time.deltaTime;
            if (t > 6.283f)
                t = 0f;
            arrow.transform.localPosition = initialArrowPos + Mathf.Sin(t) * amplitude * Vector3.right;
        }
        else if(mode == 2)
        {

        }
    }
    private void LateUpdate()
    {
        Vector3 s = arrow.transform.parent.localScale;
        arrow.transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);
    }
}
