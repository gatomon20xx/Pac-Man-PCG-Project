/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class GenerateLevel : MonoBehaviour
{
    [SerializeField] public int mazeX = 90;
    [SerializeField] public int mazeY = 90;

    [SerializeField] Tilemap floorTilemap, wallTilemap, craftTilemap;
    [SerializeField] TileBase wallTile;

    GameObject console;

    Vector2Int[][] mazeTiles = new Vector2Int[36][28];

    public void RunGeneration()
    {

        var grids = GameObject.FindObjectsOfType<Grid>();
        foreach (var grid in grids)
        {
            if (grid.transform.childCount < 2)
            {
                craftTilemap = grid.transform.GetChild(0).GetComponent<Tilemap>();
            }
        }

        foreach (var position in mazeTiles)
        {
            PaintSingleTile(position, floorTilemap, wallTile);
        }

        wallTilemap.ClearAllTiles();
        PlaceWalls();
    }

    private void PaintSingleTile(Vector2Int position, Tilemap tilemap, TileBase tile)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    private void PlaceWalls()
    {
        for (int x = -dungeonX; x <= dungeonX; x++)
        {
            for (int y = -dungeonY; y <= dungeonY; y++)
            {
                if (!floorTilemap.HasTile(new Vector3Int(x, y)) && !craftTilemap.HasTile(new Vector3Int(x, y)))
                {
                    PaintSingleTile(new Vector2Int(x, y), wallTilemap, wallTile);
                }
            }
        }
    }
}
*/