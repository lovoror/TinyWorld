using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSlot : MonoBehaviour
{
    public ShieldItem equipedItem;
    public MeshFilter equipedMesh;

    public bool Equip(ShieldItem.Type type, bool forceUpdate = false)
    {
        if (type == equipedItem.type && !forceUpdate)
            return true;

        if (type != ShieldItem.Type.None)
        {
            ShieldItem newItem = Arsenal.Instance.Get(type);
            if (newItem)
            {
                MeshFilter mf = newItem.GetComponent<MeshFilter>();
                if (mf)
                {
                    Debug.Log("toto");
                    equipedMesh.mesh = mf.mesh;
                    ShieldItem.Copy(newItem, equipedItem);
                    return true;
                }
            }
        }
        equipedItem = ShieldItem.none;
        equipedMesh.mesh = null;
        return false;
    }
}
