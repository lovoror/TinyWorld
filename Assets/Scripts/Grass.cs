using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Grass : MonoBehaviour
{
    public AnimationCurve stringDensity;
    public List<GameObject> grassStrings;

    public void Initialize(int grassNeighbours)
    {
        // delete some grass string depending on local density
        for (int i = 0; i < grassStrings.Count; i++)
        {
            var temp = grassStrings[i];
            int randomIndex = Random.Range(i, grassStrings.Count);
            grassStrings[i] = grassStrings[randomIndex];
            grassStrings[randomIndex] = temp;
        }
        int stopIndex = grassStrings.Count - (int)(grassStrings.Count * stringDensity.Evaluate(0.125f * grassNeighbours));
        for (int i = stopIndex; i < grassStrings.Count; i++)
            DestroyImmediate(grassStrings[i], true);
        grassStrings.RemoveRange(stopIndex, grassStrings.Count - stopIndex);
                
        // Merge all grassSTring into one big mesh
        MeshFilter[] meshFilters = new MeshFilter[grassStrings.Count];
        for(int i= 0; i < grassStrings.Count; i++)
            meshFilters[i] = grassStrings[i].GetComponent<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for(int i=0; i< meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = Matrix4x4.TRS(meshFilters[i].transform.localPosition, meshFilters[i].transform.localRotation, meshFilters[i].transform.localScale);
            meshFilters[i].gameObject.SetActive(false);
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        transform.gameObject.SetActive(true);

        // clear and rotate for more variability
        for (int i = 0; i < grassStrings.Count; i++)
            DestroyImmediate(grassStrings[i], true);
        grassStrings.Clear();
        transform.localEulerAngles = new Vector3(0, Random.Range(0,3) * 90, 0);
    }
}
