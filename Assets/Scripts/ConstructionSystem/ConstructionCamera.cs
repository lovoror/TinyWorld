﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

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
    //public GameObject helperPivot;
    public UIHandler uihandler;
    public ConstructionHelper helper;

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
        helper.transform.parent = null;
    }

    // Update is called once per frame
    private void Update()
    {
        // standart stuff
        if (Input.GetKeyDown(key))
            activated = true;
        helper.gameObject.SetActive(orientation || uihandler.toolName == "delete");
        if (lastActivated != activated)
        {
            if (activated)
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

        if (!eventsystem.IsPointerOverGameObject())
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
                
                if (uihandler.toolName == "building")
                {
                    if(currentObject)
                    {
                        currentTemplate.meshFilter.sharedMesh = currentTemplate.preview;
                        helper.transform.localScale = Vector3.one;
                        Vector3 s = currentTemplate.colliderSize;
                        if (s.x > 4f || s.y > 4f)
                        {
                            pointing += new Vector3(2f, 0, 2f);
                            helper.transform.localScale *= 1.5f;
                        }

                        p = new Vector3(pointing.x, 0, pointing.z);
                        if (!orientation)
                        {
                            currentObject.transform.position = p;
                            currentObject.transform.forward = Vector3.forward;
                            helper.mode = 0;
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
                            helper.mode = 1;
                            helper.transform.position = currentObject.transform.position;
                            helper.transform.rotation = currentObject.transform.rotation;
                        }

                        int scan = Physics.BoxCastNonAlloc(currentObject.transform.position + new Vector3(0, 0.49f * s.z, 0), 0.49f * new Vector3(s.x, s.z, s.y), Vector3.up,
                            scanResults, Quaternion.identity, 0.2f, 1 << LayerMask.NameToLayer("Default"));
                        currentRenderer.material = scan <= 1 ? okMaterial : nokMaterial;

                        if (Input.GetMouseButtonDown(0))
                        {
                            if (orientation)
                            {
                                GameObject go = Instantiate(currentObject);
                                go.transform.parent = map.buildingsContainer.transform;
                                go.transform.position = currentObject.transform.position;
                                go.transform.rotation = currentObject.transform.rotation;
                                go.transform.Find("mesh").GetComponent<MeshRenderer>().sharedMaterial = buildingMaterial;
                                orientation = false;

                                uihandler.audiosource.clip = placementOk;
                            }
                            else
                            {
                                if (scan <= 1)
                                {
                                    orientation = true;
                                    uihandler.audiosource.clip = uihandler.selectedSound;
                                }
                                else
                                    uihandler.audiosource.clip = uihandler.nokSound;
                            }
                            uihandler.audiosource.Play();
                        }
                    }
                }
                else if(uihandler.toolName == "delete")
                {
                    if (currentObject)
                        Destroy(currentObject);
                    helper.mode = 2;
                    helper.transform.rotation = Quaternion.identity;
                    List<GameObject> buildings = map.SearchBuildingsGameObject(pointing, 3.3f);

                    if (buildings.Count != 0)
                    {
                        helper.transform.position = buildings[0].transform.position;

                        Transform mesh = buildings[0].transform.Find("mesh");
                        if (!mesh)
                            mesh = buildings[0].transform;
                        Vector3 s = mesh.GetComponent<Collider>().bounds.size;

                        if (s.x > 4f || s.z > 4f)
                            helper.transform.localScale = new Vector3(8f, 3f, 8f);
                        else helper.transform.localScale = new Vector3(4f, 3f, 4f);
                    }
                    else
                    {
                        helper.transform.position = pointing;
                        helper.transform.localScale = new Vector3(4f, 3f, 4f);
                    }

                    if (Input.GetMouseButtonDown(0) && buildings.Count != 0)
                    {
                        uihandler.audiosource.clip = placementOk;
                        uihandler.audiosource.Play();
                        
                        if(buildings[0].name.Contains("Wall"))
                        {
                            List<Vector3> positions = new List<Vector3>();
                            positions.Add(pointing);
                            Map.Instance.PlaceTiles(positions, map.SearchTilesGameObject(pointing, 0.5f), "Grass");
                        }
                        Destroy(buildings[0]);
                    }
                    else if (Input.GetMouseButtonDown(0))
                    {
                        uihandler.audiosource.clip = uihandler.nokSound;
                        uihandler.audiosource.Play();
                    }
                }
                else if (uihandler.toolName == "terrain")
                {
                    Vector3Int cellPosition = map.GetCellFromWorld(pointing);
                    ScriptableTile tile = map.tilemap.GetTile<ScriptableTile>(cellPosition);

                    if (tile)
                    {
                        Debug.Log("Terrain brush not yet implemented");
                    }
                }
                else if (uihandler.toolName.Length != 0)
                    Debug.LogWarning("Construction mode tool " + uihandler.toolName + " unknown");
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
            uihandler.toolName = "";
        }
        lastActivated = activated;
    }
    void Update2()
    {
        // standart stuff
        if (Input.GetKeyDown(key))
            activated = true;
        helper.gameObject.SetActive(orientation || uihandler.toolName == "delete");
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

                if (uihandler.toolName == "building")
                {
                    if (currentObject)
                    {
                        currentTemplate.meshFilter.sharedMesh = currentTemplate.preview;
                        helper.transform.localScale = Vector3.one;
                        if (currentTemplate.colliderSize.x > 4f || currentTemplate.colliderSize.y > 4f)
                        {
                            pointing += new Vector3(2f, 0, 2f);
                            helper.transform.localScale *= 1.5f;
                        }

                        p = new Vector3(pointing.x, 0, pointing.z);
                        if (!orientation)
                        {
                            currentObject.transform.position = p;
                            currentObject.transform.forward = Vector3.forward;
                            helper.mode = 0;
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
                            helper.mode = 1;
                            helper.transform.position = currentObject.transform.position;
                            helper.transform.rotation = currentObject.transform.rotation;
                        }

                        Vector3 s = 0.49f * new Vector3(currentTemplate.colliderSize.x, currentTemplate.colliderSize.z, currentTemplate.colliderSize.y);

                        int scan = Physics.BoxCastNonAlloc(currentObject.transform.position + new Vector3(0, s.y, 0), s, Vector3.up,
                            scanResults, Quaternion.identity, 0.2f, 1 << LayerMask.NameToLayer("Default"));
                        currentRenderer.material = scan <= 1 ? okMaterial : nokMaterial;

                        if (Input.GetMouseButtonDown(0))
                        {
                            if (orientation)
                            {
                                GameObject go = Instantiate(currentObject);
                                go.transform.parent = map.buildingsContainer.transform;
                                go.transform.position = currentObject.transform.position;
                                go.transform.rotation = currentObject.transform.rotation;
                                go.transform.Find("mesh").GetComponent<MeshRenderer>().sharedMaterial = buildingMaterial;
                                orientation = false;

                                uihandler.audiosource.clip = placementOk;
                            }
                            else
                            {
                                if (scan <= 1)
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



                else if (uihandler.toolName == "delete")
                {
                    if (currentObject)
                        Destroy(currentObject);
                    helper.mode = 2;
                    helper.transform.rotation = Quaternion.identity;

                    Vector3Int c = map.tilemap.WorldToCell(pointing);
                    Vector3Int cell = new Vector3Int(c.x, c.y, (int)map.tilemap.transform.position.z);
                    ScriptableTile tile = map.tilemap.GetTile<ScriptableTile>(cell);

                    if(tile && (tile.name == "House" || tile.name == "Granary"))
                    {
                        helper.transform.position = pointing;
                        List<GameObject> buildings = map.SearchTilesGameObject(pointing, 3f);

                        if (Input.GetMouseButtonDown(0) && buildings.Count != 0)
                        {
                            uihandler.audiosource.clip = placementOk;
                            uihandler.audiosource.Play();
                            List<Vector3> positions = new List<Vector3>();
                            positions.Add(map.tilemap.CellToWorld(cell));
                            map.PlaceTiles(positions, buildings, "Grass");
                        }
                        else if (Input.GetMouseButtonDown(0))
                        {
                            uihandler.audiosource.clip = uihandler.nokSound;
                            uihandler.audiosource.Play();
                        }
                    }
                    else
                    {
                        List<GameObject> buildings = map.SearchBuildingsGameObject(pointing, 3f);

                        if (buildings.Count != 0)
                        {
                            helper.transform.position = buildings[0].transform.position;

                            Transform mesh = buildings[0].transform.Find("mesh");
                            if (!mesh)
                                mesh = buildings[0].transform;
                            Vector3 s = mesh.GetComponent<Collider>().bounds.size;

                            if (s.x > 4f || s.z > 4f)
                                helper.transform.localScale = new Vector3(8f, 3f, 8f);
                            else helper.transform.localScale = new Vector3(4f, 3f, 4f);
                        }
                        else
                        {
                            helper.transform.position = pointing;
                            helper.transform.localScale = new Vector3(4f, 3f, 4f);
                        }

                        if (Input.GetMouseButtonDown(0) && buildings.Count != 0)
                        {
                            uihandler.audiosource.clip = placementOk;
                            uihandler.audiosource.Play();
                            Destroy(buildings[0]);
                        }
                        else if (Input.GetMouseButtonDown(0))
                        {
                            uihandler.audiosource.clip = uihandler.nokSound;
                            uihandler.audiosource.Play();
                        }
                    }
                }



                else if (uihandler.toolName == "terrain")
                {
                    Vector3Int c = map.tilemap.WorldToCell(pointing);
                    Vector3Int cell = new Vector3Int(c.x, c.y, (int)map.tilemap.transform.position.z);
                    ScriptableTile tile = map.tilemap.GetTile<ScriptableTile>(cell);

                    if (tile)
                    {

                    }
                }
                else if(uihandler.toolName.Length != 0)
                    Debug.LogWarning("Construction mode tool " + uihandler.toolName + " unknown");
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
            uihandler.toolName = "";
        }
        lastActivated = activated;
    }

    public void SelectedBuilding(GameObject icon)
    {
        if(uihandler.toolName == "building")
        {
            if (currentObject)
                Destroy(currentObject);
            currentObject = ConstructionDictionary.Instance.Get(icon.name);
            currentTemplate = currentObject.transform.Find("interactor").GetComponent<ConstructionTemplate>();
            currentRenderer = currentObject.transform.Find("mesh").GetComponent<MeshRenderer>();
            orientation = false;
        }
        else if(uihandler.toolName == "terrain")
        {
            if (currentObject)
                Destroy(currentObject);
            orientation = false;
            Debug.LogWarning("Terrain brush selection not yet implemented");
        }
        else Debug.LogWarning("Construction mode tool (" + uihandler.toolName + ") unknown for brush (" + icon.name + ")");
    }
}
