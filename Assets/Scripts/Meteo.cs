using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteo : MonoBehaviour
{
    public Vector3 windBase;
    public GameObject prefab;
    public int size = 20;
    public float noiseMagnitude = 0.0f;
    public float t = 0.0f;
    public float alpha1 = 20;
    public float alpha2 = 5;
    public float alpha3 = 0.7f;

    private int harmonic = 1;

    // Start is called before the first frame update
    void Start()
    {
        prefab.SetActive(false);
        for(int i=-size; i< size + 1; i++)
            for (int j = -size; j < size + 1; j++)
            {
                GameObject go = Instantiate(prefab);
                go.SetActive(true);
                go.transform.localPosition = new Vector3(4 * i, 0, 4 * j);
                Transform tree = go.transform.Find("tree");
                tree.localPosition = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                float scale = Random.Range(0.5f, 1.3f);
                tree.localScale = new Vector3(scale, scale, scale);
            }
    }

    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime;
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
