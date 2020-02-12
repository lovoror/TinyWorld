using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessourceContainer : MonoBehaviour
{
    public int capacity;
    public MeshRenderer[] itemMeshes;
    public int load = 0;
    public Dictionary<string, int> inventory = new Dictionary<string, int>();

    public bool HasSpace()
    {
        return load < capacity;
    }
    public void AddItem(Material ressourceMaterial, int ressourceCount)
    {
        if (!inventory.ContainsKey(ressourceMaterial.name))
            inventory.Add(ressourceMaterial.name, ressourceCount);
        else
            inventory[ressourceMaterial.name] += ressourceCount;
        load += ressourceCount;

        for (int i = 0; i < itemMeshes.Length; i++)
        {
            if (!itemMeshes[i].enabled && 5 * i < load)
            {
                itemMeshes[i].sharedMaterial = ressourceMaterial;
                itemMeshes[i].enabled = true;
            }
        }
    }
    public void Clear()
    {
        inventory.Clear();
        load = 0;
        foreach (MeshRenderer mr in itemMeshes)
            mr.enabled = false;
    }
}
