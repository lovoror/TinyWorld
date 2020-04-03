using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(Grid))]
public class Map : MonoBehaviour
{
    public Grid grid;
    public Navigation navigation;
    public Tilemap tilemap;
    public TilemapRenderer tilemapRenderer;
    public GameObject tilesContainer;
    public GameObject buildingsContainer;
    public List<ScriptableTile> tileList;
    public Transform player;
    public int streamingRadius = 10;
    public Vector2 streamingThresholds;
    
    private Dictionary<string, Tile> tileDictionary;
    private Vector3 dy;
    private Vector3 lastStreamingUpdate;
    private Dictionary<Vector3Int, GameObject> streamingArea = new Dictionary<Vector3Int, GameObject>();

    // Singleton struct
    private static Map _instance;
    public static Map Instance { get { return _instance; } }

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
    }

    void Start()
    {
        tilemapRenderer.enabled = false;
        tilemap.enabled = false;
        grid = GetComponent<Grid>();
        dy = new Vector3(0, 0.5f * grid.cellSize.z, 0);

        tileDictionary = new Dictionary<string, Tile>();
        foreach (Tile tile in tileList)
            tileDictionary.Add(tile.name, tile);
        
        if(buildingsContainer == null)
        {
            buildingsContainer = new GameObject();
            buildingsContainer.name = "ConstructionContainer";
            buildingsContainer.transform.localPosition = Vector3.zero;
            buildingsContainer.transform.localScale = Vector3.one;
            buildingsContainer.transform.localRotation = Quaternion.identity;
            buildingsContainer.transform.parent = this.transform;
        }

        tilesContainer = new GameObject();
        tilesContainer.name = "PrefabContainer";
        tilesContainer.transform.localPosition = Vector3.zero;
        tilesContainer.transform.localScale = Vector3.one;
        tilesContainer.transform.localRotation = Quaternion.identity;
        tilesContainer.transform.parent = this.transform;

        lastStreamingUpdate = player.position + 3 * new Vector3(streamingThresholds.x, 0, streamingThresholds.y);
    }


    private void Update()
    {
        Vector3Int p = GetCellFromWorld(player.position);
        Vector3 d = player.position - lastStreamingUpdate;

        if(Mathf.Abs(d.x) > streamingThresholds.x || Mathf.Abs(d.z) > streamingThresholds.y)
        {
            // delete far tiles
            List<Vector3Int> removed = new List<Vector3Int>();
            foreach(KeyValuePair<Vector3Int, GameObject> cp in streamingArea)
            {
                if(Mathf.Abs(p.x - cp.Key.x) > streamingRadius || Mathf.Abs(p.y - cp.Key.y) > streamingRadius)
                {
                    if(cp.Value)
                        Destroy(cp.Value);
                    removed.Add(cp.Key);
                }
            }
            foreach (Vector3Int rcp in removed)
                streamingArea.Remove(rcp);

            // create new tiles
            for (int x = p.x - streamingRadius; x < p.x + streamingRadius; x++)
                for (int z = p.y - streamingRadius; z < p.y + streamingRadius; z++)
                {
                    Vector3Int cellPosition = new Vector3Int(x, z, (int)tilemap.transform.position.y);
                    if (tilemap.HasTile(cellPosition))
                    {
                        // standard
                        ScriptableTile tile = tilemap.GetTile<ScriptableTile>(cellPosition);
                        if (tile.tilePrefab && !streamingArea.ContainsKey(cellPosition))
                        {
                            streamingArea.Add(cellPosition, TileInit(tile, cellPosition));
                        }
                        
                    }
                }

            // end
            lastStreamingUpdate = grid.GetCellCenterWorld(grid.WorldToCell(player.position));
        }
    }

    public void PlaceTiles(List<Vector3> positions, List<GameObject> originals, string tileName)
    {
        foreach (GameObject original in originals)
            Destroy(original);
        if (tileDictionary.ContainsKey(tileName))
        {
            // construct tile to replace list
            List<KeyValuePair<ScriptableTile, Vector3Int>> list = new List<KeyValuePair<ScriptableTile, Vector3Int>>();
            foreach(Vector3 p in positions)
            {
                Vector3Int cell = GetCellFromWorld(p);
                tilemap.SetTile(cell, tileDictionary[tileName]);
                ScriptableTile tile = tilemap.GetTile<ScriptableTile>(cell);
                if(tile)
                {
                    list.Add(new KeyValuePair<ScriptableTile, Vector3Int>(tile, cell));
                }
            }

            // neighbours to update
            HashSet<Vector3Int> neighbourgs = new HashSet<Vector3Int>();
            foreach (KeyValuePair<ScriptableTile, Vector3Int> entry in list)
            {
                Vector3Int n1 = entry.Value + new Vector3Int(1, 0, 0);
                Vector3Int n2 = entry.Value + new Vector3Int(-1, 0, 0);
                Vector3Int n3 = entry.Value + new Vector3Int(0, 1, 0);
                Vector3Int n4 = entry.Value + new Vector3Int(0, -1, 0);

                if (!InList(n1, ref list))
                    neighbourgs.Add(n1);
                if (!InList(n2, ref list))
                    neighbourgs.Add(n2);
                if (!InList(n3, ref list))
                    neighbourgs.Add(n3);
                if (!InList(n4, ref list))
                    neighbourgs.Add(n4);
            }

            if (((ScriptableTile)tileDictionary[tileName]).neighbourUpdate)
            {
                foreach (Vector3Int cell in neighbourgs)
                {
                    List<GameObject> neighbourgGo = SearchTilesGameObject(grid.GetCellCenterWorld(cell) - dy, 0.5f);
                    foreach (GameObject go in neighbourgGo)
                        Destroy(go);
                    ScriptableTile tile = tilemap.GetTile<ScriptableTile>(cell);

                    if (tile)
                    {
                        if (tile.buildingUpdate)
                        {
                            List<GameObject> buildingGo = SearchBuildingsGameObject(grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0)) - dy, 0.5f);
                            foreach (GameObject go in buildingGo)
                                Destroy(go);
                        }
                        list.Add(new KeyValuePair<ScriptableTile, Vector3Int>(tile, new Vector3Int(cell.x, cell.y, 0)));
                    }
                    
                }
            }

            foreach (KeyValuePair<ScriptableTile, Vector3Int> entry in list)
                TileInit(entry.Key, entry.Value);
        }
        else Debug.LogWarning("no " + tileName + " in dictionary");
    }
    public List<GameObject> SearchBuildingsGameObject(Vector3 position, float radius)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (Transform child in buildingsContainer.transform)
        {
            if ((child.position - position).sqrMagnitude < radius * radius)
                result.Add(child.gameObject);
        }
        return result;
    }
    public List<GameObject> SearchTilesGameObject(Vector3 position, float radius)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (Transform child in tilesContainer.transform)
        {
            if ((child.position - position).sqrMagnitude < radius * radius)
                result.Add(child.gameObject);
        }
        return result;
    }
    public List<GameObject> MultiSearch(List<Vector3> positions, float radius)
    {
        List<GameObject> result = new List<GameObject>();
        foreach (Transform child in tilesContainer.transform)
        {
            foreach(Vector3 p in positions)
            {
                if ((child.position - p).sqrMagnitude < radius * radius)
                    result.Add(child.gameObject);
            }
        }
        return result;
    }
    public Vector3Int GetCellFromWorld(Vector3 position)
    {
        Vector3Int c = tilemap.WorldToCell(position);
        return new Vector3Int(c.x, c.y, (int)tilemap.transform.position.z);
    }

    private GameObject TileInit(ScriptableTile tile, Vector3Int cellPosition)
    {
        // tile prefab
        GameObject tilego = Instantiate(tile.tilePrefab);
        
        tilego.name = tile.name;
        tilego.transform.parent = tilesContainer.transform;
        tilego.transform.localPosition = grid.GetCellCenterWorld(cellPosition) - dy;
        tilego.transform.localEulerAngles = new Vector3(0, -tilemap.GetTransformMatrix(cellPosition).rotation.eulerAngles.z, 0);
        tilego.SetActive(true);

        var agent = tilego.GetComponent<AgentBase>();
        if (agent)
        {
            agent.cell = cellPosition;
            agent.radius = 2;
            agent.Subscribe();
        }
        var terrain = tilego.GetComponent<TerrainBase>();
        if (terrain)
        {
            terrain.cell = cellPosition;
            terrain.radius = 2;
            terrain.Subscribe();
        }
        // building prefab
        TileBuildingInit(tile, cellPosition);

        // add variability and suscribe to meteo
        Transform pivot = tilego.transform.Find("Pivot");
        if (pivot)
        {
            pivot.localPosition = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
            pivot.localEulerAngles = new Vector3(0, Random.Range(-180f, 180f), 0);
            float scale = Random.Range(0.7f, 1.3f);
            pivot.localScale = new Vector3(scale, scale, scale);
            TreeComponent tree = pivot.GetComponent<TreeComponent>();
            if (tree)
                Meteo.Instance.treesList.Add(tree);
        }

        InitGrass(tilego.GetComponent<Grass>(), cellPosition);
        InitDirt(tilego.GetComponent<Dirt>(), cellPosition);
        InitWater(tilego.GetComponent<Water>(), cellPosition);
        InitBridge(tilego.GetComponent<Bridge>(), cellPosition);
        InitStone(tilego.GetComponent<Stone>(), cellPosition);
        InitMineral(tilego.GetComponent<MineralRessource>(), cellPosition, tile.optionalMaterial);
        return tilego;
    }
    private void TileBuildingInit(ScriptableTile tile, Vector3Int cellPosition)
    {
        if(tile.buildingPrefab)
        {
            GameObject buildinggo = Instantiate(tile.buildingPrefab, buildingsContainer.transform);
            buildinggo.name = tile.buildingPrefab.name;
            buildinggo.transform.localPosition = grid.GetCellCenterWorld(cellPosition) - dy;
            buildinggo.transform.localEulerAngles = new Vector3(-90, 90-tilemap.GetTransformMatrix(cellPosition).rotation.eulerAngles.z, 0);
            buildinggo.SetActive(true);

            InitWall(buildinggo.GetComponent<Wall>(), cellPosition, tile.name);
        }
    }
    private void InitDirt(Dirt dirt, Vector3Int cellPosition)
    {
        if (dirt)
        {
            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1, 0, 0));
            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(1, 0, 0));
            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, -1, 0));
            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, 1, 0));

            bool xmb = (xm && xm.tilePrefab && (xm.tilePrefab.GetComponent<Dirt>() != null || xm.tilePrefab.name == "Bridge"));
            bool xpb = (xp && xp.tilePrefab && (xp.tilePrefab.GetComponent<Dirt>() != null || xp.tilePrefab.name == "Bridge"));
            bool zmb = (zm && zm.tilePrefab && (zm.tilePrefab.GetComponent<Dirt>() != null || zm.tilePrefab.name == "Bridge"));
            bool zpb = (zp && zp.tilePrefab && (zp.tilePrefab.GetComponent<Dirt>() != null || zp.tilePrefab.name == "Bridge"));

            dirt.InitializeFromPool(xpb, xmb, zmb, zpb, 0.3f);
        }
    }
    private void InitGrass(Grass grass, Vector3Int cellPosition)
    {
        if (grass)
        {
            BoundsInt area = new BoundsInt();
            area.min = cellPosition + new Vector3Int(-1, -1, 0);
            area.max = cellPosition + new Vector3Int(2, 2, 1);
            TileBase[] neighbours = tilemap.GetTilesBlock(area);

            int grassNeighbours = 0;
            for (int i = 0; i < neighbours.Length; i++)
            {
                ScriptableTile n = (ScriptableTile)neighbours[i];
                if (n && n.name == "Grass")
                    grassNeighbours++;
            }
            grass.InitializeFromPool(Mathf.Clamp(grassNeighbours - 1 + Random.Range(-1, 1), 0, 8));
        }
    }
    private void InitWall(Wall wall, Vector3Int cellPosition, string tileName)
    {
        if (wall)
        {
            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1, 0, 0));
            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(1, 0, 0));
            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, -1, 0));
            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, 1, 0));

            bool xmb = (xm && xm.tilePrefab && xm.tilePrefab.name.Contains("Wall"));
            bool xpb = (xp && xp.tilePrefab && xp.tilePrefab.name.Contains("Wall"));
            bool zmb = (zm && zm.tilePrefab && zm.tilePrefab.name.Contains("Wall"));
            bool zpb = (zp && zp.tilePrefab && zp.tilePrefab.name.Contains("Wall"));

            wall.Initialize(xpb, xmb, zmb, zpb, tileName);
        }
    }
    private void InitWater(Water water, Vector3Int cellPosition)
    {
        if(water)
        {
            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1, 0, 0));
            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(1, 0, 0));
            ScriptableTile zm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, -1, 0));
            ScriptableTile zp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(0, 1, 0));

            bool xmb = (xm && xm.tilePrefab && (xm.tilePrefab.name == "Water" || xm.tilePrefab.name == "Bridge"));
            bool xpb = (xp && xp.tilePrefab && (xp.tilePrefab.name == "Water" || xp.tilePrefab.name == "Bridge"));
            bool zmb = (zm && zm.tilePrefab && (zm.tilePrefab.name == "Water" || zm.tilePrefab.name == "Bridge"));
            bool zpb = (zp && zp.tilePrefab && (zp.tilePrefab.name == "Water" || zp.tilePrefab.name == "Bridge"));

            water.Initialize(xpb, xmb, zmb, zpb, 0.3f);
        }
    }
    private void InitBridge(Bridge bridge, Vector3Int cellPosition)
    {
        if (bridge)
        {
            ScriptableTile xm = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int(-1, 0, 0));
            ScriptableTile xp = tilemap.GetTile<ScriptableTile>(cellPosition + new Vector3Int( 1, 0, 0));
            bool xmIsWater = xm && xm.tilePrefab && xm.tilePrefab.GetComponent<Water>();
            bool xpIsWater = xp && xp.tilePrefab && xp.tilePrefab.GetComponent<Water>();
            bridge.Initialize(!xmIsWater && !xpIsWater);
        }
    }
    private void InitStone(Stone stone, Vector3Int cellPosition)
    {
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
    }
    private void InitMineral(MineralRessource mineral, Vector3Int cellPosition, Material material)
    {
        if (mineral)
            mineral.Initialize(material);
    }

    private bool InList(Vector3Int search, ref List<KeyValuePair<ScriptableTile, Vector3Int>> list)
    {
        foreach(KeyValuePair<ScriptableTile, Vector3Int> entry in list)
        {
            if (entry.Value == search)
                return true;
        }
        return false;
    }
}
