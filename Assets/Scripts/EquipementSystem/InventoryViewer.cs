using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryViewer : MonoBehaviour
{
    public bool visible = false;
    public bool storage = false;
    public float spacing;
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

    private bool callback = false;
    private string callbackCommand = "";
    private string callbackResource = "";

    void Start()
    {
        callback = false;
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

        if (visible)
        {
            foreach (Transform child in container)
            {
                InventoryLineTemplate line = child.GetComponent<InventoryLineTemplate>();
                line.up.enabled = storage;
                line.up2.enabled = storage;
                line.down.enabled = storage;
                line.down2.enabled = storage;
            }

            Vector3 s = background.localScale;
            background.localScale = new Vector3((storage ? backgroundWidth.x : backgroundWidth.y), s.y, s.z);

            //if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                string command = "";
                string resource = "";
                bool collision = false;
                foreach (Transform child in container)
                {
                    InventoryLineTemplate line = child.GetComponent<InventoryLineTemplate>();
                    foreach(Collider box in line.buttons)
                    {
                        /*Ray ray2 = new Ray();
                        ray2.origin = box.transform.InverseTransformPoint(ray.origin);
                        ray2.direction = box.transform.InverseTransformDirection(ray.direction);
                        if (box.bounds.IntersectRay(ray2))
                        {
                            collision = true;
                            command = box.gameObject.name;
                            resource = box.transform.parent.name;
                            break;
                        }
                        if(RayVsObb(ray.origin, ray.direction, box.transform.worldToLocalMatrix, box.bounds.min, box.bounds.max))
                        {
                            collision = true;
                            command = box.gameObject.name;
                            resource = box.transform.parent.name;
                            break;
                        }*/

                        Debug.Log(box.bounds.center.ToString() + " " + box.bounds.extents.ToString());
                    }
                    if (collision)
                        break;
                }

                if(collision)
                {
                    Debug.Log(resource + " " + command);
                }


                /*RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                //RaycastHit[] hit = Physics.RaycastAll(ray, 50f);

                if (Physics.Raycast(ray, out hit, 50f, 1 << LayerMask.NameToLayer("PlayerUI"), QueryTriggerInteraction.Collide))
                {
                    string command = hit.collider.gameObject.name;
                    string resource = hit.collider.transform.parent.name;

                    Debug.Log(resource + " " + command);
                }*/

                    /*for (int i=0; i< hit.Length;i++)
                    {
                        string command = hit[i].collider.gameObject.name;
                        string resource = hit[i].collider.transform.parent.name;

                        Debug.Log(resource + " " + command);
                    }*/
                    //Debug.Log(hit.Length);
            }
        }
    }
    public void OnDrawGizmos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Gizmos.DrawRay(ray.origin, 50f * ray.direction);
    }

    public void UpdateContent()
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);

        if (backpackSlot.equipedItem.type != BackpackItem.Type.RessourceContainer || backpack.load == 0)
        {
            loadSum.text = "empty or not\nequiped";
            loadSum.transform.localPosition = new Vector3(0, 0, 0);
            background.localScale = new Vector3(0, 0, 0);
        }
        else
        {
            loadSum.text = backpack.load.ToString() + "/" + backpack.capacity.ToString();

            int lines = backpack.inventory.Count;
            background.localScale = new Vector3((storage ? backgroundWidth.x : backgroundWidth.y), spacing * lines + 0.1f, 1);
            background.localPosition = new Vector3(0, 0.5f * spacing * lines + 0.05f, 0);
            loadSum.transform.localPosition = new Vector3(0, spacing * lines + 0.1f, 0);

            Vector3 position = Vector3.zero;
            foreach (KeyValuePair<string, int> entry in backpack.inventory)
            {
                InventoryLineTemplate go = Instantiate(template, container);
                go.transform.localPosition = position;
                go.name = entry.Key;
                go.up.enabled = storage;
                go.up2.enabled = storage;
                go.down.enabled = storage;
                go.down2.enabled = storage;
                go.gameObject.SetActive(true);

                go.count.text = entry.Value.ToString();
                go.icon.sprite = ResourceDictionary.Instance.Get(entry.Key).icon;

                position.y += spacing;
            }
        }
    }
    public void ButtonCallback(string name1, string name2)
    {
        callback = true;
        callbackCommand = name1;
        callbackResource = name2;
        Debug.Log(callbackCommand + " " + callbackResource);
    }
    private bool RayVsObb(Vector3 origin, Vector3 direction, Matrix4x4 boxTranform, Vector3 boxMin, Vector3 boxMax)
    {
        //http://www.opengl-tutorial.org/fr/miscellaneous/clicking-on-objects/picking-with-custom-ray-obb-function/

        Vector4 tmp = boxTranform.GetColumn(3);
        Vector3 delta = new Vector3(tmp.x, tmp.y, tmp.z) - origin;

        float tmin = 0f;
        float tmax = Mathf.Infinity;

        //	test on the two axis of local x
        tmp = boxTranform.GetColumn(0);
        Vector3 bx = new Vector3(tmp.x, tmp.y, tmp.z).normalized;
	    float e = Vector3.Dot(bx, delta);
	    if (Vector3.Dot(bx, direction) == 0f) // segment parallel to selected plane
	    {
		    if (-e + boxMin.x > 0.0f || -e + boxMax.x< 0.0f)
                return false;
	    }
	    else
	    {
		    float t1 = (e + boxMin.x) / Vector3.Dot(bx, direction); // Intersection with the "left" plane
            float t2 = (e + boxMax.x) / Vector3.Dot(bx, direction); // Intersection with the "right" plane
		    if (t1 > t2)
            {
                float f = t1;
                t1 = t2;
                t2 = f;
            }

            Debug.Log("x " + bx.ToString() + " " + t2.ToString());

		    if (t2 < tmax) tmax = t2;
		    if (t1 > tmin) tmin = t1;
		    if (tmax < tmin)
                return false;
	    }

        //	test on the two axis of local y
        tmp = boxTranform.GetColumn(1);
        Vector3 by = new Vector3(tmp.x, tmp.y, tmp.z).normalized;
        e = Vector3.Dot(by, delta);
	    if (Vector3.Dot(by, direction) == 0f) // segment parallel to selected plane
	    {
		    if (-e + boxMin.y > 0.0f || -e + boxMax.y< 0.0f)
                return false;
	    }
	    else
	    {
		    float t1 = (e + boxMin.y) / Vector3.Dot(by, direction); // Intersection with the "left" plane
            float t2 = (e + boxMax.y) / Vector3.Dot(by, direction); // Intersection with the "right" plane
		    if (t1 > t2)
            {
                float f = t1;
                t1 = t2;
                t2 = f;
            }

            Debug.Log("y " + t1.ToString() + " " + t2.ToString());

            if (t2 < tmax) tmax = t2;
		    if (t1 > tmin) tmin = t1;
		    if (tmax < tmin)
                return false;
	    }

        //	test on the two axis of local z
        tmp = boxTranform.GetColumn(2);
        Vector3 bz = new Vector3(tmp.x, tmp.y, tmp.z).normalized;
        e = Vector3.Dot(bz, delta);
	    if (Vector3.Dot(bz, direction) == 0f) // segment parallel to selected plane
	    {
		    if (-e + boxMin.z > 0.0f || -e + boxMax.z< 0.0f)
                return false;
	    }
	    else
	    {
		    float t1 = (e + boxMin.z) / Vector3.Dot(bz, direction); // Intersection with the "left" plane
            float t2 = (e + boxMax.z) / Vector3.Dot(bz, direction); // Intersection with the "right" plane
		    if (t1 > t2)
            {
                float f = t1;
                t1 = t2;
                t2 = f;
            }

            Debug.Log("z " + t1.ToString() + " " + t2.ToString());

            if (t2 < tmax) tmax = t2;
		    if (t1 > tmin) tmin = t1;
		    if (tmax < tmin)
                return false;
	    }
	    return true;
    }
}
