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
                        Transform pivot = go.transform.Find("Pivot");
                        if(pivot)
                        {
                            pivot.localPosition = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                            pivot.localEulerAngles = new Vector3(0, Random.Range(-180f, 180f), 0);
                            float scale = Random.Range(0.7f, 1.3f);
                            pivot.localScale = new Vector3(scale, scale, scale);
                            meteo.treesList.Add(pivot.GetComponent<TreeComponent>());
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
                            grass.InitializeFromPool(Mathf.Clamp(grassNeighbours - 1 + Random.Range(-1, 1), 0, 8));
                        }

                        // dirt tiles initialization
                        Dirt dirt = go.GetComponent<Dirt>();
                        if(dirt)
                        {
                            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1,  0, 0));
                            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 1,  0, 0));
                            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 0, -1, 0));
                            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 0, 1,  0));

                            bool xmb = (xm && xm.prefab3d && (xm.prefab3d.name == "Dirt" || xm.prefab3d.name == "Bridge" || xm.prefab3d.name.Contains("Crop")));
                            bool xpb = (xp && xp.prefab3d && (xp.prefab3d.name == "Dirt" || xp.prefab3d.name == "Bridge" || xp.prefab3d.name.Contains("Crop")));
                            bool zmb = (zm && zm.prefab3d && (zm.prefab3d.name == "Dirt" || zm.prefab3d.name == "Bridge" || zm.prefab3d.name.Contains("Crop")));
                            bool zpb = (zp && zp.prefab3d && (zp.prefab3d.name == "Dirt" || zp.prefab3d.name == "Bridge" || zp.prefab3d.name.Contains("Crop")));

                            dirt.InitializeFromPool(xpb, xmb, zmb, zpb, 0.3f);
                        }

                        // dirt tiles initialization
                        Wall wall = go.GetComponent<Wall>();
                        if (wall)
                        {
                            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1,  0, 0));
                            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 1,  0, 0));
                            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 0, -1, 0));
                            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 0,  1, 0));

                            bool xmb = (xm && xm.prefab3d && xm.prefab3d.name.Contains("Wall"));
                            bool xpb = (xp && xp.prefab3d && xp.prefab3d.name.Contains("Wall"));
                            bool zmb = (zm && zm.prefab3d && zm.prefab3d.name.Contains("Wall"));
                            bool zpb = (zp && zp.prefab3d && zp.prefab3d.name.Contains("Wall"));

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

                            bool xmb = (xm && xm.prefab3d && (xm.prefab3d.name == "Water" || xm.prefab3d.name == "Bridge"));
                            bool xpb = (xp && xp.prefab3d && (xp.prefab3d.name == "Water" || xp.prefab3d.name == "Bridge"));
                            bool zmb = (zm && zm.prefab3d && (zm.prefab3d.name == "Water" || zm.prefab3d.name == "Bridge"));
                            bool zpb = (zp && zp.prefab3d && (zp.prefab3d.name == "Water" || zp.prefab3d.name == "Bridge"));

                            water.Initialize(xpb, xmb, zmb, zpb, 0.3f);
                        }

                        // bridges tiles
                        Bridge bridge = go.GetComponent<Bridge>();
                        if (bridge)
                        {
                            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1, 0, 0));
                            bridge.Initialize(xm && xm.prefab3d && xm.prefab3d.name == "Dirt");
                        }

                        // stone tiles initialization
                        Stone stone = go.GetComponent<Stone>();
                        if (stone)
                        {
                            BoundsInt area = new BoundsInt();
                            area.min = cellPosition + new Vector3Int(-1, -1, 0);
                            area.max = cellPosition + new Vector3Int(2, 2, 1);
                            TileBase[] neighbours = tilemap.GetTilesBlock(area);

                            int grassNeighbours = 0;
                            for (int i = 0; i < neighbours.Length; i++)
                            {
                                ScriptableTile n = (ScriptableTile)neighbours[i];
                                if (n && (n.name == "Grass" || n.name.Contains("Crop")))
                                    grassNeighbours++;
                            }
                            stone.Initialize(2 - grassNeighbours / 3);
                        }

                        // minerals tiles initialization
                        MineralRessource mineral = go.GetComponent<MineralRessource>();
                        if (mineral)
                        {
                            mineral.Initialize(tile.option1);
                        }
                    }
                }
            }
    }
}
