using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlot : MonoBehaviour
{
    public WeaponItem equipedItem;
    public MeshFilter equipedMesh;

    public bool Equip(WeaponItem.Type type, bool forceUpdate = false)
    {
        if (type == equipedItem.type && !forceUpdate)
            return true;

        if (type != WeaponItem.Type.None)
        {
            WeaponItem newItem = Arsenal.Instance.Get(type);
            if (newItem)
            {
                MeshFilter mf = newItem.GetComponent<MeshFilter>();
                if (mf)
                {
                    equipedMesh.mesh = mf.mesh;
                    WeaponItem.Copy(newItem, equipedItem);
                    return true;
                }
            }
        }
        equipedItem.Clear();
        equipedMesh.mesh = null;
        return false;
    }
}
