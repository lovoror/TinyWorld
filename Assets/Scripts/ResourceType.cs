using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceType : MonoBehaviour
{
    public string resourceName;
    public Sprite icon;
    public Color gainColor;
    public List<AudioClip> collectionSound = new List<AudioClip>();
    public Material material;
    public List<string> collectionTools = new List<string>();
}
