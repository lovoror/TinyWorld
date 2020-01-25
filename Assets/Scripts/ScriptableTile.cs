using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "ScriptableTile", menuName = "Custom/ScriptableTile", order = 1)]
public class ScriptableTile : Tile
{
    public GameObject prefab3d;
    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);
        //tileData.gameObject = prefab;
    }

}
