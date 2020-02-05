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
                    if(tile.prefab3d)
                    {
                        GameObject go = Instantiate(tile.prefab3d);
                        go.transform.parent = prefabContainer.transform;
                        go.transform.localPosition = grid.GetCellCenterWorld(cellPosition) - dy;
                        go.SetActive(true);
                        
                        // add variability and suscribe to meteo
                        Transform tree = go.transform.Find("Tree");
                        if(tree)
                        {
                            tree.localPosition = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                            tree.localEulerAngles = new Vector3(0, Random.Range(-180f, 180f), 0);
                            float scale = Random.Range(0.7f, 1.3f);
                            tree.localScale = new Vector3(scale, scale, scale);
                            meteo.treesList.Add(tree.GetComponent<TreeComponent>());
                        }

                        // grass tiles initialization
                        Grass grass = go.GetComponent<Grass>();
                        if(grass)
                        {
                            BoundsInt area = new BoundsInt();
                            area.min = cellPosition + new Vector3Int(-1, -1, 0);
                            area.max = cellPosition + new Vector3Int(2, 2, 1);
                            TileBase[] neighbours = tilemap.GetTilesBlock(area);
                            
                            int grassNeighbours = 0;
                            for (int i=0; i<neighbours.Length; i++)
                            {
                                ScriptableTile n = (ScriptableTile)neighbours[i];
                                if (n && n.name == "Grass")
                                    grassNeighbours++;
                            }
                            grass.Initialize(grassNeighbours - 1);
                        }

                        // dirt tiles initialization
                        Dirt dirt = go.GetComponent<Dirt>();
                        if(dirt)
                        {
                            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1,  0, 0));
                            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 1,  0, 0));
                            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 0, -1, 0));
                            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 0, 1,  0));

                            bool xmb = (xm && xm.prefab3d.name == "Dirt");
                            bool xpb = (xp && xp.prefab3d.name == "Dirt");
                            bool zmb = (zm && zm.prefab3d.name == "Dirt");
                            bool zpb = (zp && zp.prefab3d.name == "Dirt");

                            dirt.Initialize(xpb, xmb, zmb, zpb, 0.3f);
                        }

                        // dirt tiles initialization
                        Wall wall = go.GetComponent<Wall>();
                        if (wall)
                        {
                            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1,  0, 0));
                            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 1,  0, 0));
                            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 0, -1, 0));
                            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 0,  1, 0));

                            bool xmb = (xm && xm.prefab3d.name.Contains("Wall"));
                            bool xpb = (xp && xp.prefab3d.name.Contains("Wall"));
                            bool zmb = (zm && zm.prefab3d.name.Contains("Wall"));
                            bool zpb = (zp && zp.prefab3d.name.Contains("Wall"));

                            wall.Initialize(xpb, xmb, zmb, zpb);
                        }

                        // water tiles initialization
                        Water water = go.GetComponent<Water>();
                        if (water)
                        {
                            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1, 0, 0));
                            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(1, 0, 0));
                            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, -1, 0));
                            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, 1, 0));

                            bool xmb = (xm && xm.prefab3d.name == "Water");
                            bool xpb = (xp && xp.prefab3d.name == "Water");
                            bool zmb = (zm && zm.prefab3d.name == "Water");
                            bool zpb = (zp && zp.prefab3d.name == "Water");

                            water.Initialize(xpb, xmb, zmb, zpb, 0.3f);
                        }
                    }
                }
            }
    }
}
