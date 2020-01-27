using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteo : MonoBehaviour
{
    public Vector3 windBase;
    public float t = 0.0f;
    public float alpha1 = 20;
    public float alpha2 = 5;
    private int harmonic = 1;

    public bool snow = false;
    public bool leaves = true;
    private bool lastSnow;
    private bool lastLeaves;
    public List<TreeComponent> treesList = new List<TreeComponent>();

    // Singleton struct
    private static Meteo _instance;
    public static Meteo Instance { get { return _instance; } }
    
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

    // Start is called before the first frame update
    void Start()
    {
        lastSnow = snow;
        lastLeaves = leaves;
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;

        if(snow != lastSnow)
        {
            lastSnow = snow;
            foreach (TreeComponent tree in treesList)
                tree.snow.enabled = snow;
            if (snow) leaves = false;
        }
        if (leaves != lastLeaves)
        {
            lastLeaves = leaves;
            foreach (TreeComponent tree in treesList)
                if(tree.leaves != null)
                    foreach (SkinnedMeshRenderer leave in tree.leaves)
                        leave.enabled = leaves;
            if (leaves) snow = false;
        }
    }

    public Vector3 GetWind(Vector3 position)
    {
        return windBase * GetWave(position);
    }

    private float GetWave(Vector3 position)
    {
        float result = 0.0f;
        for (int i = 0; i < harmonic; i++)
        {
            result += Mathf.Sin(alpha1 * Vector3.Dot(windBase, position) + alpha2 * t);
        }
        return result;
    }
}
