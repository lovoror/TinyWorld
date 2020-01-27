using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Grid))]
public class Map : MonoBehaviour
{
    private Grid grid;
    public Tilemap tilemap;
    public TilemapRenderer tilemapRenderer;

    void Start()
    {
        tilemapRenderer.enabled = false;
        tilemap.enabled = false;
        grid = GetComponent<Grid>();

        Vector3 dy = new Vector3(0, 0.5f * grid.cellSize.z, 0);

        Meteo meteo = Meteo.Instance;
        GameObject prefabContainer = new GameObject();
        prefabContainer.name = "PrefabContainer";
        prefabContainer.transform.localPosition = Vector3.zero;
        prefabContainer.transform.localScale = Vector3.one;
        prefabContainer.transform.localRotation = Quaternion.identity;
        prefabContainer.transform.parent = this.transform;

        for (int x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; x++)
            for (int z = tilemap.cellBounds.yMin; z < tilemap.cellBounds.yMax; z++)
            {
                Vector3Int cellPosition = new Vector3Int(x, z, (int)tilemap.transform.position.y);
                if (tilemap.HasTile(cellPosition))
                {
                    // standard
                    ScriptableTile tile = tilemap.GetTile<ScriptableTile>(cellPosition);
                    GameObject go = Instantiate(tile.prefab3d);
                    go.transform.parent = prefabContainer.transform;
                    go.transform.localPosition = grid.GetCellCenterWorld(cellPosition) - dy;
                    go.SetActive(true);

                    // add variability and suscribe to meteo
                    Transform tree = go.transform.Find("Tree");
                    tree.localPosition = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                    tree.localEulerAngles = new Vector3(0, Random.Range(-180f, 180f), 0);
                    float scale = Random.Range(0.7f, 1.3f);
                    tree.localScale = new Vector3(scale, scale, scale);
                    meteo.treesList.Add(tree.GetComponent<TreeComponent>());
                }
            }
    }
}
