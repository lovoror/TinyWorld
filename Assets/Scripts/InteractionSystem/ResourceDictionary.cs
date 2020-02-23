using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceDictionary : MonoBehaviour
{
    public string defaultName = "Stone";
    public Dictionary<string, ResourceType> resourceTypes;

    // Singleton struct
    private static ResourceDictionary _instance;
    public static ResourceDictionary Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        Initialize();
    }

    public void Initialize()
    {
        resourceTypes = new Dictionary<string, ResourceType>();

        foreach(Transform child in transform)
        {
            ResourceType resource = child.GetComponent<ResourceType>();
            if (resource)
            {
                resourceTypes.Add(resource.resourceName, resource);
            }
            else Debug.LogWarning("object " + child.name + " don't have ResourceType attached to it");
        }
    }
    public ResourceType Get(string resourceName)
    {
        if (resourceTypes.ContainsKey(resourceName))
            return resourceTypes[resourceName];
        Debug.LogWarning("requested name " + resourceName + " doesn't point to a valid resurce");
        return resourceTypes[defaultName];
    }
    public string GetNameFromType(InteractionType.Type type)
    {
        switch (type)
        {
            case InteractionType.Type.collectCrystal: return "Crystal";
            case InteractionType.Type.collectGold: return "Gold";
            case InteractionType.Type.collectIron: return "Iron";
            case InteractionType.Type.collectStone: return "Stone";
            case InteractionType.Type.collectWood: return "Wood";
            case InteractionType.Type.collectWheet: return "Wheat";
            default:
                Debug.LogWarning("requested type " + type.ToString() + " is not associated to a resource name");
                return defaultName;
        }
    }
}
