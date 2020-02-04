using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : Dirt
{
    new protected Mesh CaseA(float borderStrengh)
    {
        // creates arrays
        Vector3[] vertices = new Vector3[4] { v0, v1, v2, v3 };
        Vector3[] normals = new Vector3[4] { n, n, n, n };
        Vector2[] uv = new Vector2[4] { uv0, uv1, uv2, uv3 };
        int[] tris = new int[6] { 0, 1, 3, 1, 2, 3 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.normals = normals;
        mesh.uv = uv;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.materials = new Material[] { mr.materials[0] };
        return mesh;
    }
    new protected Mesh CaseB(float borderStrengh)
    {
        // creates sub vertices
        Vector2 sub = Vector2.Lerp(new Vector2(0.5f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = v0 + sub.x * (v1 - v0) + sub.y * (v3 - v0);
        Vector2 uv4 = uv0 + sub.x * (uv1 - uv0) + sub.y * (uv3 - uv0);

        // creates arrays
        Vector3[] vertices = new Vector3[5] { v0, v1, v2, v3, v4 };
        Vector3[] normals = new Vector3[5] { n, n, n, n, n };
        Vector2[] uv = new Vector2[5] { uv0, uv1, uv2, uv3, uv4 };
        int[] dirttri = new int[9] { 0, 4, 3, 4, 1, 3, 1, 2, 3 };
        int[] grasstri = new int[3] { 0, 1, 4 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.RecalculateNormals();
        return mesh;
    }
    new protected Mesh CaseC(float borderStrengh)
    {
        // creates sub vertices
        Vector2 sub = Vector2.Lerp(new Vector2(0.5f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = v0 + sub.x * (v1 - v0) + sub.y * (v3 - v0);
        Vector2 uv4 = uv0 + sub.x * (uv1 - uv0) + sub.y * (uv3 - uv0);
        sub = Vector2.Lerp(new Vector2(0f, 0.5f), GetBarycenticCoord(), borderStrengh);
        Vector3 v5 = v2 + sub.x * (v1 - v2) + sub.y * (v3 - v2);
        Vector2 uv5 = uv2 + sub.x * (uv1 - uv2) + sub.y * (uv3 - uv2);

        // creates arrays
        Vector3[] vertices = new Vector3[6] { v0, v1, v2, v3, v4, v5 };
        Vector3[] normals = new Vector3[6] { n, n, n, n, n, n };
        Vector2[] uv = new Vector2[6] { uv0, uv1, uv2, uv3, uv4, uv5 };
        int[] dirttri = new int[12] { 0, 4, 3, 3, 4, 1, 1, 5, 3, 1, 2, 5 };
        int[] grasstri = new int[6] { 0, 1, 4, 5, 2, 3 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.RecalculateNormals();
        return mesh;
    }
    new protected Mesh CaseD(float borderStrengh)
    {
        // creates sub vertices
        Vector2 sub = Vector2.Lerp(new Vector2(0.3f, 0.3f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = v1 + sub.x * (v0 - v1) + sub.y * (v2 - v1);
        Vector2 uv4 = uv1 + sub.x * (uv0 - uv1) + sub.y * (uv2 - uv1);

        // creates arrays
        Vector3[] vertices = new Vector3[5] { v0, v1, v2, v3, v4 };
        Vector3[] normals = new Vector3[5] { n, n, n, n, n };
        Vector2[] uv = new Vector2[5] { uv0, uv1, uv2, uv3, uv4 };
        int[] dirttri = new int[6] { 0, 4, 2, 0, 2, 3 };
        int[] grasstri = new int[6] { 0, 1, 4, 1, 2, 4 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.RecalculateNormals();
        return mesh;
    }
    new protected Mesh CaseE(float borderStrengh)
    {
        // creates sub vertices
        Vector2 sub = Vector2.Lerp(new Vector2(0f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v4 = sub.x * v0 + sub.y * v3;
        Vector2 uv4 = new Vector2(0.5f, 0.5f) + sub.x * uv0 + sub.y * uv3;
        sub = Vector2.Lerp(new Vector2(0f, 0f), GetBarycenticCoord(), borderStrengh);
        Vector3 v5 = sub.x * v1 + sub.y * v2;
        Vector2 uv5 = new Vector2(0.5f, 0.5f) + sub.x * uv1 + sub.y * uv2;

        // creates arrays
        Vector3[] vertices = new Vector3[6] { v0, v1, v2, v3, v4, v5 };
        Vector3[] normals = new Vector3[6] { n, n, n, n, n, n };
        Vector2[] uv = new Vector2[6] { uv0, uv1, uv2, uv3, uv4, uv5 };
        int[] dirttri = new int[6] { 4, 5, 3, 3, 5, 2 };
        int[] grasstri = new int[12] { 0, 4, 3, 0, 1, 4, 4, 1, 5, 5, 1, 2 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.RecalculateNormals();
        return mesh;
    }
    new protected Mesh CaseF(float borderStrengh)
    {
        // creates sub vertices
        float alpha = Mathf.Lerp(0.5f, Random.Range(0.1f, 0.9f), borderStrengh);
        float beta = Mathf.Lerp(0.5f, Random.Range(0.1f, 0.9f), borderStrengh);
        float gamma = Mathf.Lerp(0.5f, Random.Range(0.1f, 0.9f), borderStrengh);
        float delta = Mathf.Lerp(0.5f, Random.Range(0.1f, 0.9f), borderStrengh);
        Vector3 v4 = alpha * v0; Vector2 uv4 = alpha * uv0;
        Vector3 v5 = beta * v1; Vector2 uv5 = beta * uv1;
        Vector3 v6 = gamma * v2; Vector2 uv6 = gamma * uv2;
        Vector3 v7 = delta * v3; Vector2 uv7 = delta * uv3;

        // creates arrays
        Vector3[] vertices = new Vector3[8] { v0, v1, v2, v3, v4, v5, v6, v7 };
        Vector3[] normals = new Vector3[8] { n, n, n, n, n, n, n, n };
        Vector2[] uv = new Vector2[8] { uv0, uv1, uv2, uv3, uv4, uv5, uv6, uv7 };
        int[] dirttri = new int[6] { 4, 5, 6, 6, 7, 4 };
        int[] grasstri = new int[24] { 0, 1, 4, 4, 1, 5, 1, 6, 5, 1, 2, 6, 7, 6, 2, 7, 2, 3, 0, 4, 3, 3, 4, 7 };

        //push in mesh struct
        Mesh mesh = new Mesh();
        mesh.subMeshCount = 2;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.SetTriangles(dirttri, 0);
        mesh.SetTriangles(grasstri, 1);
        mesh.RecalculateNormals();
        return mesh;
    }
}
