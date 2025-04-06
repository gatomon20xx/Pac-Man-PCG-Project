using System;
using System.Collections.Generic;

class Map
{
    // direction enums (in clockwise order)
    private const int DIR_UP = 0;
    private const int DIR_RIGHT = 1;
    private const int DIR_DOWN = 2;
    private const int DIR_LEFT = 3;

    private int numCols;
    private int numRows;
    private int numTiles;
    private int widthPixels;
    private int heightPixels;
    private string tiles;
    private char[] currentTiles;

    // get direction enum from a direction vector
    public static int getEnumFromDir((int, int) dir)
    {
        if (dir.Item1 == -1) return DIR_LEFT;
        if (dir.Item1 == 1) return DIR_RIGHT;
        if (dir.Item2 == -1) return DIR_UP;
        if (dir.Item2 == 1) return DIR_DOWN;
        return -1;
    }

    // set direction vector from a direction enum
    public static (int, int) setDirFromEnum((int?, int?) dir, int dirEnum)
    {
        if (dirEnum == DIR_UP) { return (0, -1); }
        else if (dirEnum == DIR_RIGHT) { return (1, 0); }
        else if (dirEnum == DIR_DOWN) { return (0, 1); }
        else if (dirEnum == DIR_LEFT) { return (-1, 0); }
        else { return (0, 0); }
    }

    // size of a square tile in pixels
    int tileSize = 8;

    // constructor
    public Map(int numCols, int numRows, string tiles)
    {

        // sizes
        this.numCols = numCols;
        this.numRows = numRows;
        this.numTiles = numCols * numRows;
        this.widthPixels = numCols * tileSize;
        this.heightPixels = numRows * tileSize;

        // ascii map
        this.tiles = tiles;

        this.resetCurrent();
        this.parseWalls();
    }

    // reset current tiles
    public void resetCurrent()
    {
        this.currentTiles = this.tiles.ToCharArray(); // create a mutable list copy of an immutable string
    }

    // This is a procedural way to generate original-looking maps from a simple ascii tile
    // map without a spritesheet.
    public void parseWalls()
    {

        Map that = this;

        // a map of wall tiles that already belong to a built path
        Dictionary<int, bool> visited = new Dictionary<int, bool>();

        // we extend the x range to suggest the continuation of the tunnels
        int toIndex(int x, int y)
        {
            if (x >= -2 && x < that.numCols + 2 && y >= 0 && y < that.numRows)
            {
                return (x + 2) + y * (that.numCols + 4);
            }
            return -1;
        }

        // a map of which wall tiles that are not completely surrounded by other wall tiles
        Dictionary<int, bool> edges = new Dictionary<int, bool>();
        int i = 0;
        for (int y = 0; y < this.numRows; y++)
        {
            for (int x = -2; x < this.numCols + 2; x++, i++)
            {
                if (this.getTile(x, y) == '|' &&
                    (this.getTile(x - 1, y) != '|' ||
                    this.getTile(x + 1, y) != '|' ||
                    this.getTile(x, y - 1) != '|' ||
                    this.getTile(x, y + 1) != '|' ||
                    this.getTile(x - 1, y - 1) != '|' ||
                    this.getTile(x - 1, y + 1) != '|' ||
                    this.getTile(x + 1, y - 1) != '|' ||
                    this.getTile(x + 1, y + 1) != '|'))
                {
                    edges[i] = true;
                }
            }
        }

        // walks along edge wall tiles starting at the given index to build a canvas path
        void makePath(int tx, int ty)
        {

            // get initial direction
            (int, int) dir = (-2, -2);
            int dirEnum = -1;
            if (edges.ContainsKey(toIndex(tx + 1, ty)))
                dirEnum = DIR_RIGHT;
            else if (edges.ContainsKey(toIndex(tx, ty + 1)))
                dirEnum = DIR_DOWN;
            else
                throw new Exception("tile shouldn't be 1x1 at " + tx + "," + ty);
            dir = setDirFromEnum(dir, dirEnum);

            // increment to next tile
            tx += dir.Item1;
            ty += dir.Item2;

            // backup initial location and direction
            int init_tx = tx;
            int init_ty = ty;
            int init_dirEnum = dirEnum;

            List<(int, int)> path = new List<(int, int)> { };
            int pad = 0; // (persists for each call to getStartPoint)
            (int, int) point;
            (int, int) lastPoint;

            bool turn = false, turnAround = false;

            /*

            We employ the 'right-hand rule' by keeping our right hand in contact
            with the wall to outline an individual wall piece.

            Since we parse the tiles in row major order, we will always start
            walking along the wall at the leftmost tile of its topmost row.  We
            then proceed walking to the right.  

            When facing the direction of the walk at each tile, the outline will
            hug the left side of the tile unless there is a walkable tile to the
            left.  In that case, there will be a padding distance applied.
            
            */
            (int, int) getStartPoint(int tx, int ty, int dirEnum)
            {
                (int?, int?) dir = (null, null);
                dir = setDirFromEnum(dir, dirEnum);
                if (!edges.ContainsKey(toIndex(tx + (int)dir.Item2, ty - (int)dir.Item1)))
                    pad = that.isFloorTile(tx + (int)dir.Item2, ty - (int)dir.Item1) ? 5 : 0;
                var px = -tileSize / 2 + pad;
                var py = tileSize / 2;
                var a = dirEnum * Math.PI / 2;
                var c = Math.Cos(a);
                var s = Math.Sin(a);
                return (
                    // the first expression is the rotated point centered at origin
                    // the second expression is to translate it to the tile
                    (int)((px * c - py * s) + (tx + 0.5) * tileSize),
                    (int)((px * s + py * c) + (ty + 0.5) * tileSize)
                );
            }
            while (true)
            {

                visited[toIndex(tx, ty)] = true;

                // determine start point
                point = getStartPoint(tx, ty, dirEnum);

                // update direction
                turn = false;
                turnAround = false;
                if (edges.ContainsKey(toIndex(tx + dir.Item2, ty - dir.Item1)))
                { // turn left
                    dirEnum = (dirEnum + 3) % 4;
                    turn = true;
                }
                else if (edges.ContainsKey(toIndex(tx + dir.Item1, ty + dir.Item2)))
                { // continue straight
                }
                else if (edges.ContainsKey(toIndex(tx - dir.Item2, ty + dir.Item1)))
                { // turn right
                    dirEnum = (dirEnum + 1) % 4;
                    turn = true;
                }
                else
                { // turn around
                    dirEnum = (dirEnum + 2) % 4;
                    turnAround = true;
                }
                dir = setDirFromEnum(dir, dirEnum);

                // commit path point
                path.Add(point);

                // special case for turning around (have to connect more dots manually)
                if (turnAround)
                {
                    path.Add(getStartPoint(tx - dir.Item1, ty - dir.Item2, (dirEnum + 2) % 4));
                    path.Add(getStartPoint(tx, ty, dirEnum));
                }

                // advance to the next wall
                tx += dir.Item1;
                ty += dir.Item2;

                // exit at full cycle
                if (tx == init_tx && ty == init_ty && dirEnum == init_dirEnum)
                {
                    break;
                }
            }
        }

        // iterate through all edges, making a new path after hitting an unvisited wall edge
        i = 0;
        for (int y = 0; y < this.numRows; y++)
            for (int x = -2; x < this.numCols + 2; x++, i++)
            {
                if (edges.ContainsKey(i) && !visited.ContainsKey(i))
                {
                    visited[i] = true;
                    makePath(x, y);
                }
            }
    }

    public int posToIndex(int x, int y)
    {
        if (x >= 0 && x < this.numCols && y >= 0 && y < this.numRows)
            return x + y * this.numCols;
        else
            return 0;
    }
    // retrieves tile character at given coordinate
    // extended to include offscreen tunnel space
    public char? getTile(int x, int y)
    {
        if (x >= 0 && x < this.numCols && y >= 0 && y < this.numRows)
        {
            return this.currentTiles[this.posToIndex(x, y)];
        }

        // extend walls and paths outward for entrances and exits
        if ((x == -1 && this.getTile(x + 1, y) == '|' && (this.isFloorTile(x + 1, y + 1) || this.isFloorTile(x + 1, y - 1))) ||
            (x == this.numCols && this.getTile(x - 1, y) == '|' && (this.isFloorTile(x - 1, y + 1) || this.isFloorTile(x - 1, y - 1))))
        {
            return '|';
        }
        if ((x == -1 && this.isFloorTile(x + 1, y)) ||
            (x == this.numCols && this.isFloorTile(x - 1, y)))
        {
            return ' ';
        }
        return null;
    }

    // determines if the given character is a walkable floor tile
    bool isFloorTileChar(char? tile)
    {
        return tile == ' ' || tile == '.' || tile == 'o';
    }

    // determines if the given tile coordinate has a walkable floor tile
    bool isFloorTile(int x, int y)
    {
        return this.isFloorTileChar(this.getTile(x, y));
    }

    public string generateText()
    {
        int x, y;
        string text = "";
        for (y = 3; y < this.numRows - 2; y++)
        {
            for (x = 0; x < this.numCols; x++)
            {
                var t = this.getTile(x, y);
                if (t == 'o' || t == '.' || t == ' ')
                {
                    if (x == 0 || x == this.numCols - 1)
                    {
                        text = text + 'w';
                    }
                    else
                        text = text + '.';

                }
                else if (t == '|')
                {
                    text = text + '|';
                }
                else
                {
                    text = text + ' ';
                }
                //if (t==' ') {
                //    text = text + ' ';
                //}
            }
            text = text + "\n";
        }
        return text;
    }

}