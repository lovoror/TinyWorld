using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fences : MonoBehaviour
{
    public MeshFilter meshFilter;

    public GameObject fl;
    public GameObject fr;
    public GameObject fu;
    public GameObject fd;

    public GameObject flu;
    public GameObject fld;
    public GameObject fru;
    public GameObject frd;

    public List<GameObject> childs;

    public void Initialize(bool left, bool right, bool up, bool down)
    {
        List<GameObject> allChilds = new List<GameObject>(childs);

        if (left) childs.Remove(fl);
        if (right) childs.Remove(fr);
        if (up) childs.Remove(fu);
        if (down) childs.Remove(fd);

        if (left && up) childs.Remove(flu);
        if (left && down) childs.Remove(fld);
        if (right && up) childs.Remove(fru);
        if (right && down) childs.Remove(frd);
        
        CombineInstance[] combine = new CombineInstance[childs.Count];
        for (int i = 0; i < childs.Count; i++)
        {
            combine[i].mesh = childs[i].GetComponent<MeshFilter>().mesh;
            combine[i].transform = Matrix4x4.identity;

            BoxCollider[] box2 = childs[i].GetComponents<BoxCollider>();
            if(box2.Length > 0)
            {
                foreach(BoxCollider b2 in box2)
                {
                    BoxCollider box = gameObject.AddComponent<BoxCollider>();
                    box.center = transform.InverseTransformPoint(childs[i].transform.TransformPoint(b2.center));
                    box.size = transform.InverseTransformDirection(childs[i].transform.TransformDirection(b2.size));
                }
            }
        }
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine);
        meshFilter.mesh.RecalculateBounds();

        foreach (GameObject child in allChilds)
            Destroy(child);
    }
}
