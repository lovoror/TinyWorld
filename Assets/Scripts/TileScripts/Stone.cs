using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public void Initialize(int rockSize)
    {
        GameObject go = Instantiate(TilePrefabsContainer.Instance.GetStone(rockSize));
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        go.SetActive(true);
    }
}
