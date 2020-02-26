using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectData : MonoBehaviour
{
    public int ressourceCount = 0;
    private void Start()
    {
        if (ressourceCount == 0)
            ressourceCount = Random.Range(3, 5);
    }
}
