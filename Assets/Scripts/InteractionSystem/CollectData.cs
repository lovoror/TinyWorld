using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectData : MonoBehaviour
{
    public int ressourceCount;
    private void Start()
    {
        ressourceCount = Random.Range(3, 5);
    }
}
