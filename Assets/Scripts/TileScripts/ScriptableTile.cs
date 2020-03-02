using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "ScriptableTile", menuName = "Custom/ScriptableTile", order = 1)]
public class ScriptableTile : Tile
{
    public GameObject prefab3d;
    public Material option1;
    public bool neighbourUpdate = false;
}
