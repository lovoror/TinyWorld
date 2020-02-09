using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheet : MonoBehaviour
{
    public float width = 0.05f;
    public float height = 0.7f;

    public int density;
    static float dispersion = 0.3f;

    private void Start()
    {
        CombineInstance[] combine = new CombineInstance[density * density];
        float delta = 4f / density;
        for (int i = 0; i < density; i++)
            for (int j = 0; j < density; j++)
            {
                Vector3 dp = new Vector3(Random.Range(-dispersion, dispersion), 0, Random.Range(-dispersion, dispersion));
                Vector3 ds = new Vector3(0, 0, Random.Range(-dispersion, dispersion));
                combine[i * density + j].mesh = GetStringA(width, height + Random.Range(-0.3f, 0.3f));
                combine[i * density + j].transform = Matrix4x4.TRS(new Vector3(i * delta - 2f + 0.5f * dispersion, 0, j * delta - 2f + 0.5f * dispersion) + dp, Quaternion.Euler(0, Random.Range(0, 180), 0), Vector3.one + ds);
            }
        GetComponent<MeshFilter>().mesh = new Mesh();
        GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        GetComponent<MeshFilter>().mesh.RecalculateBounds();
    }

    private Mesh GetStringA(float w, float h)
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.Add(new Vector3(-w, 0, 0));
        vertices.Add(new Vector3(w, 0, 0));
        vertices.Add(new Vector3(-0.8f * w, 0.3f * h, 0));
        vertices.Add(new Vector3(0.8f * w, 0.3f * h, 0));
        vertices.Add(new Vector3(-0.8f * w, 0.6f * h, 0));
        vertices.Add(new Vector3(0.8f * w, 0.6f * h, 0));
        vertices.Add(new Vector3(-0.8f * w, h, 0));
        vertices.Add(new Vector3(0.8f * w, h, 0));
        
        triangles.Add(0); triangles.Add(2); triangles.Add(1);
        triangles.Add(2); triangles.Add(3); triangles.Add(1);
        triangles.Add(3); triangles.Add(2); triangles.Add(4);
        triangles.Add(4); triangles.Add(5); triangles.Add(3);
        triangles.Add(4); triangles.Add(6); triangles.Add(5);
        triangles.Add(5); triangles.Add(6); triangles.Add(7);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        MakeDoubleFaced(mesh);
        return mesh;
    }
    private void MakeDoubleFaced(Mesh mesh)
    {
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var szV = vertices.Length;
        var newVerts = new Vector3[szV * 2];
        var newUv = new Vector2[szV * 2];
        var newNorms = new Vector3[szV * 2];

        for (var j = 0; j < szV; j++)
        {
            newVerts[j] = newVerts[j + szV] = vertices[j];
            newNorms[j] = normals[j];
            newNorms[j + szV] = -normals[j];
        }

        var triangles = mesh.triangles;
        var szT = triangles.Length;
        var newTris = new int[szT * 2];

        for (var i = 0; i < szT; i += 3)
        {
            newTris[i] = triangles[i];
            newTris[i + 1] = triangles[i + 1];
            newTris[i + 2] = triangles[i + 2];

            int j = i + szT;
            newTris[j] = triangles[i] + szV;
            newTris[j + 2] = triangles[i + 1] + szV;
            newTris[j + 1] = triangles[i + 2] + szV;
        }

        mesh.vertices = newVerts;
        mesh.normals = newNorms;
        mesh.triangles = newTris;

        mesh.RecalculateBounds();
    }
}
