using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConstructionCamera : MonoBehaviour
{
    [Header("Linking & entry variables")]
    public bool activated;
    public bool quit;
    public bool mouseControl = true;
    public KeyCode key;
    public CameraController trackballController;
    public GameObject constructionUI;
    public Map map;
    public GameObject currentObject;
    private MeshRenderer currentRenderer;
    public ConstructionTemplate currentTemplate;
    public Material buildingMaterial;
    public Material okMaterial;
    public Material nokMaterial;
    public AudioClip placementOk;

    [Header("Control parameters")]
    public float speed = 4f;
    public float scrollSpeed = 1f;
    public int borderThickness = 10;
    public float limit = 20f;
    public Vector2 distanceLimit;

    private float height = 30f;
    private bool lastActivated;
    private Vector3 entryPosition;
    private Quaternion entryRotation;

    [Header("UI linking")]
    public Transform iconContainer;
    public BuildingIconTemplate template;
    public Text helperText;
    public GameObject helperPivot;
    public UIHandler uihandler;
    public Transform directionHelper;

    [Header("Debug")]
    private EventSystem eventsystem;
    private RaycastHit[] scanResults = new RaycastHit[20];
    private bool orientation;

    private void Start()
    {
        lastActivated = !activated;
        eventsystem = (EventSystem)FindObjectOfType(typeof(EventSystem));
        if (eventsystem == null)
            Debug.LogError("No event system in scene");
        directionHelper.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        // standart stuff
        if (Input.GetKeyDown(key))
            activated = true;
        directionHelper.gameObject.SetActive(orientation);
        if (lastActivated != activated)
        {
            if(activated)
            {
                entryPosition = transform.position;
                entryRotation = transform.rotation;
                transform.position = new Vector3(trackballController.target.position.x, height, trackballController.target.position.z);
                currentObject = null;
                orientation = false;
            }
            trackballController.enabled = !activated;
            constructionUI.SetActive(activated);
        }
        if (!activated)
        {
            lastActivated = activated;
            return;
        }

        // position update
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.Z) || (mouseControl && Input.mousePosition.y >= Screen.height - borderThickness))
            direction = new Vector3(0, 0, 1);
        else if (Input.GetKey(KeyCode.S) || (mouseControl && Input.mousePosition.y <= borderThickness))
            direction = new Vector3(0, 0, -1);
        if (Input.GetKey(KeyCode.D) || (mouseControl && Input.mousePosition.x >= Screen.width - borderThickness))
            direction += new Vector3(1, 0, 0);
        else if (Input.GetKey(KeyCode.Q) || (mouseControl && Input.mousePosition.x <= borderThickness))
            direction += new Vector3(-1, 0, 0);
        direction.Normalize();
        
        Vector3 p = transform.position + speed * direction;
        p.x = Mathf.Clamp(p.x, trackballController.target.position.x - limit, trackballController.target.position.x + limit);
        p.z = Mathf.Clamp(p.z, trackballController.target.position.z - limit, trackballController.target.position.z + limit);

        if(!eventsystem.IsPointerOverGameObject())
            height = Mathf.Clamp(height - scrollSpeed * Input.GetAxis("Mouse ScrollWheel"), distanceLimit.x, distanceLimit.y);
        p.y = height;

        transform.position = p;
        transform.forward = -Vector3.up;

        // raycast
        if (!eventsystem.IsPointerOverGameObject())
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50f, 1 << LayerMask.NameToLayer("Ground")))
            {
                Vector3 pointing = map.grid.GetCellCenterWorld(map.grid.WorldToCell(hit.point));
                //ScriptableTile tile = map.tilemap.GetTile<ScriptableTile>(map.grid.WorldToCell(hit.point));
                if (currentObject)
                {
                    currentTemplate.meshFilter.sharedMesh = currentTemplate.preview;
                    directionHelper.localScale = Vector3.one;
                    if (currentTemplate.colliderSize.x > 4f || currentTemplate.colliderSize.y > 4f)
                    {
                        pointing += new Vector3(2f, 0, 2f);
                        directionHelper.localScale *= 1.5f;
                    }

                    p = new Vector3(pointing.x, 0, pointing.z);
                    if (!orientation)
                    {
                        currentObject.transform.position = p;
                        currentObject.transform.forward = Vector3.forward;
                    }
                    else
                    {
                        pointing = (p - currentObject.transform.position).normalized;
                        if (pointing.x > 0.7071f)
                            pointing = Vector3.forward;
                        else if (pointing.x < -0.7071f)
                            pointing = Vector3.back;
                        else if (pointing.z > 0.7071f)
                            pointing = Vector3.left;
                        else pointing = Vector3.right;

                        currentObject.transform.forward = pointing;
                        directionHelper.position = currentObject.transform.position;
                        directionHelper.rotation = currentObject.transform.rotation;
                    }

                    Vector3 s = 0.49f * new Vector3(currentTemplate.colliderSize.x, currentTemplate.colliderSize.z, currentTemplate.colliderSize.y);

                    int scan = Physics.BoxCastNonAlloc(currentObject.transform.position + new Vector3(0, s.y, 0), s, Vector3.up, 
                        scanResults, Quaternion.identity, 0.2f, 1 << LayerMask.NameToLayer("Default"));
                    currentRenderer.material = scan <= 1 ? okMaterial : nokMaterial;
                    
                    if (Input.GetMouseButtonDown(0))
                    {
                        if(orientation)
                        {
                            GameObject go = Instantiate(currentObject);
                            go.transform.parent = map.constructionContainer.transform;
                            go.transform.position = currentObject.transform.position;
                            go.transform.rotation = currentObject.transform.rotation;
                            go.transform.Find("mesh").GetComponent<MeshRenderer>().sharedMaterial = buildingMaterial;
                            orientation = false;

                            uihandler.audiosource.clip = placementOk;
                        }
                        else
                        {
                            if(scan <= 1)
                            {
                                orientation = true;
                                uihandler.audiosource.clip = uihandler.selectedSound;
                            }
                            else
                            {
                                uihandler.audiosource.clip = uihandler.nokSound;
                            }
                        }
                        uihandler.audiosource.Play();
                    }
                }
                
            }
        }

        // check if quit mode
        if (Input.GetKeyDown(KeyCode.Escape) || (Input.GetKeyDown(key) && lastActivated == activated) || quit)
        {
            trackballController.enabled = true;
            activated = false;
            transform.position = entryPosition;
            transform.rotation = entryRotation;
            constructionUI.SetActive(false);
            quit = false;
            orientation = false;
            if (currentObject)
                Destroy(currentObject);
        }
        lastActivated = activated;
    }

    public void SelectedBuilding(GameObject icon)
    {
        if (currentObject)
            Destroy(currentObject);
        currentObject = ConstructionDictionary.Instance.Get(icon.name);
        currentTemplate = currentObject.transform.Find("interactor").GetComponent<ConstructionTemplate>();
        currentRenderer = currentObject.transform.Find("mesh").GetComponent<MeshRenderer>();
        orientation = false;
    }
}
