using PCC.ContentRepresentation.Sample;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
//using UnityEngine.JSONSerializeModule;

public class MapManager : MonoBehaviour
{
    readonly string fileNameCrafted = "map_crafted_";
    readonly string fileLoadPathCrafted = "PCG_Content\\Maps_TrainingData\\";
    string fileLoad;// = fileLoadPath + fileName;

    string fileWritePath;// = Application.persistentDataPath + "/";
    string fileWriteCrafted;// = fileWritePath + fileNameCrafted;

    //readonly string fileNamePCG = "map_pcg_";
    //readonly string fileLoadPathPCG = "PCG_Content\\Maps_PCG\\";
    //string fileWritePCG;// = fileWritePath + fileNamePCG;

    private PCG pcg;
    public bool isLoadingNewMapsViaPCG = false;

    public Grid mapGrid;

    public Vector3Int ULpos = new Vector3Int(-13, 14, 0);
    //public Vector3Int centerpos = new Vector3Int(0, 0, 0);
    public Vector3Int BRpos = new Vector3Int(13, -17, 0);

    // TILEMAPS
    //=========
    // If a tile is a wall, it CANNOT be anything else
    public Tilemap walls;
    
    // CANNOT be walls
    public Tilemap pellets;
    public Tilemap turnNodes;
    
    // CANNOT be walls. 
    // CAN be drawn over pellets
    // portals, ghost home inside/outside
    public Tilemap specialNodes;


    // TILES
    //======
    public TileBase tilePellet;
    public TileBase tilePelletPower;
    public TileBase tileWall;
    public TileBase tilePortal;
    public TileBase tilePortalExit;
    public TileBase tileTurnNode;

    public List<MapData> _maps;

    [SerializeField]
    private int currentMapIndex = -1;

    // FUNCTIONS
    //==========


    private void Awake()
    {
        fileLoad = fileLoadPathCrafted + fileNameCrafted;
        fileWritePath = Application.persistentDataPath + "/";
        fileWriteCrafted = fileWritePath + fileNameCrafted;
    }


    // Start is called before the first frame update
    void Start()
    {
        //LoadCraftedMapsTextIntoJSON();
        LoadCraftedMapsFromJSON();

        if (currentMapIndex < 0 || currentMapIndex > _maps.Count - 1)
            currentMapIndex = UnityEngine.Random.Range(0, _maps.Count - 1);

        pcg = new PCG(_maps[0]);

        //pcg.InitPCG(_maps[0]);
        //pcg = new PCG(_maps[0].mapStringRaw);
        //pcg.SetSampleMap(_maps[0].mapStringRaw);

        //DrawCurrentMap();
        //SetUpMapFromString(_maps[currentMapIndex].mapStringRaw);
    }

    private void ClearMap(string[] mapStringsArray)
    {
        int row = 0;
        Vector3Int newTilePos;
        foreach (var rowString in mapStringsArray)
        {
            for (int col = 0; col < mapStringsArray[row].Length; col++)
            {
                newTilePos = this.GetTileWorldPosFromRowCol(row, col);
                if (!(row >= 12 && row <= 16 && col >= 10 && col <= 17))
                {
                    Debug.Log("Space cleared");
                    walls.SetTile(newTilePos, null);
                    turnNodes.SetTile(newTilePos, null);
                    specialNodes.SetTile(newTilePos, null);
                    pellets.SetTile(newTilePos, null);
                }
            }
            row++;
        }
    }

    public void GetNextLevel(Sample sample = null, Sample probsample = null)
    {
        bool pcgMapWorked = false;

        if (isLoadingNewMapsViaPCG && (sample == null || probsample == null))
        {
            pcgMapWorked = LoadAndDrawPCGMap(pcg.GenerateMap( pcg.CreateRandomizedMapFeatures() ));
            if (pcgMapWorked)
                return;
        }
        else if(isLoadingNewMapsViaPCG && sample != null && probsample != null)
        {
            pcgMapWorked = LoadAndDrawPCGMap(pcg.GenerateMap(pcg.CreateMapFromPCCSample(sample, probsample)));
            if (pcgMapWorked)
                return;
        }

        //else, we using crafted maps!!
        currentMapIndex++;
        currentMapIndex %= _maps.Count;

        DrawCurrentMap();
    }

    private void LoadCraftedMapsFromJSON()
    {
        MapData tmpMap;
        var jsonFiles = Resources.LoadAll<TextAsset>(fileLoadPathCrafted);
        int fileNumber = 0;
        foreach (var jsonFile in jsonFiles)
        {
            tmpMap = JsonUtility.FromJson<MapData>(jsonFile.ToString());

            // Will extract map features if not already done.
            //bool neededExtracting = tmpMap.ExtractMapFeatures(true);
            bool neededExtracting = tmpMap.ExtractMapFeatures();
            _maps.Add(tmpMap);

            // Save to file to extract data later
            if (neededExtracting)
            {
                string jsonNewString = JsonUtility.ToJson(tmpMap, true);
                System.IO.File.WriteAllText(fileWriteCrafted + fileNumber + ".json", jsonNewString);
            }
            fileNumber++;
        }
    }


    //@Gardone, this function is untested
    //TODO: test 
    // Returns false if map comes back invalid
    // Returns true if map can be added and drawn
    private bool LoadAndDrawPCGMap(string mapRawString)
    {
        MapData newMap = new MapData();
        newMap.mapStringRaw = mapRawString;
        newMap.ExtractMapFeatures();

        if (!newMap.isValidMap)
            return false;
        
        _maps.Add(newMap);
        currentMapIndex = _maps.Count - 1;
        DrawCurrentMap();
        return true;
    }



    private void DrawCurrentMap()
    {
        //Debug.Log("Drawing New Current Map");
        //string[] mapStringsArray = i_mapFullString.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        string[] mapStringsArray = _maps[currentMapIndex].mapStringSplit;

        ClearMap(mapStringsArray);

        char charTileSymbol;
        TileBase setTile = null;
        Vector3Int newTilePos;

        char tileLeft;
        char tileRight;
        char tileDown;
        char tileUp;

        TileBase currentTileWall = null;
        //TileBase currentTilePellet = null;
        //TileBase currentTile = null;

        int row = 0;
        foreach (var rowString in mapStringsArray)
        {
            for (int col = 0; col < mapStringsArray[row].Length; col++)
            {
                newTilePos = this.GetTileWorldPosFromRowCol(row, col);

                currentTileWall = walls.GetTile(newTilePos);
                if (currentTileWall != null)
                    continue;

                //currentTilePellet = pellets.GetTile(newTilePos);
                charTileSymbol = mapStringsArray[row][col];

                setTile = null;
                switch (charTileSymbol)
                {
                    case '.':
                        setTile = tilePellet;
                        break;
                    case 'o':
                        setTile = tilePelletPower;
                        break;
                    case '|':
                        setTile = tileWall;
                        break;
                    case ' ':
                        setTile = null;
                        break;
                    case 'w':
                        setTile = tilePortal;
                        break;
                    default:
                        break;
                }
                if (setTile == tileWall)
                {
                    walls.SetTile(newTilePos, setTile);
                }
                else if (setTile == tilePortal)
                {
                    if (col > mapStringsArray[row].Length / 2)
                    {
                        specialNodes.SetTile(this.GetTileWorldPosFromRowCol(row, col + 1), setTile);
                    }
                    else
                    {
                        specialNodes.SetTile(this.GetTileWorldPosFromRowCol(row, col - 1), setTile);
                    }
                }
                else
                {
                    pellets.SetTile(newTilePos, setTile);
                }
            }
            row++;
        }

        row = 0;
        foreach (var rowString in mapStringsArray)
        {
            for (int col = 0; col < mapStringsArray[row].Length; col++)
            {
                newTilePos = this.GetTileWorldPosFromRowCol(row, col);

                // Make sure that within bounds, the nodes are placed properly.
                if (col > 0 && col < mapStringsArray[row].Length - 1 && row > 0 && row < mapStringsArray.Length - 1)
                {
                    tileLeft = mapStringsArray[row - 1][col];
                    tileRight = mapStringsArray[row + 1][col];
                    tileDown = mapStringsArray[row][col + 1];
                    tileUp = mapStringsArray[row][col - 1];
                    if ((tileLeft == ' ' || tileRight == ' ' || tileLeft == '.' || tileRight == '.' || tileLeft == 'o' || tileRight == 'o') &&
                        (tileUp == ' ' || tileDown == ' ' || tileUp == '.' || tileDown == '.' || tileUp == 'o' || tileDown == 'o'))
                    {
                        turnNodes.SetTile(newTilePos, tileTurnNode);
                    }
                }
            }
            row++;
        }

        //Debug.Log("END drawing");
    }

    private void SetUpMapFromString(string i_mapFullString)
    {
        Debug.Log("Loading pellets from string.");
        string[] mapStringsArray = i_mapFullString.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        char charTileSymbol;
        TileBase setTile = null;
        Vector3Int newTilePos;

        TileBase currentTileWall = null;
        //TileBase currentTilePellet = null;
        //TileBase currentTile = null;

        int row = 0;
        foreach (var rowString in mapStringsArray)
        {
            for (int col = 0; col < mapStringsArray[row].Length; col++)
            {
                newTilePos = this.GetTileWorldPosFromRowCol(row, col);

                currentTileWall = walls.GetTile(newTilePos);
                if (currentTileWall != null)
                    continue;

                //currentTilePellet = pellets.GetTile(newTilePos);
                charTileSymbol = mapStringsArray[row][col];

                setTile = null;
                switch (charTileSymbol)
                {
                    case '.':
                        setTile = tilePellet;
                        break;
                    case 'o':
                        setTile = tilePelletPower;
                        break;
                    case '|':
                        setTile = tileWall;
                        break;
                    case '-':
                        setTile = tileWall;
                        break;
                    case ' ':
                        setTile = null;
                        break;
                    default:
                        break;
                }
                if (setTile == tileWall)
                {
                    walls.SetTile(newTilePos, setTile);
                }
                else
                    pellets.SetTile(newTilePos, setTile);
            }
            row++;
        }

        Debug.Log("END drawing");
    }

    
    // Default grid/tilemap coordinate system is x left-right (col), and y up-down (row)
    // i.e., x == col.... y == row
    private Vector3Int GetTileWorldPosFromRowCol(int i_row, int i_col)
    {
        return GetTileWorldPosFromXY(i_col, i_row);
        //int tmpX = ULpos.x + i_col - 1;
        //int tmpY = ULpos.y - i_row - 1;

        //Vector3Int tilePos = new Vector3Int(tmpX, tmpY, ULpos.z);

        //return tilePos;
    }

    private Vector3Int GetTileWorldPosFromXY(int i_x, int i_y)
    {
        int tmpX = ULpos.x + i_x - 1;
        int tmpY = ULpos.y - i_y - 1;

        Vector3Int tilePos = new Vector3Int(tmpX, tmpY, ULpos.z);

        return tilePos;
    }

    // Scripts for manipulating text maps into JSON
    //---------------------------------------------
    void LoadCraftedMapsTextIntoJSON()
    {
        MapData _tmpMapData = null;
        for (int i = 2; i <= 7; i++)
        {
            var textFile = Resources.Load<TextAsset>(fileLoad + i);
            _tmpMapData.mapStringRaw = textFile.ToString();
            var json2 = JsonUtility.ToJson(_tmpMapData, true);
            System.IO.File.WriteAllText(fileWriteCrafted + i + ".json", json2);

        }
        // Gather other data for map and store into _firstMapTest here:
    }

}
