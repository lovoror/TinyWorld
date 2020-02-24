using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InventoryViewer : MonoBehaviour
{
    public bool visible = false;
    public RessourceContainer storage;
    public float spacing;
    public PlayerController player;
    public Vector2 backgroundWidth = new Vector2(2.4f, 1.5f);
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
    public AudioClip noktransaction;
    public AudioClip oktransaction;

    private bool prevStorage = false;

    void Start()
    {
        pivot.SetActive(visible);
        audiosource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //  show update
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
            else if(storage == null && (backpackSlot.equipedItem.type != BackpackItem.Type.RessourceContainer || backpack.load == 0))
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
                if(storage == null)
                    UpdateContent(backpack.inventory);
                else
                    UpdateContent(GetFusionInventory());
                if (audiosource)
                    audiosource.clip = onsound;
            }

            if (audiosource)
                audiosource.Play();
            visible = !visible;
        }

        //  storage update
        if(visible && prevStorage != (storage != null))
        {
            if (storage == null && (backpackSlot.equipedItem.type != BackpackItem.Type.RessourceContainer || backpack.load == 0))
            {
                pivot.SetActive(true);
                loadSum.text = "empty or not\nequiped";
                loadSum.transform.localPosition = new Vector3(0, 0, 0);
                background.localScale = new Vector3(0, 0, 0);
                foreach (Transform child in container)
                    Destroy(child.gameObject);
            }
            else
            {
                pivot.SetActive(true);
                if (storage == null)
                    UpdateContent(backpack.inventory);
                else
                    UpdateContent(GetFusionInventory());
            }
        }

        // other update
        if (visible)
        {
            UpdateArrowVisibility();

            Vector3 s = background.localScale;
            background.localScale = new Vector3(((storage != null) ? backgroundWidth.x : backgroundWidth.y), s.y, s.z);

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50f, 1 << LayerMask.NameToLayer("PlayerUI")))
                {
                    string command = hit.collider.gameObject.name;
                    string resource = hit.collider.transform.parent.name;
                    InventoryLineTemplate line = hit.collider.transform.parent.GetComponent<InventoryLineTemplate>();

                    if (command == "arrow-" && line.down.enabled)
                    {
                        if (backpack.inventory[resource] <= 0 || !storage.HasSpace())
                        {
                            if (audiosource)
                            { 
                                audiosource.clip = noktransaction;
                                audiosource.Play();
                            }
                        }
                        else
                        {
                            if (audiosource)
                            {
                                audiosource.clip = oktransaction;
                                audiosource.Play();
                            }
                            backpack.inventory[resource]--;
                            storage.AddItem(resource, 1);
                            if (backpack.inventory[resource] <= 0)
                                backpack.inventory.Remove(resource);
                            UpdateContent(GetFusionInventory());
                        }
                    }
                    else if (command == "arrow--" && line.down2.enabled)
                    {
                        if (backpack.inventory[resource] <= 0 || !storage.HasSpace())
                        {
                            if (audiosource)
                            {
                                audiosource.clip = noktransaction;
                                audiosource.Play();
                            }
                        }
                        else
                        {
                            int maximumTransfert = Mathf.Min(storage.capacity - storage.load, backpack.inventory[resource]);
                            if (audiosource)
                            {
                                audiosource.clip = oktransaction;
                                audiosource.Play();
                            }
                            storage.AddItem(resource, maximumTransfert);
                            backpack.inventory[resource] -= maximumTransfert;
                            if (backpack.inventory[resource] <= 0)
                                backpack.inventory.Remove(resource);
                            UpdateContent(GetFusionInventory());
                        }
                    }
                    else if (command == "arrow+" && line.up.enabled)
                    {
                        if (storage.inventory[resource] <= 0 || !backpack.HasSpace())
                        {
                            if (audiosource)
                            {
                                audiosource.clip = noktransaction;
                                audiosource.Play();
                            }
                        }
                        else
                        {
                            if (audiosource)
                            {
                                audiosource.clip = oktransaction;
                                audiosource.Play();
                            }
                            storage.inventory[resource]--;
                            backpack.AddItem(resource, 1);
                            if (storage.inventory[resource] <= 0)
                                storage.inventory.Remove(resource);
                            UpdateContent(GetFusionInventory());
                        }
                    }
                    else if (command == "arrow++" && line.up2.enabled)
                    {
                        if (storage.inventory[resource] <= 0 || !backpack.HasSpace())
                        {
                            if (audiosource)
                            {
                                audiosource.clip = noktransaction;
                                audiosource.Play();
                            }
                        }
                        else
                        {
                            if (audiosource)
                            {
                                audiosource.clip = oktransaction;
                                audiosource.Play();
                            }
                            int maximumTransfert = Mathf.Min(backpack.capacity - backpack.load, storage.inventory[resource]);
                            backpack.AddItem(resource, storage.inventory[resource]);
                            storage.inventory[resource] -= maximumTransfert;
                            if (storage.inventory[resource] <= 0)
                                storage.inventory.Remove(resource);
                            UpdateContent(GetFusionInventory());
                        }
                    }

                    storage.RecomputeLoad();
                    player.RecomputeLoadFactor();
                }
            }
        }

        prevStorage = (storage != null);
    }
    
    public void UpdateContent(SortedDictionary<string, int> list)
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);

        if (backpackSlot.equipedItem.type != BackpackItem.Type.RessourceContainer || list.Count == 0)
        {
            loadSum.text = "empty or not\nequiped";
            loadSum.transform.localPosition = new Vector3(0, 0, 0);
            background.localScale = new Vector3(0, 0, 0);
        }
        else
        {
            loadSum.text = backpack.load.ToString() + "/" + backpack.capacity.ToString();

            int lines = list.Count;
            background.localScale = new Vector3(((storage != null) ? backgroundWidth.x : backgroundWidth.y), spacing * lines + 0.1f, 1);
            background.localPosition = new Vector3(0, 0.5f * spacing * lines + 0.05f, 0);
            loadSum.transform.localPosition = new Vector3(0, spacing * lines + 0.1f, 0);

            Vector3 position = Vector3.zero;
            foreach (KeyValuePair<string, int> entry in list)
            {
                InventoryLineTemplate go = Instantiate(template, container);
                go.transform.localPosition = position;
                go.name = entry.Key;
                go.gameObject.SetActive(true);

                go.count.text = entry.Value.ToString();
                go.icon.sprite = ResourceDictionary.Instance.Get(entry.Key).icon;

                position.y += spacing;
            }

            UpdateArrowVisibility();
        }
    }

    public SortedDictionary<string, int> GetFusionInventory()
    {
        SortedDictionary<string, int> extendedContainer = new SortedDictionary<string, int>();
        foreach (KeyValuePair<string, int> entry in backpack.inventory)
            extendedContainer.Add(entry.Key, entry.Value);
        foreach (KeyValuePair<string, int> entry in storage.inventory)
        {
            if (!extendedContainer.ContainsKey(entry.Key))
                extendedContainer.Add(entry.Key, 0);
        }
        
        return extendedContainer;
    }
    public void UpdateArrowVisibility()
    {
        foreach (Transform child in container)
        {
            InventoryLineTemplate line = child.GetComponent<InventoryLineTemplate>();
            line.up.enabled = (storage != null && storage.inventory.ContainsKey(line.gameObject.name));
            line.up2.enabled = (storage != null && storage.inventory.ContainsKey(line.gameObject.name));
            line.down.enabled = (storage != null && line.count.text != "0");
            line.down2.enabled = (storage != null && line.count.text != "0");
        }
    }
}
