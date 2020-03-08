using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnEmpty : MonoBehaviour
{
    private RessourceContainer container;

    void Start()
    {
        container = GetComponent<RessourceContainer>();
    }
    
    void LateUpdate()
    {
        if (container.load == 0)
            Destroy(transform.parent.gameObject);
    }
}
