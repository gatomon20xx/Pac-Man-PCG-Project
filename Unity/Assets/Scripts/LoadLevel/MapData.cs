//using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class MapData
{
    public string mapStringRaw;

    //[NonSerialized]
    public string[] mapStringSplit;


    // DATA
    //=====
    public bool isValidMap = false;
    public bool isFeaturesExtracted = false;

    //between 0, 1 (0.5 isi neutral and is invalid)
    public float preference = 0.5f;

    // = pellets + empty spaces where pellets could be
    // (!walls nor special nodes)
    public int totSpaces; 
    public int totRegPellets;
    public int totPowerPellets;

    public int totWalls;
    public int totPortals;
    public int totPortalExits;
    public List<Vector3Int> powerPelletPositions;

    // Features
    //=========
    //tot pellets, tot pellet/map space density, power/map space density, and power range

    // = totPellets / totSpaces
    public float pelletDensity;

    // = pow/spaces
    public float powerPelletDensity;
    //// = 1- above
    //int regPelletDensity; // Seems unnecessary to track both. Gives same info.

    public Vector2 rangePower2Power;
    //Vector2 rangeReg2Power;  //Valuable?????? It seems like it will usually be [1,1] 


    // HELPER FUNCTIONS
    //=================
    // Test for validity
    // @Gardone @Nancy == make this more robust
    public bool IsMapValid()
    {
        // Has to have at least one pellet
        // 
        bool regPellet = mapStringRaw.Contains('.');
        bool powerPellet = mapStringRaw.Contains('o');

        return (regPellet || powerPellet);
    }


    //private void ExtractMapFeatrues(ref MapData io_mapData)
    public bool ExtractMapFeatures(bool i_forceExtract = false)
    {
        //Debug.Log("Extracting Map Features.");

        bool featuresNeedExtracting = true;
        // IF not forcing AND features have already been extracted, return
        if (!i_forceExtract && isFeaturesExtracted)
        {
            Debug.Log($"No extraction of map features: i_forceExtract {i_forceExtract}");
            //Debug.Log("Extracting map features: FORCED && is already extracted");
            return !featuresNeedExtracting;
        }

        // ELSE extract features
        if (mapStringSplit == null)
            mapStringSplit = mapStringRaw.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);


        // RESET all "tot" counters to zero
        totSpaces = totRegPellets = totPowerPellets = totWalls = totPortals = totPortalExits = 0;

        powerPelletPositions = new();

        char charTileSymbol;
       
        int row = 0;
        foreach (var rowString in mapStringSplit)
        {
            for (int col = 0; col < mapStringSplit[row].Length; col++)
            {
                //currentTilePellet = pellets.GetTile(newTilePos);
                charTileSymbol = mapStringSplit[row][col];

                switch (TileConversion.Char2TileType(mapStringSplit[row][col]))
                {
                    case TileConversion.TileType.wall:
                        totWalls++;
                        break;
                    case TileConversion.TileType.portal:
                        totPortals++;
                        break;
                    case TileConversion.TileType.portalExit:
                        totPortalExits++;
                        break;
                    case TileConversion.TileType.empty:
                        totSpaces++;
                        break;
                    case TileConversion.TileType.pellet:
                        totRegPellets++;
                        totSpaces++;
                        break;
                    case TileConversion.TileType.pelletPower:
                        totPowerPellets++;
                        totSpaces++;
                        powerPelletPositions.Add(new Vector3Int(col, row, 0));
                        break;
                    case TileConversion.TileType.ghostHome:
                    case TileConversion.TileType.ghostDoor:
                    case TileConversion.TileType.pacman:
                    default:
                        break;
                }
            }
            row++;
        }

        // = totPellets / totSpaces
        pelletDensity = (totRegPellets + totPowerPellets) / (float)totSpaces;
        powerPelletDensity = totPowerPellets / (float)totSpaces;
        rangePower2Power = CalculateRange();

        isFeaturesExtracted = true;

        isValidMap = IsMapValid();

        return featuresNeedExtracting;
    }

    private Vector2 CalculateRange()
    {
        // If none, then distance is -1
        if (totPowerPellets == 0)
            return new Vector2(-1, -1);

        // If one, then distance is zero
        if (totPowerPellets == 1 )
            return Vector2.zero;
        
        Vector2 minMax = new(float.MaxValue, float.MinValue);

        int min = 0;
        int max = 1;

        for (int i = 0; i < totPowerPellets - 1; i++)
        {
            for (int j = i+1; j < totPowerPellets; j++)
            {
                float distance = Vector3Int.Distance(powerPelletPositions[i], powerPelletPositions[j]);
                if (distance < minMax[min])
                    minMax[min] = distance;
                if (distance > minMax[max])
                    minMax[max] = distance;
            }
        }
        return minMax;
    }



    public void LoadMapFromString(string i_fullMapString) 
    {
        mapStringRaw = i_fullMapString;
        mapStringSplit = mapStringRaw.Split('\n', System.StringSplitOptions.RemoveEmptyEntries);

        //ExtractMapFeaturesAndDraw(false);

        try
        {
            IsMapValid();
        }
        catch {
            Debug.LogError("Map Invalid");
        }
    }

}
