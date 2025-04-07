using System;
using System.Text;
using System.Linq;
//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PCC.ContentRepresentation.Sample;


// TODO: I don't believe MonoBehavior is requisite for this class.
//public class PCG : MonoBehaviour
public class PCG
{
    //The map is laid out as rows (x) and cols (y).
    //Each string in the array is a row
    private string[] BaseMapStringArray { get; set; }

    private MapData BaseMapData { get; set; }
    private string BaseMapStringRaw { get; set; }

    private List<StringBuilder> baseStringBuilderArray;

    private Map map;

    // These can be LEARNED or hard coded values.
    // Once the game starts, these values stay the same.
    public static class MapRules
    {
        // WALL RULES
        //-----------
        //public Vector2Int widthRange = new Vector2Int(10, 30);
        //public Vector2Int heightRange = new Vector2Int(10, 45);
        //Rule for borders are walls OR portals

        // Pellet & Empty Space Rules:
        //---------------------------
        public static Vector2Int totSpacesRange = new Vector2Int(20, 296);
        // Tot pellets CANNOT be more than tot spaces
        public static Vector2Int totPelletsRange = new Vector2Int(1, 296);

        

        
    }

    // This will be the data to receive from the pref learning
    // Can change these to ranges
    public struct MapFeatures
    {
        // TOTAL pellet density = TOT pellets / TOT spaces
        public float totPelletDensity;
        //public Vector2 pelletDensityMinMax;
        
        // = power/totPellets
        public float powerPelletDensity;
        //// = 1- above
        //int regPelletDensity; // Seems unnecessary to track both. Gives same info.

        public double smallStopMapGrowth;
        public double medStopMapGrowth;
        public double larStopMapGrowth;

        public double sizeTwoMapExtend;
        public double sizeFourMapExtend;
        public double topAndBotMapJoin;
        public int maxLongPieces;

        // TOTAL ghost count
        // public int ghostCount;

        public Vector2 rangePower2Power;

        public bool isSymetric;
    }

    public class MapValues
    {
        public int cols;
        public int rows;

        public int totPellets;
        public int totRegPellets;
        public int totPowerPellets;

        public double totSmallStop;
        public double totMedStop;
        public double totLargeStop;

        public double totSizeTwo;
        public double totSizeFour;
        public double totTopBot;
        public int totLongPieces;

        //public int totWalls;
        //public int totPortals;
        //public int totPortalExits;
    }

    // These will change each time we make a new map
    public MapFeatures mapFeatures = new MapFeatures();
    public MapValues mapValues = new MapValues();


    // FUNCTIONS
    //==========

    public PCG(MapData i_BaseMapData)
    {
        BaseMapData = i_BaseMapData;

        reinitializeMap();

    }

    private void reinitializeMap()
    {
        map = Mapgen.returnMap(mapValues.totSmallStop, mapValues.totMedStop, mapValues.totLargeStop, mapValues.totTopBot, mapValues.totSizeTwo, mapValues.totSizeFour, mapValues.totLongPieces);
        string mapShape = map.generateText();

        // Debug.Log(BaseMapData.mapStringRaw);

        //Replace all instances of pellets.

        BaseMapData.mapStringRaw = mapShape;

        BaseMapStringRaw = BaseMapData.mapStringRaw.Replace('.', ' ');
        BaseMapStringRaw = BaseMapStringRaw.Replace('o', ' ');
        BaseMapStringRaw = BaseMapStringRaw.Replace('-', '|');
        BaseMapStringRaw = BaseMapStringRaw.Replace('e', '|');

        // Debug.Log(BaseMapData.mapStringRaw);

        BaseMapStringArray = BaseMapStringRaw.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Debug.Log(BaseMapStringArray);


        baseStringBuilderArray = new();
        for (int row = 0; row < BaseMapStringArray.Length; row++)
        {
            baseStringBuilderArray.Add(new StringBuilder(BaseMapStringArray[row]));
        }
    }

    private int StringBuilderIndexOfChar(StringBuilder i_sb, char i_check, int i_start = 0, int i_end=-1)
    {
        if (i_end == -1 || i_end >= i_sb.Length)
            i_end = i_sb.Length - 1;

        for (int i = i_start; i < i_end; i++)
        {
            if (i_sb[i] == i_check)
                return i;
        }

        return -1;
    }

    public string GenerateMap(MapFeatures i_mapFeatures)
    {
        mapFeatures = i_mapFeatures;
        SetMapValueRanges();

        int countPelletsTot = 0;
        int countPelletsReg = 0;
        int countPelletsPower = 0;

        reinitializeMap();

        // This should be a deep copy
        List<StringBuilder> newMapStringBuilderList = BaseMapStringArray.Select(s => new StringBuilder(string.Copy(s))).ToList();
        Debug.Log(BaseMapStringArray);
        // THIS DOES NOT WORK... it's a ref copy 
        //List<StringBuilder> newMapStringBuilderList = new(BaseMapStringArray);

        List<int> potentialRows = Enumerable.Range(1, BaseMapStringArray.Length - 2).ToList();

        char space = ' ';
        while (countPelletsTot < mapValues.totPellets && potentialRows.Count > 0)
        {
            //Grab a row with potential spaces (The first and last rows are all walls)
            int potentialRowIndex = UnityEngine.Random.Range(0, potentialRows.Count-1);
            int row = potentialRows[potentialRowIndex];

            StringBuilder newRow = newMapStringBuilderList[row];

            int col = StringBuilderIndexOfChar(newRow, space, 1);
            //int col = BaseMapStringArray[row].IndexOf(space, 1);

            if (col == -1)
            {
                potentialRows.Remove(row);
                continue;
            }
            
            //borders are walls or warps. Cannot be pellets.
            int randCol = UnityEngine.Random.Range(col, newRow.Length-2);

            Debug.Log($"newRow {newMapStringBuilderList}\n");
            Debug.Log($"col: {col}\n newRow.Length-2: {newRow.Length - 2}");

            while (newRow[randCol] != space)
            {
                randCol++;
                if (randCol >= newRow.Length)
                    randCol = col;
            }

            //Select pellet type
            if (IsTrueRandomPercent(1 - mapFeatures.powerPelletDensity) && countPelletsReg < mapValues.totRegPellets)
            {
                HelperAssignNewValue('.', col, ref newRow, ref countPelletsReg, ref countPelletsTot);
            }
            else
            {
                HelperAssignNewValue('o', col, ref newRow, ref countPelletsPower, ref countPelletsTot);
            }

            newMapStringBuilderList[row] = newRow;

        }

        string returnMapString = String.Join(Environment.NewLine, newMapStringBuilderList);

        

        Debug.Log($"mapFeatures.powerPelletDensity: {mapFeatures.powerPelletDensity}, mapFeatures.powerPelletDensity: {mapFeatures.powerPelletDensity}"); 
        Debug.Log($"countPelletsTot: {countPelletsTot}, countPelletsReg: {countPelletsReg}, countPelletsPower: {countPelletsPower}");
        Debug.Log($"returnMapString {returnMapString}");

        return returnMapString;//.ToString();

        //StringBuilder returnMapString = new StringBuilder();
        //// Set Pellets
        ////------------
        ////pellets cannot be walls. Walls/portals are the borders
        //for (int row = 0; row < BaseMapStringArray.Length; row++) {
        //    StringBuilder tmpRowString = new StringBuilder(BaseMapStringArray[row]);
        //    for (int col = 1; col < mapValues.cols - 1; col++)
        //    {
        //        // Decide if pellet
        //        if (tmpRowString[col] == ' ' 
        //            && countPelletsTot < mapValues.totPellets
        //            && IsTrueRandomPercent(1-mapFeatures.powerPelletDensity)
        //           ) 
        //        {
        //            // Decide type of pellet
        //            if (countPelletsPower >= mapValues.totPowerPellets 
        //                || IsTrueRandomPercent(mapFeatures.totPelletDensity))
        //            {
        //                HelperAssignNewValue('.', col, ref tmpRowString, ref countPelletsReg);
        //            }
        //            else
        //            {
        //                HelperAssignNewValue('o', col, ref tmpRowString, ref countPelletsPower);
        //            }
        //            countPelletsTot++;
        //            if (mapFeatures.isSymetric)
        //                countPelletsTot++;
        //        }
        //    }
        //    returnMapString.Append(tmpRowString + Environment.NewLine);
        //}

        ////string returnMapString = String.Join(Environment.NewLine, BaseMapStringArray);
        //return returnMapString.ToString();
    }


    // HELPER FUNCTIONS
    //-----------------
    private void HelperAssignNewValue(char i_assign, int i_col, ref StringBuilder io_sb, ref int o_counterType, ref int o_counterTot)
    {
        io_sb[i_col] = i_assign;
        o_counterTot++; 
        o_counterType++;
        int symCol = io_sb.Length - 1 - i_col;
        if (mapFeatures.isSymetric && io_sb[symCol] == ' ')
        {
            io_sb[symCol] = i_assign;
            o_counterTot++;
            o_counterType++;
        }
    }


    private bool IsTrueRandomPercent(float i_percent)
    {
        return UnityEngine.Random.Range(0, 1) <= i_percent;
    }

    public MapFeatures CreateRandomizedMapFeatures()
    {
        MapFeatures returnMapFeatures = new MapFeatures();
        returnMapFeatures.totPelletDensity = UnityEngine.Random.Range(0.0f, 1.0f);
        //public Vector2 pelletDensityMinMax;

        // = pow/spaces
        returnMapFeatures.powerPelletDensity = UnityEngine.Random.Range(0.0f, 1.0f);

        returnMapFeatures.rangePower2Power = new Vector2(UnityEngine.Random.Range(1.0f, 1.0f), UnityEngine.Random.Range(1.0f, 1.0f));

        //returnMapFeatures.isSymetric = true;
        returnMapFeatures.isSymetric = UnityEngine.Random.Range(0.0f, 1.0f) <= 0.5f;

        return returnMapFeatures;
    }

    public MapFeatures CreateMapFromPCCSample(Sample sample, Sample powsample, Sample probsample)
    {
        MapFeatures returnMapFeatures = new MapFeatures();

        // tracked
        returnMapFeatures.totPelletDensity = sample.GetSampleValue("pellet_density").Item2.floatVal;

        // tracked
        returnMapFeatures.powerPelletDensity = powsample.GetSampleValue("power_pellets").Item2.floatVal;

        // tracked
        returnMapFeatures.smallStopMapGrowth = (double)probsample.GetSampleValue("small_stop").Item2.floatVal;

        // tracked
        returnMapFeatures.medStopMapGrowth = (double)probsample.GetSampleValue("med_stop").Item2.floatVal;

        // tracked
        returnMapFeatures.larStopMapGrowth = (double)probsample.GetSampleValue("large_stop").Item2.floatVal;

        // tracked
        returnMapFeatures.sizeTwoMapExtend = (double)probsample.GetSampleValue("sizetwo_grow").Item2.floatVal;

        // tracked
        returnMapFeatures.sizeFourMapExtend = (double)probsample.GetSampleValue("sizefour_grow").Item2.floatVal;

        // tracked
        returnMapFeatures.topAndBotMapJoin = (double)probsample.GetSampleValue("topbot_mix").Item2.floatVal;

        // tracked
        returnMapFeatures.maxLongPieces = probsample.GetSampleValue("long_pieces").Item2.intVal;

        // Not tracked -- ignore
        returnMapFeatures.rangePower2Power = new Vector2(UnityEngine.Random.Range(1.0f, 1.0f), UnityEngine.Random.Range(1.0f, 1.0f));

        // Not tracked -- ignore
        returnMapFeatures.isSymetric = UnityEngine.Random.Range(0.0f, 1.0f) <= 0.5f;

        return returnMapFeatures;
    }

    private void SetMapValueRanges()
    {
        mapValues.cols = BaseMapStringArray[0].Length;
        mapValues.rows = BaseMapStringArray.Length;

        //TODO:  add some randomness to this assignment
        mapValues.totPellets = Math.Clamp((int)(MapRules.totSpacesRange[1] * mapFeatures.totPelletDensity), MapRules.totPelletsRange[0], MapRules.totPelletsRange[1]);       
        mapValues.totPowerPellets = Math.Clamp((int)(mapValues.totPellets * mapFeatures.powerPelletDensity), 0, MapRules.totPelletsRange[1]);
        mapValues.totRegPellets = mapValues.totPellets - mapValues.totPowerPellets;

        mapValues.totSmallStop = mapFeatures.smallStopMapGrowth;
        mapValues.totMedStop = mapFeatures.medStopMapGrowth;
        mapValues.totLargeStop = mapFeatures.larStopMapGrowth;
        mapValues.totSizeTwo = mapFeatures.sizeTwoMapExtend;
        mapValues.totSizeFour = mapFeatures.sizeFourMapExtend;
        mapValues.totTopBot = mapFeatures.topAndBotMapJoin;
        mapValues.totLongPieces = mapFeatures.maxLongPieces;

        //mapValues.rangePower2Power.x = mapFeatures.rangePower2Power;
    }



    //// TEMP: extracting space positions to make it easier to draw.
    //public void ExtractEmptyTiles()
    //{ 
    
    
    //}


}
