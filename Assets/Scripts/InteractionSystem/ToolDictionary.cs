using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolDictionary : MonoBehaviour
{
    public string defaultName = "Pickaxe";
    public Dictionary<string, ToolType> toolTypes;

    // Singleton struct
    private static ToolDictionary _instance;
    public static ToolDictionary Instance { get { return _instance; } }

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
        toolTypes = new Dictionary<string, ToolType>();

        foreach (Transform child in transform)
        {
            ToolType tool = child.GetComponent<ToolType>();
            if (tool)
            {
                toolTypes.Add(tool.toolName, tool);
            }
            else Debug.LogWarning("object " + child.name + " don't have ToolType attached to it");
        }
    }

    public ToolType Get(string toolName)
    {
        if (toolTypes.ContainsKey(toolName))
            return toolTypes[toolName];
        Debug.LogWarning("requested name " + toolName + " doesn't point to a valid tool");
        return toolTypes[defaultName];
    }
}
