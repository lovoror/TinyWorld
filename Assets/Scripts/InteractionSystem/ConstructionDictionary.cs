using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionDictionary : MonoBehaviour
{
    public string defaultName = "ConstructionBarracks";
    public List<GameObject> templateList;
    public Dictionary<string, GameObject> dictionary;
    public Dictionary<string, Sprite> spriteDictionary;

    // Singleton struct
    private static ConstructionDictionary _instance;
    public static ConstructionDictionary Instance { get { return _instance; } }

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

    private void Initialize()
    {
        dictionary = new Dictionary<string, GameObject>();
        spriteDictionary = new Dictionary<string, Sprite>();
        foreach (GameObject go in templateList)
        {
            dictionary.Add(go.name, go);
            spriteDictionary.Add(go.name, go.transform.Find("interactor").GetComponent<ConstructionTemplate>().sprite);
        }
    }

    GameObject Get(string constructionName)
    {
        if(dictionary.ContainsKey(constructionName))
        {
            return Instantiate(dictionary[constructionName]);
        }
        else
        {
            Debug.LogWarning("No construction template named " + constructionName);
            return Instantiate(dictionary[defaultName]);
        }
    }

    Sprite GetSprite(string constructionName)
    {
        if (spriteDictionary.ContainsKey(constructionName))
        {
            return spriteDictionary[constructionName];
        }
        else
        {
            Debug.LogWarning("No construction template named " + constructionName);
            return spriteDictionary[defaultName];
        }
    }
}
