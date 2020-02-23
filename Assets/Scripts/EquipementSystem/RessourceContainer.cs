using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RessourceContainer : MonoBehaviour
{
    public int capacity;
    public MeshRenderer[] itemMeshes;
    public int load = 0;
    public List<string> acceptedResources = new List<string>();
    public Dictionary<string, int> inventory = new Dictionary<string, int>();

    public bool HasSpace()
    {
        return load < capacity;
    }
    public void AddItem(string ressourceName, int ressourceCount)
    {
        if (!inventory.ContainsKey(ressourceName))
            inventory.Add(ressourceName, ressourceCount);
        else
            inventory[ressourceName] += ressourceCount;
        UpdateContent();
    }
    public void Clear()
    {
        inventory.Clear();
        load = 0;
        foreach (MeshRenderer mr in itemMeshes)
            mr.enabled = false;
    }
    public void UpdateContent()
    {
        if (itemMeshes.Length != 0)
        {
            List<string> names = new List<string>();
            foreach (KeyValuePair<string, int> entry in inventory)
            {
                for (int i = 0; i < entry.Value; i++)
                    names.Add(entry.Key);
            }

            for (int i = 0; i < itemMeshes.Length; i++)
            {
                if (5 * i < names.Count)
                {
                    itemMeshes[i].sharedMaterial = ResourceDictionary.Instance.Get(names[5 * i]).material;
                    itemMeshes[i].enabled = true;
                }
                else itemMeshes[i].enabled = false;
            }

            load = names.Count;
        }
        else
        {
            load = 0;
            foreach (KeyValuePair<string, int> entry in inventory)
                load += entry.Value;
        }
    }
}
