using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreHouseViewer : MonoBehaviour
{
    public bool visible = false;
    public float spacing;
    public List<Sprite> ressourceIcons;
    public InventoryLineTemplate template;
    public GameObject pivot;
    public Transform containerTransform;
    public RessourceContainer container;
    public TextMesh loadSum;
    public Transform background;
    private Dictionary<string, Sprite> icons = new Dictionary<string, Sprite>();

    void Start()
    {
        foreach (Sprite s in ressourceIcons)
        {
            string name = s.name.Substring(0, s.name.IndexOf("Icon"));
            icons.Add(name, s);
        }
    }

    void UpdateContent(bool isVisible)
    {
        visible = isVisible;
        if (!isVisible)
        {
            pivot.SetActive(false);
            foreach (Transform child in containerTransform)
                Destroy(child.gameObject);
        }
        else if (container.load == 0)
        {
            pivot.SetActive(true);
            loadSum.text = "empty or not\nequiped";
            loadSum.transform.localPosition = new Vector3(0, 0, 0);
            background.localScale = new Vector3(0, 0, 0);
        }
        else
        {
            pivot.SetActive(true);

            foreach (Transform child in containerTransform)
                Destroy(child.gameObject);

            loadSum.text = container.load.ToString() + "/" + container.capacity.ToString();

            int lines = container.inventory.Count;
            background.localScale = new Vector3(1, spacing * lines + 0.1f, 1);
            background.localPosition = new Vector3(0, 0.5f * spacing * lines + 0.05f, 0);
            loadSum.transform.localPosition = new Vector3(0, spacing * lines + 0.1f, 0);

            Vector3 position = Vector3.zero;
            foreach (KeyValuePair<string, int> entry in container.inventory)
            {
                InventoryLineTemplate go = Instantiate(template, containerTransform);
                go.transform.localPosition = position;
                go.gameObject.SetActive(true);

                go.count.text = entry.Value.ToString();
                go.icon.sprite = icons[entry.Key];

                position.y += spacing;
            }
        }
    }
}
