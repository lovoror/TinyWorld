using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePrefabsContainer : MonoBehaviour
{
    public int diversity = 10;
    public Grass originalGrass;
    private Mesh[] grassMeshes;

    public Dirt originalDirt;
    private Mesh[] dirtMeshes;

    // Singleton struct
    private static TilePrefabsContainer _instance;
    public static TilePrefabsContainer Instance { get { return _instance; } }

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

        dirtMeshes = new Mesh[5 * diversity + 1];
        grassMeshes = new Mesh[9 * diversity];
        InitDirt();
        InitGrass();
    }

    private void InitDirt()
    {
        GameObject container = new GameObject();
        container.name = originalDirt.name + "_container";
        container.transform.parent = transform;
        container.transform.localPosition = Vector3.zero;
        container.transform.localScale = Vector3.one;
        container.transform.localRotation = Quaternion.identity;

        for (int j = 0; j < 6; j++)
        {
            for (int i = 0; i < diversity; i++)
            {
                GameObject go = Instantiate(originalDirt.gameObject);
                go.transform.parent = container.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;

                Dirt dirt = go.GetComponent<Dirt>();

                switch (j)
                {
                    case 0: dirt.Initialize(false, false, false, false, 0.3f); break;
                    case 1: dirt.Initialize(false, true, false, false, 0.3f); break;
                    case 2: dirt.Initialize(false, false, true, true, 0.3f); break;
                    case 3: dirt.Initialize(false, true, true, false, 0.3f); break;
                    case 4: dirt.Initialize(false, true, true, true, 0.3f); break;
                    case 5: dirt.Initialize(true, true, true, true, 0.3f); break;
                    default: break;
                }

                dirtMeshes[j * diversity + i] = go.GetComponent<MeshFilter>().sharedMesh;
                if (j == 5)
                    break;
            }
        }

        container.SetActive(false);
    }

    private void InitGrass()
    {
        GameObject container = new GameObject();
        container.name = originalGrass.name + "_container";
        container.transform.parent = transform;
        container.transform.localPosition = Vector3.zero;
        container.transform.localScale = Vector3.one;
        container.transform.localRotation = Quaternion.identity;

        for (int j = 0; j < 9; j++) 
        {
            for (int i = 0; i < diversity; i++)
            {
                GameObject go = Instantiate(originalGrass.gameObject);
                go.transform.parent = container.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;

                Grass grass = go.GetComponent<Grass>();
                grass.Initialize(j);
                grassMeshes[j * diversity + i] = grass.GetComponent<MeshFilter>().sharedMesh;
            }
        }

        container.SetActive(false);
    }

    public Mesh GetDirtA() { return dirtMeshes[5 * diversity]; }
    public Mesh GetDirtB() { return dirtMeshes[4 * diversity + Random.Range(0, diversity - 1)]; }
    public Mesh GetDirtC() { return dirtMeshes[2 * diversity + Random.Range(0, diversity - 1)]; }
    public Mesh GetDirtD() { return dirtMeshes[3 * diversity + Random.Range(0, diversity - 1)]; }
    public Mesh GetDirtE() { return dirtMeshes[1 * diversity + Random.Range(0, diversity - 1)]; }
    public Mesh GetDirtF() { return dirtMeshes[Random.Range(0, diversity - 1)]; }

    public Mesh GetGrass(int n) { return grassMeshes[n * diversity + Random.Range(0, diversity - 1)]; }
}
