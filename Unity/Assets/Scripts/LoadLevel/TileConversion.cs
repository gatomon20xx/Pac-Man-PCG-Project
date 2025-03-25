using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TileConversion
{
    public enum TileType
    {
        empty,
        wall,
        pellet,
        pelletPower,
        ghostHome,
        ghostDoor,
        pacman,
        portal,
        portalExit
    };

    public static TileBase empty = null;
    public static TileBase pelletReg = Resources.Load<TileBase>("Assets/Tiles/Pellet_Tile");
    public static TileBase pelletPower = Resources.Load<TileBase>("Assets/Tiles/PowerPellet_Tile");
    public static TileBase wall = Resources.Load<TileBase>("Assets/Tiles/Wall_00");
    public static TileBase ghostHome = null;
    public static TileBase ghostDoor = null;
    public static TileBase pacmanStart = null;
    public static TileBase portal = null;
    public static TileBase portalExit = null;

    //"trafficSpace" is any tile where player/NPC can move on,
    //    - may be empty or contain pickups and pellets
    //    - CANNOT be a wall OR door (but could be a door later)
    // Will need later for adding A*
    // IS NOT assigned via the map string directly (so no tileType specifically)
    public static TileBase trafficSpace = null;

    public static Dictionary<char, TileType> _char2TileTypeDict = new Dictionary<char, TileType>() { 
        {' ', TileType.empty},
        {'-', TileType.wall},
        {'|', TileType.wall},
        {'.', TileType.pellet},
        {'o', TileType.pelletPower},
        {'g', TileType.ghostHome},
        {'d', TileType.ghostDoor},
        {'p', TileType.pacman},
        {'w', TileType.portal},
        {'e', TileType.portalExit}
    };
    public static Dictionary<TileType, TileBase> _tileType2TileDict = new Dictionary<TileType, TileBase>() {
        {TileType.empty, empty},
        {TileType.wall, wall},
        {TileType.pellet, pelletReg},
        {TileType.pelletPower, pelletPower},
        {TileType.ghostHome, ghostHome},
        {TileType.ghostDoor, ghostDoor},
        {TileType.pacman, pacmanStart},
        {TileType.portal, portal},
        {TileType.portalExit, portalExit}
    };


    
    // Char --> Tile prefab
    public static TileBase Char2Tile(char i_char)
    {
        return _tileType2TileDict[_char2TileTypeDict[i_char]];
    }

    // Char --> TileType
    public static TileType Char2TileType(char i_char)
    {
        return _char2TileTypeDict[i_char];
    }

    // TileType --> Tile prefab
    public static TileBase TileType2Tile(TileType i_TileType)
    {
        return _tileType2TileDict[i_TileType];
    }

}
