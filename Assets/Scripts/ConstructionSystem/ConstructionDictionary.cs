using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionDictionary : MonoBehaviour
{
    public string defaultName = "ConstructionBarracks";
    public List<GameObject> templateList;
    public Dictionary<string, GameObject> dictionary;
    public Dictionary<string, ConstructionTemplate> templateDictionary;

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
        templateDictionary = new Dictionary<string, ConstructionTemplate>();
        foreach (GameObject go in templateList)
        {
            dictionary.Add(go.name, go);
            ConstructionTemplate template = go.transform.Find("interactor").GetComponent<ConstructionTemplate>();
            templateDictionary.Add(go.name, template);
        }
    }

    public GameObject Get(string constructionName)
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

    public Sprite GetSprite(string constructionName)
    {
        if (templateDictionary.ContainsKey(constructionName))
        {
            return templateDictionary[constructionName].sprite;
        }
        else
        {
            Debug.LogWarning("No construction template named " + constructionName);
            return templateDictionary[defaultName].sprite;
        }
    }
}
