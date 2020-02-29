using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionTemplate : MonoBehaviour
{
    public Mesh[] steps;
    public List<string> transition;

    [Range(0f, 1f)]
    public float progress;
    private float lastProgress;
    public MeshFilter meshFilter;
    public GameObject finished;
    public SpriteMask mask1;
    public SpriteMask mask2;

    // Start is called before the first frame update
    void Start()
    {
        lastProgress = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (progress != lastProgress)
        {
            // text
            if (progress == 0.5f)
            {

            }

            // progress bars
            if (progress < 0.5f)
                meshFilter.sharedMesh = steps[0];
            else if(progress < 1f)
                meshFilter.sharedMesh = steps[1];
            else
            {
                if (finished)
                {
                    GameObject go = Instantiate(finished);
                    go.transform.parent = transform.parent;
                    go.transform.localPosition = transform.localPosition;
                    go.transform.localEulerAngles = new Vector3(-90, transform.localEulerAngles.y, transform.localEulerAngles.z);


                    Destroy(transform.gameObject);
                }
                else Debug.Log("Nothing to instanciate at end of construction process");
            }
        }
        mask1.alphaCutoff = 1f - Mathf.Clamp(2 * progress, 0f, 1f);
        mask2.alphaCutoff = 1f - Mathf.Clamp(2 * progress - 1f, 0f, 1f);
        lastProgress = progress;
    }
}
