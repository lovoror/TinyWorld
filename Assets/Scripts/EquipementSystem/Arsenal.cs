using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arsenal : MonoBehaviour
{
    [Header("Backpack items")]
    public List<BackpackItem> backpackObjectList;
    public Dictionary<BackpackItem.Type, BackpackItem> backpackDictionary;

    [Header("Weapons items")]
    public List<WeaponItem> weaponObjectList;
    public Dictionary<WeaponItem.Type, WeaponItem> weaponDictionary;

    [Header("Heads items")]
    public List<HeadItem> headObjectList;
    public Dictionary<HeadItem.Type, HeadItem> headDictionary;

    [Header("Second hand items")]
    public List<SecondItem> secondObjectList;
    public Dictionary<SecondItem.Type, SecondItem> secondDictionary;

    [Header("Shield items")]
    public List<ShieldItem> shieldObjectList;
    public Dictionary<ShieldItem.Type, ShieldItem> shieldDictionary;

    [Header("Bodies items")]
    public List<BodyItem> bodyObjectList;
    public Dictionary<BodyItem.Type, BodyItem> bodyDictionary;


    // Singleton struct
    private static Arsenal _instance;
    public static Arsenal Instance { get { return _instance; } }

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
    }

    void Start()
    {
        // create backpack association table
        backpackDictionary = new Dictionary<BackpackItem.Type, BackpackItem>();
        foreach(BackpackItem b in backpackObjectList)
        {
            if (!backpackDictionary.ContainsKey(b.type))
            {
                backpackDictionary[b.type] = b;
            }
            else Debug.LogError("In Arsenal : The backpack dictionary already contain an item of this type, see gameObject " + b.gameObject);
        }

        // create weapons association table
        weaponDictionary = new Dictionary<WeaponItem.Type, WeaponItem>();
        foreach (WeaponItem w in weaponObjectList)
        {
            if (!weaponDictionary.ContainsKey(w.type))
            {
                weaponDictionary[w.type] = w;
            }
            else Debug.LogError("In Arsenal : The weapon dictionary already contain an item of this type, see gameObject " + w.gameObject);
        }

        // create weapons association table
        headDictionary = new Dictionary<HeadItem.Type, HeadItem>();
        foreach (HeadItem h in headObjectList)
        {
            if (!headDictionary.ContainsKey(h.type))
            {
                headDictionary[h.type] = h;
            }
            else Debug.LogError("In Arsenal : The head dictionary already contain an item of this type, see gameObject " + h.gameObject);
        }

        // create weapons association table
        secondDictionary = new Dictionary<SecondItem.Type, SecondItem>();
        foreach (SecondItem s in secondObjectList)
        {
            if (!secondDictionary.ContainsKey(s.type))
            {
                secondDictionary[s.type] = s;
            }
            else Debug.LogError("In Arsenal : The second hand dictionary already contain an item of this type, see gameObject " + s.gameObject);
        }

        // create weapons association table
        shieldDictionary = new Dictionary<ShieldItem.Type, ShieldItem>();
        foreach (ShieldItem s in shieldObjectList)
        {
            if (!shieldDictionary.ContainsKey(s.type))
            {
                shieldDictionary[s.type] = s;
            }
            else Debug.LogError("In Arsenal : The shield dictionary already contain an item of this type, see gameObject " + s.gameObject);
        }

        // create weapons association table
        bodyDictionary = new Dictionary<BodyItem.Type, BodyItem>();
        foreach (BodyItem b in bodyObjectList)
        {
            if (!bodyDictionary.ContainsKey(b.type))
            {
                bodyDictionary[b.type] = b;
            }
            else Debug.LogError("In Arsenal : The body dictionary already contain an item of this type, see gameObject " + b.gameObject);
        }
    }
}
