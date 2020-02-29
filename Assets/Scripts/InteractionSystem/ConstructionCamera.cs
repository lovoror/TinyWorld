using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionCamera : MonoBehaviour
{
    [Header("Linking & entry variables")]
    public bool activated;
    public bool mouseControl = true;
    public KeyCode key;
    public CameraController trackballController;
    public GameObject constructionUI;

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

    private void Start()
    {
        lastActivated = !activated;
    }

    // Update is called once per frame
    void Update()
    {
        // standart stuff
        if (lastActivated != activated)
        {
            if(activated)
            {
                entryPosition = transform.position;
                entryRotation = transform.rotation;
                transform.position = new Vector3(trackballController.target.position.x, height, trackballController.target.position.z);
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
        height = Mathf.Clamp(height - scrollSpeed * Input.GetAxis("Mouse ScrollWheel"), distanceLimit.x, distanceLimit.y);
        p.y = height;
        transform.position = p;
        transform.forward = -Vector3.up;

        // raycast

        // check if quit mode
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(key))
        {
            trackballController.enabled = true;
            activated = false;
            transform.position = entryPosition;
            transform.rotation = entryRotation;
        }
        lastActivated = activated;
    }
}
