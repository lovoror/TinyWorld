using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteo : MonoBehaviour
{
    public Vector3 windBase;
    //public GameObject prefab;
    //public int sizeZ = 10;
    //public int sizeX = 20;
    //public float noiseMagnitude = 0.0f;
    public float t = 0.0f;
    public float alpha1 = 20;
    public float alpha2 = 5;
    //public float alpha3 = 0.7f;

    private int harmonic = 1;

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
        /*prefab.SetActive(false);
        for(int i=-sizeX; i< sizeX + 1; i++)
            for (int j = -sizeZ; j < sizeZ + 1; j++)
            {
                GameObject go = Instantiate(prefab);
                go.SetActive(true);
                go.transform.localPosition = new Vector3(4 * i, 0, 4 * j);
                Transform tree = go.transform.Find("Tree");
                tree.localPosition = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                tree.localEulerAngles = new Vector3(0, Random.Range(-180f, 180f), 0);
                float scale = Random.Range(0.7f, 1.3f);
                tree.localScale = new Vector3(scale, scale, scale);
                trees.Add(tree.GetComponent<TreeComponent>());
            }
        lastSnow = snow;
        lastLeaves = leaves;*/
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;

        /*if(snow != lastSnow)
        {
            lastSnow = snow;
            foreach (TreeComponent tree in trees)
                tree.snow.enabled = snow;
            if (snow) leaves = false;
        }
        if (leaves != lastLeaves)
        {
            lastLeaves = leaves;
            foreach (TreeComponent tree in trees)
                if(tree.leaves != null)
                    foreach (SkinnedMeshRenderer leave in tree.leaves)
                        leave.enabled = leaves;
            if (leaves) snow = false;
        }*/
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
