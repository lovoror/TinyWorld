using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Button3D : MonoBehaviour
{
    public InventoryViewer viewer;
    private void OnMouseDown()
    {
        viewer.ButtonCallback(transform.name, transform.parent.name);
    }
}
