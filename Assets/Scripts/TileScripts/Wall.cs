using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public GameObject prefabA;
    public GameObject prefabB;
    public GameObject prefabC;
    public GameObject prefabD;
    public GameObject prefabE;
    public GameObject prefabF;

    public int configuration;

    public void Initialize(bool xp, bool xm, bool zp, bool zm)
    {
        // compute configuration and choose the resolve mesh algorithm accordingly
        configuration = (zp ? 0 : 1) << 3 | (zm ? 0 : 1) << 2 | (xp ? 0 : 1) << 1 | (xm ? 0 : 1) << 0;
        float rotation = 0f;
        GameObject prefab = null;
        switch (configuration)
        {
            case 0:
                prefab = prefabA;
                rotation = 0f;
                break;
            case 1:
                prefab = prefabB;
                rotation = 0f;
                break;
            case 2:
                prefab = prefabB;
                rotation = 180f;
                break;
            case 3:
                prefab = prefabC;
                rotation = 0f;
                break;
            case 4:
                prefab = prefabB;
                rotation = 90f;
                break;
            case 5:
                prefab = prefabD;
                rotation = 0f;
                break;
            case 6:
                prefab = prefabD;
                rotation = 90f;
                break;
            case 7:
                prefab = prefabE;
                rotation = 90f;
                break;
            case 8:
                prefab = prefabB;
                rotation = -90f;
                break;
            case 9:
                prefab = prefabD;
                rotation = -90f;
                break;
            case 10:
                prefab = prefabD;
                rotation = -180f;
                break;
            case 11:
                prefab = prefabE;
                rotation = -90f;
                break;
            case 12:
                prefab = prefabC;
                rotation = 90f;
                break;
            case 13:
                prefab = prefabE;
                rotation = 0f;
                break;
            case 14:
                prefab = prefabE;
                rotation = 180f;
                break;
            case 15:
                prefab = prefabF;
                rotation = 0f;
                break;
            default:
                Debug.LogError("Wall init : invald tile configuration");
                break;
        }

        // end
        GameObject go = Instantiate(prefab);
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        transform.localEulerAngles = new Vector3(0, rotation, 0);
    }
}
