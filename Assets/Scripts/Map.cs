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

        for (int x = tilemap.cellBounds.xMin; x < tilemap.cellBounds.xMax; x++)
            for (int z = tilemap.cellBounds.yMin; z < tilemap.cellBounds.yMax; z++)
            {
                Vector3Int cellPosition = new Vector3Int(x, z, (int)tilemap.transform.position.y);
                if (tilemap.HasTile(cellPosition))
                {
                    ScriptableTile tile = tilemap.GetTile<ScriptableTile>(cellPosition);
                    GameObject go = Instantiate(tile.prefab3d);
                    go.transform.localPosition = grid.CellToWorld(cellPosition);
                    go.SetActive(true);
                    Debug.Log("toto");
                }
            }

        Debug.Log(tilemap.cellBounds);
    }
}
