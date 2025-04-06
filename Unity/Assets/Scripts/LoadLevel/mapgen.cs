using System;
using System.Linq;
using System.Collections.Generic;

class Mapgen
{
    private const int UP = 0;
    private const int RIGHT = 1;
    private const int DOWN = 2;
    private const int LEFT = 3;

    private class Cell
    {
        public int x;
        public int y;
        public bool filled;
        public bool[]? connect;
        public Cell[]? next;
        public int group;
        public bool isRaiseHeightCandidate;
        public bool isShrinkWidthCandidate;
        public bool raiseHeight;
        public bool shrinkWidth;
        public int final_x;
        public int final_y;
        public int final_h;
        public int final_w;
        public bool isEdgeTunnelCandidate;
        public bool isVoidTunnelCandidate;
        public bool isSingleDeadEndCandidate;
        public int singleDeadEndDir;
        public bool isDoubleDeadEndCandidate;
        public bool topTunnel;
        public bool isJoinCandidate;
    }

    private static List<Cell> cells = new List<Cell>();
    private static Dictionary<int, int> tallRows = new Dictionary<int, int>();
    private static Dictionary<int, int> narrowCols = new Dictionary<int, int>();

    private const int rows = 9;
    private const int cols = 5;

    static Random rnd = new Random();

    public static Map returnMap(double smallStop, double medStop, double largeStop, double topBot, double sizeTwo, double sizeFour, int maxPiece)
    {
        genRandom(smallStop, medStop, largeStop, topBot, sizeTwo, sizeFour, maxPiece);
        Map map = new Map(28, 36, getTiles());
        return map;
    }

    public static int getRandomInt(int min, int max)
    {
        return rnd.Next(min, max + 1);
    }

    public static void shuffle<T>(List<T> list)
    {
        int len = list.Count;
        for (int i = 0; i < len; i++)
        {
            int j = getRandomInt(0, len - 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public static T? randomElement<T>(List<T> list) where T : class
    {
        int len = list.Count;
        if (len > 0)
        {
            return list[getRandomInt(0, len - 1)];
        }
        return null;
    }

    public static void reset()
    {
        // initialize cells
        cells = new List<Cell>();
        for (int i = 0; i < rows * cols; i++)
        {
            cells.Add(new Cell
            {
                x = i % cols,
                y = i / cols,
                filled = false,
                connect = new bool[4],
                next = new Cell[4],
                group = 0,
                isRaiseHeightCandidate = false,
                isShrinkWidthCandidate = false,
                raiseHeight = false,
                shrinkWidth = false,
                final_x = 0,
                final_y = 0,
                final_h = 0,
                final_w = 0,
                isEdgeTunnelCandidate = false,
                isVoidTunnelCandidate = false,
                isSingleDeadEndCandidate = false,
                singleDeadEndDir = -1,
                isDoubleDeadEndCandidate = false,
                topTunnel = false,
                isJoinCandidate = false,
            });
        }

        // allow each cell to refer to surrounding cells by direction
        for (int i = 0; i < rows * cols; i++)
        {
            var c = cells[i];
            if (c.x > 0)
                c.next[LEFT] = cells[i - 1];
            if (c.x < cols - 1)
                c.next[RIGHT] = cells[i + 1];
            if (c.y > 0)
                c.next[UP] = cells[i - cols];
            if (c.y < rows - 1)
                c.next[DOWN] = cells[i + cols];
        }

        // define the ghost home square
        // Code seems to work fine
        int index = 3 * cols;
        var cell = cells[index];
        cell.filled = true;
        cell.connect[LEFT] = cell.connect[RIGHT] = cell.connect[DOWN] = true;

        index++;
        cell = cells[index];
        cell.filled = true;
        cell.connect[LEFT] = cell.connect[DOWN] = true;

        index += cols - 1;
        cell = cells[index];
        cell.filled = true;
        cell.connect[LEFT] = cell.connect[UP] = cell.connect[RIGHT] = true;

        index++;
        cell = cells[index];
        cell.filled = true;
        cell.connect[UP] = cell.connect[LEFT] = true;
    }

    public static void genRandom(double smallStop, double medStop, double largeStop, double topBot, double sizeTwo, double sizeFour, int maxPiece)
    {
        List<Cell> getLeftMostEmptyCells()
        {
            List<Cell> leftCells = new List<Cell>();
            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Cell c = cells[x + y * cols];
                    if (!c.filled)
                    {
                        leftCells.Add(c);
                    }
                }

                if (leftCells.Count > 0)
                {
                    break;
                }
            }
            return leftCells;
        }

        bool isOpenCell(Cell cell, int i, int prevDir = -1, int size = 0)
        {
            // prevent wall from going through starting position
            if ((cell.y == 6 && cell.x == 0 && i == DOWN) ||
                (cell.y == 7 && cell.x == 0 && i == UP))
            {
                return false;
            }

            // prevent long straight pieces of length 3
            if (size == 2 && (i == prevDir || (i + 2) % 4 == prevDir))
            {
                return false;
            }

            // examine an adjacent empty cell
            if (cell.next[i] != null && !cell.next[i].filled)
            {
                // only open if the cell to the left of it is filled
                if (cell.next[i].next[LEFT] != null && !cell.next[i].next[LEFT].filled)
                {
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        (List<int> openCells, int numOpenCells) getOpenCells(Cell cell, int prevDir, int size)
        {
            List<int> openCells = new List<int>();
            int numOpenCells = 0;
            for (int i = 0; i < 4; i++)
            {
                if (isOpenCell(cell, i, prevDir, size))
                {
                    openCells.Add(i);
                    numOpenCells++;
                }
            }
            return (openCells, numOpenCells);
        }

        void connectCell(Cell cell, int dir)
        {
            cell.connect[dir] = true;
            cell.next[dir].connect[(dir + 2) % 4] = true;
            if (cell.x == 0 && dir == RIGHT)
            {
                cell.connect[LEFT] = true;
            }
        }

        void gen(double smallStop, double medStop, double largeStop, double topBot, double sizeTwo, double sizeFour, int maxPiece)
        {
            Cell cell;      // cell at the center of growth (open cells are chosen around this cell)
            Cell newCell = null;   // most recent cell filled
            Cell firstCell; // the starting cell of the current group

            List<Cell> openCells;   // list of open cells around the center cell
            int numOpenCells;       // size of openCells

            int dir = -1;    // the most recent direction of growth relative to the center cell
            int i;      // loop control variable used for iterating directions

            int numFilled = 0;  // current count of total cells filled
            int numGroups;      // current count of cell groups created
            int size;           // current number of cells in the current group
            double[] probStopGrowingAtSize = { // probability of stopping growth at sizes...
                0,      // size 0
                0,      // size 1
                smallStop,   // size 2
                medStop,    // size 3
                largeStop,   // size 4
                1       // size 5
            };

            // A single cell group of size 1 is allowed at each row at y=0 and y=rows-1,
            // so keep count of those created.
            Dictionary<int, int> singleCount = new Dictionary<int, int>();
            singleCount[0] = singleCount[rows - 1] = 0;
            double probTopAndBotSingleCellJoin = topBot;

            // A count and limit of the number long pieces (i.e. an "L" of size 4 or "T" of size 5)
            int longPieces = 0;
            int maxLongPieces = maxPiece;
            double probExtendAtSize2 = sizeTwo;
            double probExtendAtSize3or4 = sizeFour;

            void fillCell(Cell cellIn)
            {
                cellIn.filled = true;
                cellIn.group = numGroups;
            }

            for (numGroups = 0; ; numGroups++)
            {
                // find all the leftmost empty cells
                openCells = getLeftMostEmptyCells();

                // stop add pieces if there are no more empty cells.
                numOpenCells = openCells.Count;
                if (numOpenCells == 0)
                {
                    break;
                }

                // choose the center cell to be a random open cell, and fill it.
                cell = openCells[getRandomInt(0, numOpenCells - 1)];
                firstCell = cell;
                fillCell(cell);

                // randomly allow one single-cell piece on the top or bottom of the map.
                if (cell.x < cols - 1 && singleCount.ContainsKey(cell.y) && rnd.NextDouble() <= probTopAndBotSingleCellJoin)
                {
                    if (singleCount[cell.y] == 0)
                    {
                        cell.connect[cell.y == 0 ? UP : DOWN] = true;
                        singleCount[cell.y]++;
                        continue;
                    }
                }

                // number of cells in this contiguous group
                size = 1;

                if (cell.x == cols - 1)
                {
                    // if the first cell is at the right edge, then don't grow it.
                    cell.connect[RIGHT] = true;
                    cell.isRaiseHeightCandidate = true;
                }
                else
                {
                    // only allow the piece to grow to 5 cells at most.
                    while (size < 5)
                    {

                        bool stop = false;

                        if (size == 2)
                        {
                            // With a horizontal 2-cell group, try to turn it into a 4-cell "L" group.
                            // This is done here because this case cannot be reached when a piece has already grown to size 3.
                            Cell c = firstCell;
                            if (c.x > 0 && c.connect[RIGHT] && c.next[RIGHT] != null && c.next[RIGHT].next[RIGHT] != null)
                            {
                                if (longPieces < maxLongPieces && rnd.NextDouble() <= probExtendAtSize2)
                                {

                                    c = c.next[RIGHT].next[RIGHT];
                                    Dictionary<int, bool> dirs = new Dictionary<int, bool>();
                                    if (isOpenCell(c, UP))
                                    {
                                        dirs[UP] = true;
                                    }
                                    if (isOpenCell(c, DOWN))
                                    {
                                        dirs[DOWN] = true;
                                    }

                                    if (dirs.ContainsKey(UP) && dirs.ContainsKey(DOWN))
                                    {
                                        if (getRandomInt(0, 1) == 0)
                                        {
                                            i = UP;
                                        }
                                        else
                                        {
                                            i = DOWN;
                                        }
                                    }
                                    else if (dirs.ContainsKey(UP))
                                    {
                                        i = UP;
                                    }
                                    else if (dirs.ContainsKey(DOWN))
                                    {
                                        i = DOWN;
                                    }
                                    else
                                    {
                                        i = -1;
                                    }

                                    if (i != -1)
                                    {
                                        connectCell(c, LEFT);
                                        fillCell(c);
                                        connectCell(c, i);
                                        fillCell(c.next[i]);
                                        longPieces++;
                                        size += 2;
                                        stop = true;
                                    }
                                }
                            }
                        }

                        if (!stop)
                        {
                            // find available open adjacent cells.
                            var result = getOpenCells(cell, dir, size);
                            List<int> openCellList = result.openCells;
                            numOpenCells = result.numOpenCells;

                            // if no open cells found from center point, then use the last cell as the new center
                            // but only do this if we are of length 2 to prevent numerous short pieces.
                            // then recalculate the open adjacent cells.
                            if (numOpenCells == 0 && size == 2)
                            {
                                cell = newCell;
                                result = getOpenCells(cell, dir, size);
                                openCellList = result.openCells;
                                numOpenCells = result.numOpenCells;
                            }

                            // no more adjacent cells, so stop growing this piece.
                            if (numOpenCells == 0)
                            {
                                stop = true;
                            }
                            else
                            {
                                // choose a random valid direction to grow.
                                dir = openCellList[getRandomInt(0, numOpenCells - 1)];
                                newCell = cell.next[dir];

                                // connect the cell to the new cell.
                                connectCell(cell, dir);

                                // fill the cell
                                fillCell(newCell);

                                // increase the size count of this piece.
                                size++;

                                // don't let center pieces grow past 3 cells
                                if (firstCell.x == 0 && size == 3)
                                {
                                    stop = true;
                                }

                                // Use a probability to determine when to stop growing the piece.
                                if (rnd.NextDouble() <= probStopGrowingAtSize[size])
                                {
                                    stop = true;
                                }
                            }
                        }

                        // Close the piece.
                        if (stop)
                        {
                            Cell c;
                            if (size == 1)
                            {
                                // This is provably impossible because this loop is never entered with size==1.
                            }
                            else if (size == 2)
                            {

                                // With a vertical 2-cell group, attach to the right wall if adjacent.
                                c = firstCell;
                                if (c.x == cols - 1)
                                {

                                    // select the top cell
                                    if (c.connect[UP])
                                    {
                                        c = c.next[UP];
                                    }
                                    c.connect[RIGHT] = c.next[DOWN].connect[RIGHT] = true;
                                }

                            }
                            else if (size == 3 || size == 4)
                            {

                                // Try to extend group to have a long leg
                                if (longPieces < maxLongPieces && firstCell.x > 0 && rnd.NextDouble() <= probExtendAtSize3or4)
                                {
                                    List<int> dirs = new List<int>();
                                    int dirsLength = 0;
                                    for (i = 0; i < 4; i++)
                                    {
                                        if (cell.connect[i] && isOpenCell(cell.next[i], i))
                                        {
                                            dirs.Add(i);
                                            dirsLength++;
                                        }
                                    }
                                    if (dirsLength > 0)
                                    {
                                        i = dirs[getRandomInt(0, dirsLength - 1)];
                                        c = cell.next[i];
                                        connectCell(c, i);
                                        fillCell(c.next[i]);
                                        longPieces++;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }
            setResizeCandidates();
        }

        void setResizeCandidates()
        {
            Cell c, c2;
            bool[] q, q2;
            double x, y;
            for (int i = 0; i < rows * cols; i++)
            {
                c = cells[i];
                x = i % cols;
                y = i / cols;

                // determine if it has flexible height

                //
                // |_|
                //
                // or
                //  _
                // | |
                //
                q = c.connect;
                if ((c.x == 0 || !q[LEFT]) &&
                    (c.x == cols - 1 || !q[RIGHT]) &&
                    q[UP] != q[DOWN])
                {
                    c.isRaiseHeightCandidate = true;
                }

                //  _ _
                // |_ _|
                //
                c2 = c.next[RIGHT];
                if (c2 != null)
                {
                    q2 = c2.connect;
                    if (((c.x == 0 || !q[LEFT]) && !q[UP] && !q[DOWN]) &&
                        ((c2.x == cols - 1 || !q2[RIGHT]) && !q2[UP] && !q2[DOWN])
                        )
                    {
                        c.isRaiseHeightCandidate = c2.isRaiseHeightCandidate = true;
                    }
                }

                // determine if it has flexible width

                // if cell is on the right edge with an opening to the right
                if (c.x == cols - 1 && q[RIGHT])
                {
                    c.isShrinkWidthCandidate = true;
                }

                //  _
                // |_
                // 
                // or
                //  _
                //  _|
                //
                if ((c.y == 0 || !q[UP]) &&
                    (c.y == rows - 1 || !q[DOWN]) &&
                    q[LEFT] != q[RIGHT])
                {
                    c.isShrinkWidthCandidate = true;
                }

            }
        }

        // Identify if a cell is the center of a cross.
        bool cellIsCrossCenter(Cell c)
        {
            return c.connect[UP] && c.connect[RIGHT] && c.connect[DOWN] && c.connect[LEFT];
        }

        bool chooseNarrowCols()
        {

            bool canShrinkWidth(int x, int y)
            {

                // Can cause no more tight turns.
                if (y == rows - 1)
                {
                    return true;
                }

                // get the right-hand-side bound
                Cell c = null, c2 = null;
                for (int x0 = x; x0 < cols; x0++)
                {
                    c = cells[x0 + y * cols];
                    c2 = c.next[DOWN];
                    if ((!c.connect[RIGHT] || cellIsCrossCenter(c)) &&
                        (!c2.connect[RIGHT] || cellIsCrossCenter(c2)))
                    {
                        break;
                    }
                }

                // build candidate list
                List<Cell> candidates = new List<Cell>();
                int numCandidates = 0;
                // Based on the original code, I think this is trying to be a reassignment
                while (c2 != null)
                {
                    if (c2.isShrinkWidthCandidate)
                    {
                        candidates.Add(c2);
                        numCandidates++;
                    }

                    // cannot proceed further without causing irreconcilable tight turns
                    if ((!c2.connect[LEFT] || cellIsCrossCenter(c2)) &&
                        (!c2.next[UP].connect[LEFT] || cellIsCrossCenter(c2.next[UP])))
                    {
                        break;
                    }
                    c2 = c2.next[LEFT];
                }
                shuffle(candidates);

                for (int i = 0; i < numCandidates; i++)
                {
                    c2 = candidates[i];
                    if (canShrinkWidth(c2.x, c2.y))
                    {
                        c2.shrinkWidth = true;
                        narrowCols[c2.y] = c2.x;
                        return true;
                    }
                }

                return false;
            }
            ;

            Cell c;
            for (int x = cols - 1; x >= 0; x--)
            {
                c = cells[x];
                if (c.isShrinkWidthCandidate && canShrinkWidth(x, 0))
                {
                    c.shrinkWidth = true;
                    narrowCols[c.y] = c.x;
                    return true;
                }
            }

            return false;
        }

        bool chooseTallRows()
        {

            bool canRaiseHeight(int x, int y)
            {

                // Can cause no more tight turns.
                if (x == cols - 1)
                {
                    return true;
                }

                // find the first cell below that will create too tight a turn on the right
                Cell c = null, c2 = null;
                for (int y0 = y; y0 >= 0; y0--)
                {
                    c = cells[x + y0 * cols];
                    c2 = c.next[RIGHT];
                    if ((!c.connect[UP] || cellIsCrossCenter(c)) &&
                        (!c2.connect[UP] || cellIsCrossCenter(c2)))
                    {
                        break;
                    }
                }

                // Proceed from the right cell upwards, looking for a cell that can be raised.
                List<Cell> candidates = new List<Cell>();
                int numCandidates = 0;
                while (c2 != null)
                {
                    if (c2.isRaiseHeightCandidate)
                    {
                        candidates.Add(c2);
                        numCandidates++;
                    }

                    // cannot proceed further without causing irreconcilable tight turns
                    if ((!c2.connect[DOWN] || cellIsCrossCenter(c2)) &&
                        (!c2.next[LEFT].connect[DOWN] || cellIsCrossCenter(c2.next[LEFT])))
                    {
                        break;
                    }
                    c2 = c2.next[DOWN];
                }
                shuffle(candidates);

                for (int i = 0; i < numCandidates; i++)
                {
                    c2 = candidates[i];
                    if (canRaiseHeight(c2.x, c2.y))
                    {
                        c2.raiseHeight = true;
                        tallRows[c2.x] = c2.y;
                        return true;
                    }
                }

                return false;
            }
            ;

            // From the top left, examine cells below until hitting top of ghost house.
            // A raisable cell must be found before the ghost house.
            Cell c;
            for (int y = 0; y < 3; y++)
            {
                c = cells[y * cols];
                if (c.isRaiseHeightCandidate && canRaiseHeight(0, y))
                {
                    c.raiseHeight = true;
                    tallRows[c.x] = c.y;
                    return true;
                }
            }

            return false;
        }

        // This is a function to detect impurities in the map that have no heuristic implemented to avoid it yet in gen().
        bool isDesirable()
        {

            // ensure a solid top right corner
            Cell c = cells[4];
            if (c.connect[UP] || c.connect[RIGHT])
            {
                return false;
            }

            // ensure a solid bottom right corner
            c = cells[rows * cols - 1];
            if (c.connect[DOWN] || c.connect[RIGHT])
            {
                return false;
            }

            // ensure there are no two stacked/side-by-side 2-cell pieces.
            bool isHori(int x, int y)
            {
                bool[] q1 = cells[x + y * cols].connect;
                bool[] q2 = cells[x + 1 + y * cols].connect;
                return !q1[UP] && !q1[DOWN] && (x == 0 || !q1[LEFT]) && q1[RIGHT] &&
                    !q2[UP] && !q2[DOWN] && q2[LEFT] && !q2[RIGHT];
            }
            bool isVert(int x, int y)
            {
                bool[] q1 = cells[x + y * cols].connect;
                bool[] q2 = cells[x + (y + 1) * cols].connect;
                if (x == cols - 1)
                {
                    // special case (we can consider two single cells as vertical at the right edge)
                    return !q1[LEFT] && !q1[UP] && !q1[DOWN] &&
                        !q2[LEFT] && !q2[UP] && !q2[DOWN];
                }
                return !q1[LEFT] && !q1[RIGHT] && !q1[UP] && q1[DOWN] &&
                    !q2[LEFT] && !q2[RIGHT] && q2[UP] && !q2[DOWN];
            }
            int g;
            for (int y = 0; y < rows - 1; y++)
            {
                for (int x = 0; x < cols - 1; x++)
                {
                    if ((isHori(x, y) && isHori(x, y + 1)) ||
                        (isVert(x, y) && isVert(x + 1, y)))
                    {

                        // don't allow them in the middle because they'll be two large when reflected.
                        if (x == 0)
                        {
                            return false;
                        }

                        // Join the four cells to create a square.
                        cells[x + y * cols].connect[DOWN] = true;
                        cells[x + y * cols].connect[RIGHT] = true;
                        g = cells[x + y * cols].group;

                        cells[x + 1 + y * cols].connect[DOWN] = true;
                        cells[x + 1 + y * cols].connect[LEFT] = true;
                        cells[x + 1 + y * cols].group = g;

                        cells[x + (y + 1) * cols].connect[UP] = true;
                        cells[x + (y + 1) * cols].connect[RIGHT] = true;
                        cells[x + (y + 1) * cols].group = g;

                        cells[x + 1 + (y + 1) * cols].connect[UP] = true;
                        cells[x + 1 + (y + 1) * cols].connect[LEFT] = true;
                        cells[x + 1 + (y + 1) * cols].group = g;
                    }
                }
            }

            if (!chooseTallRows())
            {
                return false;
            }

            if (!chooseNarrowCols())
            {
                return false;
            }

            return true;
        }
        ;

        // set the final position and size of each cell when upscaling the simple model to actual size
        void setUpScaleCoords()
        {
            Cell c = null;
            for (int i = 0; i < rows * cols; i++)
            {
                c = cells[i];
                c.final_x = c.x * 3;
                if (narrowCols[c.y] < c.x)
                {
                    c.final_x--;
                }
                c.final_y = c.y * 3;
                if (tallRows[c.x] < c.y)
                {
                    c.final_y++;
                }
                c.final_w = c.shrinkWidth ? 2 : 3;
                c.final_h = c.raiseHeight ? 4 : 3;
            }
        }

        bool createTunnels()
        {

            // declare candidates
            List<Cell> singleDeadEndCells = new List<Cell>();
            List<Cell> topSingleDeadEndCells = new List<Cell>();
            List<Cell> botSingleDeadEndCells = new List<Cell>();

            List<Cell> voidTunnelCells = new List<Cell>();
            List<Cell> topVoidTunnelCells = new List<Cell>();
            List<Cell> botVoidTunnelCells = new List<Cell>();

            List<Cell> edgeTunnelCells = new List<Cell>();
            List<Cell> topEdgeTunnelCells = new List<Cell>();
            List<Cell> botEdgeTunnelCells = new List<Cell>();

            List<Cell> doubleDeadEndCells = new List<Cell>();

            int numTunnelsCreated = 0;

            // prepare candidates
            Cell c = null;
            for (int y = 0; y < rows; y++)
            {
                c = cells[cols - 1 + y * cols];
                if (c.connect[UP])
                {
                    continue;
                }
                if (c.y > 1 && c.y < rows - 2)
                {
                    c.isEdgeTunnelCandidate = true;
                    edgeTunnelCells.Add(c);
                    if (c.y <= 2)
                    {
                        topEdgeTunnelCells.Add(c);
                    }
                    else if (c.y >= 5)
                    {
                        botEdgeTunnelCells.Add(c);
                    }
                }
                bool upDead = (c.next[UP] == null || c.next[UP].connect[RIGHT]);
                bool downDead = (c.next[DOWN] == null || c.next[DOWN].connect[RIGHT]);
                if (c.connect[RIGHT])
                {
                    if (upDead)
                    {
                        c.isVoidTunnelCandidate = true;
                        voidTunnelCells.Add(c);
                        if (c.y <= 2)
                        {
                            topVoidTunnelCells.Add(c);
                        }
                        else if (c.y >= 6)
                        {
                            botVoidTunnelCells.Add(c);
                        }
                    }
                }
                else
                {
                    if (c.connect[DOWN])
                    {
                        continue;
                    }
                    if (upDead != downDead)
                    {
                        if (!c.raiseHeight && y < rows - 1 && !c.next[LEFT].connect[LEFT])
                        {
                            singleDeadEndCells.Add(c);
                            c.isSingleDeadEndCandidate = true;
                            c.singleDeadEndDir = upDead ? UP : DOWN;
                            var offset = upDead ? 1 : 0;
                            if (c.y <= 1 + offset)
                            {
                                topSingleDeadEndCells.Add(c);
                            }
                            else if (c.y >= 5 + offset)
                            {
                                botSingleDeadEndCells.Add(c);
                            }
                        }
                    }
                    else if (upDead && downDead)
                    {
                        if (y > 0 && y < rows - 1)
                        {
                            if (c.next[LEFT].connect[UP] && c.next[LEFT].connect[DOWN])
                            {
                                c.isDoubleDeadEndCandidate = true;
                                if (c.y >= 2 && c.y <= 5)
                                {
                                    doubleDeadEndCells.Add(c);
                                }
                            }
                        }
                    }
                }
            }

            // choose tunnels from candidates
            int numTunnelsDesired = rnd.NextDouble() <= 0.45 ? 2 : 1;
            c = null;
            void selectSingleDeadEnd(Cell c1)
            {
                c1.connect[RIGHT] = true;
                if (c1.singleDeadEndDir == UP)
                {
                    c1.topTunnel = true;
                }
                else
                {
                    c1.next[DOWN].topTunnel = true;
                }
            }
            if (numTunnelsDesired == 1)
            {
                c = randomElement(voidTunnelCells);
                if (c != null)
                {
                    c.topTunnel = true;
                }
                else
                {
                    c = randomElement(singleDeadEndCells);
                    if (c != null)
                    {
                        selectSingleDeadEnd(c);
                    }
                    else
                    {
                        c = randomElement(edgeTunnelCells);
                        if (c != null)
                        {
                            c.topTunnel = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            else if (numTunnelsDesired == 2)
            {
                c = randomElement(doubleDeadEndCells);
                if (c != null)
                {
                    c.connect[RIGHT] = true;
                    c.topTunnel = true;
                    c.next[DOWN].topTunnel = true;
                }
                else
                {
                    numTunnelsCreated = 1;
                    c = randomElement(topVoidTunnelCells);
                    if (c != null)
                    {
                        c.topTunnel = true;
                    }
                    else
                    {
                        c = randomElement(topSingleDeadEndCells);
                        if (c != null)
                        {
                            selectSingleDeadEnd(c);
                        }
                        else
                        {
                            c = randomElement(topEdgeTunnelCells);
                            if (c != null)
                            {
                                c.topTunnel = true;
                            }
                            else
                            {
                                // could not find a top tunnel opening
                                numTunnelsCreated = 0;
                            }
                        }
                    }

                    c = randomElement(botVoidTunnelCells);
                    if (c != null)
                    {
                        c.topTunnel = true;
                    }
                    else
                    {
                        c = randomElement(botSingleDeadEndCells);
                        if (c != null)
                        {
                            selectSingleDeadEnd(c);
                        }
                        else
                        {
                            c = randomElement(botEdgeTunnelCells);
                            if (c != null)
                            {
                                c.topTunnel = true;
                            }
                            else
                            {
                                // could not find a bottom tunnel opening
                                if (numTunnelsCreated == 0)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            // don't allow a horizontal path to cut straight through a map (through tunnels)
            bool exit;
            int topy;
            for (int y = 0; y < rows; y++)
            {
                c = cells[cols - 1 + y * cols];
                if (c.topTunnel)
                {
                    exit = true;
                    topy = c.final_y;
                    while (c.next[LEFT] != null)
                    {
                        c = c.next[LEFT];
                        if (!c.connect[UP] && c.final_y == topy)
                        {
                            continue;
                        }
                        else
                        {
                            exit = false;
                            break;
                        }
                    }
                    if (exit)
                    {
                        return false;
                    }
                }
            }

            // clear unused void tunnels (dead ends)
            int len = voidTunnelCells.Count;

            void replaceGroup(int oldg, int newg)
            {
                Cell c = null;
                for (int i = 0; i < rows * cols; i++)
                {
                    c = cells[i];
                    if (c.group == oldg)
                    {
                        c.group = newg;
                    }
                }
            }
            for (int i = 0; i < len; i++)
            {
                c = voidTunnelCells[i];
                if (!c.topTunnel)
                {
                    replaceGroup(c.group, c.next[UP].group);
                    c.connect[UP] = true;
                    c.next[UP].connect[DOWN] = true;
                }
            }

            return true;
        }

        void joinWalls()
        {

            // randomly join wall pieces to the boundary to increase difficulty

            Cell c = null;

            // join cells to the top boundary
            for (int x = 0; x < cols; x++)
            {
                c = cells[x];
                if (!c.connect[LEFT] && !c.connect[RIGHT] && !c.connect[UP] &&
                    (!c.connect[DOWN] || !c.next[DOWN].connect[DOWN]))
                {

                    // ensure it will not create a dead-end
                    if ((c.next[LEFT] == null || !c.next[LEFT].connect[UP]) &&
                        (c.next[RIGHT] != null && !c.next[RIGHT].connect[UP]))
                    {

                        // prevent connecting very large piece
                        if (!(c.next[DOWN] != null && c.next[DOWN].connect[RIGHT] && c.next[DOWN].next[RIGHT].connect[RIGHT]))
                        {
                            c.isJoinCandidate = true;
                            if (rnd.NextDouble() <= 0.25)
                            {
                                c.connect[UP] = true;
                            }
                        }
                    }
                }
            }

            // join cells to the bottom boundary
            for (int x = 0; x < cols; x++)
            {
                c = cells[x + (rows - 1) * cols];
                if (!c.connect[LEFT] && !c.connect[RIGHT] && !c.connect[DOWN] &&
                    (!c.connect[UP] || !c.next[UP].connect[UP]))
                {

                    // ensure it will not creat a dead-end
                    if ((c.next[LEFT] == null || !c.next[LEFT].connect[DOWN]) &&
                        (c.next[RIGHT] != null && !c.next[RIGHT].connect[DOWN]))
                    {

                        // prevent connecting very large piece
                        if (!(c.next[UP] != null && c.next[UP].connect[RIGHT] && c.next[UP].next[RIGHT].connect[RIGHT]))
                        {
                            c.isJoinCandidate = true;
                            if (rnd.NextDouble() <= 0.25)
                            {
                                c.connect[DOWN] = true;
                            }
                        }
                    }
                }
            }

            // join cells to the right boundary
            Cell c2 = null;
            for (int y = 1; y < rows - 1; y++)
            {
                c = cells[cols - 1 + y * cols];
                if (c.raiseHeight)
                {
                    continue;
                }
                if (!c.connect[RIGHT] && !c.connect[UP] && !c.connect[DOWN] &&
                    !c.next[UP].connect[RIGHT] && !c.next[DOWN].connect[RIGHT])
                {
                    if (c.connect[LEFT])
                    {
                        c2 = c.next[LEFT];
                        if (!c2.connect[UP] && !c2.connect[DOWN] && !c2.connect[LEFT])
                        {
                            c.isJoinCandidate = true;
                            if (rnd.NextDouble() <= 0.5)
                            {
                                c.connect[RIGHT] = true;
                            }
                        }
                    }
                }
            }
        }

        // try to generate a valid map, and keep count of tries.
        int genCount = 0;
        while (true)
        {
            reset();
            gen(smallStop, medStop, largeStop, topBot, sizeTwo, sizeFour, maxPiece);
            genCount++;
            if (!isDesirable())
            {
                continue;
            }

            setUpScaleCoords();
            joinWalls();
            if (!createTunnels())
            {
                continue;
            }

            break;
        }

    }

    public static string getTiles()
    {

        List<char> tiles = new List<char>(); // each is a character indicating a wall(|), path(.), or blank(_).
        List<Cell> tileCells = new List<Cell>(); // maps each tile to a specific cell of our simple map
        int subrows = rows * 3 + 1 + 3;
        int subcols = cols * 3 - 1 + 2;

        int midcols = subcols - 2;
        int fullcols = (subcols - 2) * 2;

        // getter and setter for tiles (ensures vertical symmetry axis)
        void setTile(int x, int y, char v)
        {
            if (x < 0 || x > subcols - 1 || y < 0 || y > subrows - 1)
            {
                return;
            }
            x -= 2;
            tiles[midcols + x + y * fullcols] = v;
            tiles[midcols - 1 - x + y * fullcols] = v;
        }
        char getTile(int x, int y)
        {
            if (x < 0 || x > subcols - 1 || y < 0 || y > subrows - 1)
            {
                return '?';
            }
            x -= 2;
            return tiles[midcols + x + y * fullcols];
        }

        // getter and setter for tile cells
        void setTileCell(int x, int y, Cell cell)
        {
            if (x < 0 || x > subcols - 1 || y < 0 || y > subrows - 1)
            {
                return;
            }
            x -= 2;
            tileCells[x + y * subcols] = cell;
        }
        Cell getTileCell(int x, int y)
        {
            if (x < 0 || x > subcols - 1 || y < 0 || y > subrows - 1)
            {
                return null;
            }
            x -= 2;
            int index = x + y * subcols;
            if (index < 0)
            {
                return null;
            }
            return tileCells[index];
        }
        ;

        // initialize tiles
        for (int i = 0; i < subrows * fullcols; i++)
        {
            tiles.Add('_');
        }
        for (int i = 0; i < subrows * subcols; i++)
        {
            tileCells.Add(null);
        }

        // set tile cells
        Cell c = null;
        int x, y, w, h;
        for (int i = 0; i < rows * cols; i++)
        {
            c = cells[i];
            for (int x0 = 0; x0 < c.final_w; x0++)
            {
                for (int y0 = 0; y0 < c.final_h; y0++)
                {
                    setTileCell(c.final_x + x0, c.final_y + 1 + y0, c);
                }
            }
        }

        // set path tiles
        Cell cl = null, cu = null;
        for (y = 0; y < subrows; y++)
        {
            for (x = 0; x < subcols; x++)
            {
                c = getTileCell(x, y); // cell
                cl = getTileCell(x - 1, y); // left cell
                cu = getTileCell(x, y - 1); // up cell

                if (c != null)
                {
                    // inside map
                    if ((cl != null && c.group != cl.group) || // at vertical boundary
                        (cu != null && c.group != cu.group) || // at horizontal boundary
                        cu == null && !c.connect[UP])
                    { // at top boundary
                        setTile(x, y, '.');
                    }
                }
                else
                {
                    // outside map
                    if ((cl != null && (!cl.connect[RIGHT] || getTile(x - 1, y) == '.')) || // at right boundary
                        (cu != null && (!cu.connect[DOWN] || getTile(x, y - 1) == '.')))
                    { // at bottom boundary
                        setTile(x, y, '.');
                    }
                }

                // at corner connecting two paths
                if (getTile(x - 1, y) == '.' && getTile(x, y - 1) == '.' && getTile(x - 1, y - 1) == '_')
                {
                    setTile(x, y, '.');
                }
            }
        }

        y = 0;
        // extend tunnels
        for (c = cells[cols - 1]; c != null; c = c.next[DOWN])
        {
            if (c.topTunnel)
            {
                y = c.final_y + 1;
                setTile(subcols - 1, y, '.');
                setTile(subcols - 2, y, '.');
            }
        }

        // fill in walls
        for (y = 0; y < subrows; y++)
        {
            for (x = 0; x < subcols; x++)
            {
                // any blank tile that shares a vertex with a path tile should be a wall tile
                if (getTile(x, y) != '.' && (getTile(x - 1, y) == '.' || getTile(x, y - 1) == '.' || getTile(x + 1, y) == '.' || getTile(x, y + 1) == '.' ||
                    getTile(x - 1, y - 1) == '.' || getTile(x + 1, y - 1) == '.' || getTile(x + 1, y + 1) == '.' || getTile(x - 1, y + 1) == '.'))
                {
                    setTile(x, y, '|');
                }
            }
        }

        // create the ghost door
        setTile(2, 12, '-');

        // set energizers
        (int, int) getTopEnergizerRange()
        {
            int miny = 0;
            int maxy = subrows / 2;
            int x = subcols - 2;
            for (int y = 2; y < maxy; y++)
            {
                if (getTile(x, y) == '.' && getTile(x, y + 1) == '.')
                {
                    miny = y + 1;
                    break;
                }
            }
            maxy = Math.Min(maxy, miny + 7);
            for (int y = miny + 1; y < maxy; y++)
            {
                if (getTile(x - 1, y) == '.')
                {
                    maxy = y - 1;
                    break;
                }
            }
            return (miny, maxy);
        }
        (int, int) getBotEnergizerRange()
        {
            int miny = subrows / 2;
            int maxy = 0;
            var x = subcols - 2;
            for (int y = subrows - 3; y >= miny; y--)
            {
                if (getTile(x, y) == '.' && getTile(x, y + 1) == '.')
                {
                    maxy = y;
                    break;
                }
            }
            miny = Math.Max(miny, maxy - 7);
            for (int y = maxy - 1; y > miny; y--)
            {
                if (getTile(x - 1, y) == '.')
                {
                    miny = y + 1;
                    break;
                }
            }
            return (miny, maxy);
        }
        int x2 = subcols - 2;
        int y2;
        (int, int) range;

        range = getTopEnergizerRange();
        y2 = getRandomInt(range.Item1, range.Item2);
        setTile(x2, y2, 'o');

        range = getBotEnergizerRange();
        y2 = getRandomInt(range.Item1, range.Item2);
        setTile(x2, y2, 'o');

        // erase pellets in the tunnels
        void eraseUntilIntersection(int x, int y)
        {
            List<(int, int)> adj = new List<(int, int)>();
            while (true)
            {
                adj = new List<(int, int)>();
                if (getTile(x - 1, y) == '.')
                {
                    adj.Add((x - 1, y));
                }
                if (getTile(x + 1, y) == '.')
                {
                    adj.Add((x + 1, y));
                }
                if (getTile(x, y - 1) == '.')
                {
                    adj.Add((x, y - 1));
                }
                if (getTile(x, y + 1) == '.')
                {
                    adj.Add((x, y + 1));
                }
                if (adj.Count == 1)
                {
                    setTile(x, y, ' ');
                    x = adj[0].Item1;
                    y = adj[0].Item2;
                }
                else
                {
                    break;
                }
            }
        }
        x = subcols - 1;
        for (y = 0; y < subrows; y++)
        {
            if (getTile(x, y) == '.')
            {
                eraseUntilIntersection(x, y);
            }
        }

        // erase pellets on starting position
        setTile(1, subrows - 8, ' ');

        // erase pellets around the ghost house
        int j;
        y = 0;
        for (int i = 0; i < 7; i++)
        {

            // erase pellets from bottom of the ghost house proceeding down until
            // reaching a pellet tile that isn't surround by walls
            // on the left and right
            y = subrows - 14;
            setTile(i, y, ' ');
            j = 1;
            while (getTile(i, y + j) == '.' &&
                    getTile(i - 1, y + j) == '|' &&
                    getTile(i + 1, y + j) == '|')
            {
                setTile(i, y + j, ' ');
                j++;
            }

            // erase pellets from top of the ghost house proceeding up until
            // reaching a pellet tile that isn't surround by walls
            // on the left and right
            y = subrows - 20;
            setTile(i, y, ' ');
            j = 1;
            while (getTile(i, y - j) == '.' &&
                    getTile(i - 1, y - j) == '|' &&
                    getTile(i + 1, y - j) == '|')
            {
                setTile(i, y - j, ' ');
                j++;
            }
        }
        // erase pellets on the side of the ghost house
        for (int i = 0; i < 7; i++)
        {

            // erase pellets from side of the ghost house proceeding right until
            // reaching a pellet tile that isn't surround by walls
            // on the top and bottom.
            int x3 = 6;
            y = subrows - 14 - i;
            setTile(x3, y, ' ');
            j = 1;
            while (getTile(x3 + j, y) == '.' &&
                    getTile(x3 + j, y - 1) == '|' &&
                    getTile(x3 + j, y + 1) == '|')
            {
                setTile(x3 + j, y, ' ');
                j++;
            }
        }

        // return a tile string
        return (
            "____________________________" +
            "____________________________" +
            "____________________________" +
            string.Join("", tiles) +
            "____________________________" +
            "____________________________");
    }


}