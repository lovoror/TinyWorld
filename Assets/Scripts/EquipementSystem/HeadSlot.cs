using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadSlot : MonoBehaviour
{
    public HeadItem equipedItem;
    public MeshFilter equipedMesh;

    public bool Equip(HeadItem.Type type, bool forceUpdate = false)
    {
        if (type == equipedItem.type && !forceUpdate)
            return true;

        if (type != HeadItem.Type.None)
        {
            HeadItem newItem = Arsenal.Instance.Get(type);
            if (newItem)
            {
                MeshFilter mf = newItem.GetComponent<MeshFilter>();
                if (mf)
                {
                    equipedMesh.mesh = mf.mesh;
                    HeadItem.Copy(newItem, equipedItem);
                    return true;
                }
            }
        }
        equipedItem.Clear();
        equipedMesh.mesh = null;
        return false;
    }
}
