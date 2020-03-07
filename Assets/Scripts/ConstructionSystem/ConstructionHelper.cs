using System.Collections.Generic;
using UnityEngine;

public class ConstructionHelper : MonoBehaviour
{
    [Header("Helper mode")]
    public int mode;

    [Header("Building acess helper anim")]
    public GameObject arrow;
    public GameObject orientationApivot;
    public GameObject orientationBpivot;
    public List<Transform> orientationA;
    public List<Transform> orientationB;
    public TextMesh textA;
    public TextMesh textB;
    public float amplitude;
    public float speed;
    private float t;
    private Vector3 initialArrowPos;

    [Header("")]
    public GameObject delete;
    private List<Vector3> initialOrientationA = new List<Vector3>();
    private List<Vector3> initialOrientationB = new List<Vector3>();
    private Vector3 pivotAdelta;
    private Vector3 pivotBdelta;

    void Start()
    {
        mode = 0;
        t = 0;
        initialArrowPos = arrow.transform.localPosition;
        foreach (Transform t in orientationA)
            initialOrientationA.Add(t.localScale);
        foreach (Transform t in orientationB)
            initialOrientationB.Add(t.localScale);
        pivotAdelta = orientationApivot.transform.position - transform.position;
        pivotBdelta = orientationBpivot.transform.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        arrow.SetActive(mode == 1);
        delete.SetActive(mode == 2);
        orientationApivot.SetActive(mode == 1);
        orientationBpivot.SetActive(mode == 1);

        if (mode == 1)
        {
            t += speed * Time.deltaTime;
            if (t > 6.283f)
                t = 0f;
            arrow.transform.localPosition = initialArrowPos + Mathf.Sin(t) * amplitude * Vector3.right;

            for(int i = 0; i < orientationA.Count; i++)
                orientationA[i].localScale = Vector3.MoveTowards(orientationA[i].localScale, initialOrientationA[i], 3 * Time.deltaTime);
            for (int i = 0; i < orientationB.Count; i++)
                orientationB[i].localScale = Vector3.MoveTowards(orientationB[i].localScale, initialOrientationB[i], 3 * Time.deltaTime);
        }
    }
    private void LateUpdate()
    {
        Vector3 s = arrow.transform.parent.localScale;
        arrow.transform.localScale = new Vector3(3f / s.x, 3f / s.y, 3f / s.z);

        orientationApivot.transform.position = transform.position + pivotAdelta;
        orientationBpivot.transform.position = transform.position + pivotBdelta;
        orientationApivot.transform.rotation = Quaternion.identity;
        orientationBpivot.transform.rotation = Quaternion.identity;
    }
    public void SetKeys(KeyCode keyA, KeyCode keyB)
    {
        textA.text = keyA.ToString();
        textB.text = keyB.ToString();
    }
    public void orientationAtrigger()
    {
        foreach (Transform t in orientationA)
            t.localScale = 2f * Vector3.one;
    }
    public void orientationBtrigger()
    {
        foreach (Transform t in orientationB)
            t.localScale = 2f * Vector3.one;
    }
}
