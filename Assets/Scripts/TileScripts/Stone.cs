using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public int size;
    public int yieldPerSize = 4;
    public void Initialize(int rockSize)
    {
        size = rockSize;

        GameObject go = Instantiate(TilePrefabsContainer.Instance.GetStone(rockSize));
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        go.SetActive(true);

        int dispersion = (int)(0.25f * (rockSize + 1) * yieldPerSize);
        go.transform.Find("Interactor").GetComponent<CollectData>().ressourceCount = Random.Range((rockSize + 1) * yieldPerSize - dispersion, (rockSize + 1) * yieldPerSize + dispersion);
    }
}
