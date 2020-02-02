using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondSlot : MonoBehaviour
{
    public SecondItem equipedItem;
    public MeshFilter equipedMesh;

    public bool Equip(SecondItem.Type type, bool forceUpdate = false)
    {
        if (type == equipedItem.type && !forceUpdate)
            return true;

        if (type != SecondItem.Type.None)
        {
            SecondItem newItem = Arsenal.Instance.Get(type);
            if (newItem)
            {
                MeshFilter mf = newItem.GetComponent<MeshFilter>();
                if (mf)
                {
                    equipedMesh.mesh = mf.mesh;
                    SecondItem.Copy(newItem, equipedItem);
                    return true;
                }
            }
        }
        equipedItem = SecondItem.none;
        equipedMesh.mesh = null;
        return false;
    }
}
