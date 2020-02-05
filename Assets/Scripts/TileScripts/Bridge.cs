using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : MonoBehaviour
{
    public MeshCollider waterCollider;
    public Transform bridgepivot;

    public void Initialize(bool leftIsDirt)
    {
        bridgepivot.eulerAngles = new Vector3(0, leftIsDirt ? 90 : 0, 0);
        waterCollider.enabled = false;
    }
}
