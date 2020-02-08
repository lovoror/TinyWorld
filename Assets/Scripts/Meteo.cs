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

    public MeshFilter waterMesh;
    public int waterDiv = 10;
    Vector3[] vertices;
    public float amplitude = 0.2f;
    public float alpha3 = 1f;
    public float alpha4 = 1f;

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
        InitializeWater();
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
        
        // tree configuration update
        if (snow != lastSnow || leaves != lastLeaves)
        {
            lastSnow = snow;
            lastLeaves = leaves;

            foreach (TreeComponent tree in treesList)
            {
                if(tree)
                    tree.SetConfiguration(leaves, snow);
            }
        }

        // water mesh update
        for (int x = 0; x < waterDiv - 1; x++)
            for (int z = 0; z < waterDiv - 1; z++)
            {
                int index = x * waterDiv + z;
                vertices[index] = new Vector3(vertices[index].x, amplitude * Mathf.Sin(alpha3 * t + alpha4 * vertices[index].x * vertices[index].z), vertices[index].z);
            }
        for (int x = 0; x < waterDiv - 1; x++)
        {
            vertices[(waterDiv - 1) * waterDiv + x] = new Vector3(vertices[(waterDiv - 1) * waterDiv + x].x, vertices[x].y, vertices[(waterDiv - 1) * waterDiv + x].z);
            vertices[x * waterDiv + waterDiv - 1] = new Vector3(vertices[x * waterDiv + waterDiv - 1].x, vertices[x].y, vertices[x * waterDiv + waterDiv - 1].z);
        }
        vertices[vertices.Length - 1] = new Vector3(vertices[vertices.Length - 1].x, vertices[0].y, vertices[vertices.Length - 1].z);
        vertices[waterDiv - 1] = new Vector3(vertices[waterDiv - 1].x, vertices[0].y, vertices[waterDiv - 1].z);
        vertices[(waterDiv - 1) * waterDiv] = new Vector3(vertices[(waterDiv - 1) * waterDiv].x, vertices[0].y, vertices[(waterDiv - 1) * waterDiv].z);



        waterMesh.sharedMesh.vertices = vertices;
        waterMesh.sharedMesh.RecalculateNormals();
        waterMesh.sharedMesh.RecalculateBounds();

        Vector3[] normals = waterMesh.sharedMesh.normals;
        for (int i = 0; i < waterDiv; i++)
        {
            normals[i * waterDiv] = new Vector3(normals[i * waterDiv].x, normals[i * waterDiv].y, 0);
            normals[i * waterDiv + (waterDiv - 1)] = new Vector3(normals[i * waterDiv].x, normals[i * waterDiv].y, 0);
            normals[i] = new Vector3(0, normals[i].y, normals[i].z);
            normals[(waterDiv - 1) * waterDiv + i] = new Vector3(0, normals[i].y, normals[i].z);
        }
        waterMesh.sharedMesh.normals = normals;
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

    protected void InitializeWater()
    {
        List<Vector3> verticesL = new List<Vector3>();
        for (int x=0; x< waterDiv; x++)
            for (int z = 0; z < waterDiv; z++)
            {
                verticesL.Add(new Vector3(-2, 0, -2) + 4f / (waterDiv-1) * new Vector3(x, 0, z));
            }

        List<int> triangles = new List<int>();
        for (int x = 0; x < waterDiv - 1; x++)
            for (int z = 0; z < waterDiv - 1; z++)
            {
                triangles.Add(x * waterDiv + z);
                triangles.Add(x * waterDiv + z + 1);
                triangles.Add((x + 1) * waterDiv + z + 1);

                triangles.Add(x * waterDiv + z);
                triangles.Add((x + 1) * waterDiv + z + 1);
                triangles.Add((x + 1) * waterDiv + z);
            }

        Mesh mesh = new Mesh();
        mesh.name = "water_mesh";
        mesh.vertices = verticesL.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        vertices = mesh.vertices;
        waterMesh.sharedMesh = mesh;
    }
}
