using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryViewer : MonoBehaviour
{
    public bool visible = false;
    public float spacing;
    public BackpackSlot backpackSlot;
    public InventoryLineTemplate template;
    public GameObject pivot;
    public Transform container;
    public RessourceContainer backpack;
    public TextMesh loadSum;
    public Transform background;
    private AudioSource audiosource;
    public AudioClip onsound;
    public AudioClip offsound;

    

    void Start()
    {
        pivot.SetActive(visible);
        audiosource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            if(visible)
            {
                pivot.SetActive(false);
                foreach (Transform child in container)
                    Destroy(child.gameObject);
                if(audiosource)
                    audiosource.clip = offsound;
            }
            else if(backpackSlot.equipedItem.type != BackpackItem.Type.RessourceContainer || backpack.load == 0)
            {
                pivot.SetActive(true);
                loadSum.text = "empty or not\nequiped";
                loadSum.transform.localPosition = new Vector3(0, 0, 0);
                background.localScale = new Vector3(0, 0, 0);
                if (audiosource)
                    audiosource.clip = onsound;
            }
            else
            {
                pivot.SetActive(true);
                UpdateContent();
                if (audiosource)
                    audiosource.clip = onsound;
            }

            if (audiosource)
                audiosource.Play();
            visible = !visible;
        }
    }

    public void UpdateContent()
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);

        loadSum.text = backpack.load.ToString() + "/" + backpack.capacity.ToString();

        int lines = backpack.inventory.Count;
        background.localScale = new Vector3(1, spacing * lines + 0.1f, 1);
        background.localPosition = new Vector3(0, 0.5f * spacing * lines + 0.05f, 0);
        loadSum.transform.localPosition = new Vector3(0, spacing * lines + 0.1f, 0);

        Vector3 position = Vector3.zero;
        foreach(KeyValuePair<string, int> entry in backpack.inventory)
        {
            InventoryLineTemplate go = Instantiate(template, container);
            go.transform.localPosition = position;
            go.gameObject.SetActive(true);

            go.count.text = entry.Value.ToString();
            go.icon.sprite = ResourceDictionary.Instance.Get(entry.Key).icon;

            position.y += spacing;
        }
    }
}
