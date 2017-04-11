using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class RoomCreation : MonoBehaviour
{
    //room sizes
    public int width;
    public int height;
    //room starting coordinates
    public int startX = 0;
    public int startY = 0;

    //a loose guideline to fill the room, percentage only
    //[Range(0, 100)]
    public IntRange chanceToChange = new IntRange(45, 60);
    //cell death happens at lower neighbour limit
    public int cellDeath = 4;
    //cell birth happens at X number of neighbours
    public int cellBirth = 3;
    //region size(groups of tiles of same value
    public int wallThreshHoldSize = 5;
    public int floorThreshHoldSize = 5;
    //tile changing threshhold
    public int tileThreshHold = 4;
    //passage way sizes
    public int passageWaySize = 1;
    //room array of tiles
    //[Range(0, 10)]
    public IntRange smoothCount = new IntRange(1, 3);
    public bool debugLines = false;
    //the used gameobject prefab
    public GameObject tileContainer;//wall is 0, floor is 1
    //current chance
    int currentChangeOfChange=0;
    //the specific sprite for each tile type currently available
    public Sprite floor;
    public Sprite wall;
    public Sprite connecter;

    //room generating function
    public void GenerateRoom(int W, int H, Tiletype[,] roomArray,ref List<Region> regionList)
    {
        bool check = false;

        width = W;
        height = H;
        startX = 0;
        startY = 0;

        //fill the room with tiles
        RandomPercentFill(roomArray);

        check = RoomCheck(roomArray);

        if (!check)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    roomArray[i, j] = Tiletype.Wall;
                }
            }

            GenerateRoom(W, H, roomArray, ref regionList);
            return;
        }

        //start iterating with the current room
        for (int i = 0; i < RandomlyPick(smoothCount.m_Min, smoothCount.m_Max); i++)
        {
            SmoothTiles(roomArray);
        }

        //check for "regions"
        ProcessRoom(roomArray,ref regionList);
    }

    //system need to be able to connect rooms, it can be specified if there should be connectivity within the room
    //default of accessibility is false but the system uses true for connectivity to be achieved
    //pass the surviving rooms list and what type of connectivity should the room achieve
    void ConnectClosestRegions(List<Region> allRegions, Tiletype[,] roomArray, bool forceAccessibilityFromMainRoom = false)
    {
        //used to further connect rooms from their own lists
        //list of regions that are not accessible from the main room
        List<Region> regionListA = new List<Region>();
        //list of regions that are accessible from the main room
        List<Region> regionListB = new List<Region>();

        //if connectivity is the target
        if (forceAccessibilityFromMainRoom)
        {
            //for each region in all of the surviving regions
            foreach (Region region in allRegions)
            {
                //if region can be accessed from the main room
                if (region.isAccessibleFromMainRegion)
                {
                    //then add it to the list of same regions
                    regionListB.Add(region);
                }
                else
                {
                    //else add it to the list of rooms without accessability
                    regionListA.Add(region);
                }
            }
        }//if connectivity is not the target
        else
        {
            //just leave the entire list
            regionListA = allRegions;
            regionListB = allRegions;
        }

        //best distance between rooms
        int bestDistance = 0;
        //best two tiles, one belonging to each region, to connect from
        Position bestTileA = new Position();
        Position bestTileB = new Position();
        //best closest region to connect from
        Region bestRegionA = new Region();
        Region bestRegionB = new Region();
        //if a connection is found, signal the system
        bool possibleConnectionFound = false;

        //for each room in the list of regions that are not connected to the main region
        foreach (Region regionA in regionListA)
        {
            //check if accessability is required
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                //if the room already has a connection
                if (regionA.connectedRegions.Count > 0)
                {
                    //skip
                    continue;
                }
            }

            //for the rooms in the list
            foreach (Region regionB in regionListB)
            {
                //cases that should be skipped
                if (regionA == regionB || regionA.IsConnected(regionB))
                {
                    continue;
                }

                //verify their edge tiles 1 by 1
                for (int tileIndexA = 0; tileIndexA < regionA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < regionB.edgeTiles.Count; tileIndexB++)
                    {
                        //get the tiles
                        Position tileA = regionA.edgeTiles[tileIndexA];
                        Position tileB = regionB.edgeTiles[tileIndexB];
                        //calculate the distance between them
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        //find the best distance from existing one or just make a connection if none has been found yet
                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            //add best cases 
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRegionA = regionA;
                            bestRegionB = regionB;
                        }
                    }
                }
            }
            //if accesability is unneeded
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRegionA, bestRegionB, bestTileA, bestTileB, roomArray);
            }
        }

        //we create a passage only we are forcing accessability from the main region and only when connection is found
        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            //create the passage and pass regions + tiles to connect from
            CreatePassage(bestRegionA, bestRegionB, bestTileA, bestTileB, roomArray);
            ConnectClosestRegions(allRegions, roomArray, true);
        }

        //if no accesability is needed, just connect closest rooms
        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRegions(allRegions, roomArray, true);
        }
    }

    void CreatePassage(Region regionA, Region regionB, Position tileA, Position tileB, Tiletype[,] roomArray)
    {
        //connect the two regions
        Region.ConnectRegions(regionA, regionB);
        if (debugLines)
        {
            Debug.DrawLine(PosToWorldPoint(tileA), PosToWorldPoint(tileB), Color.green, 1);
        }

        //get the line between the two tiles in order to determine what tiles in between them need to have their type changed to create corridors
        List<Position> line = GetLine(tileA, tileB);

        //each point in the line
        foreach (Position pos in line)
        {
            //draw a circle, with a radius
            DrawCircle(pos, passageWaySize, roomArray);
        }
    }

    void CreatePassage(Region regionA, Region regionB, Position tileA, Position tileB, Tiletype[,] roomArray, List<Passage> passageList)
    {
        //connect the two regions
        Region.ConnectRegions(regionA, regionB);

        //get the line between the two tiles in order to determine what tiles in between them need to have their type changed to create corridors
        List<Position> line = GetLine(tileA, tileB);

        //each point in the line
        foreach (Position pos in line)
        {
            //draw a circle, with a radius
            DrawCircle(pos, passageWaySize, roomArray);
        }
    }

    //having the tile and a radius
    void DrawCircle(Position pos, int radius, Tiletype[,] roomArray)
    {
        //for how big the radius should be in both X and Y
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (i * i + j * j <= radius * radius)
                {
                    //change the tile type of both the current tile and the neighbouring tiles(inside the radius)
                    int x = pos.tileX + i;
                    int y = pos.tileY + j;
                    if (IsInRoomRange(x, y))
                    {
                        //create the corridor
                        roomArray[x, y] = Tiletype.Floor;
                    }
                }
            }
        }
    }

    //need to find out what tiles are under the debug line so the corridor can be created
    //method takes the start tile and end tile position
    public List<Position> GetLine(Position from, Position to)
    {
        //list of all tile coordinates that are under the line
        List<Position> line = new List<Position>();

        //using straight line equation we need the following variables
        int x = from.tileX;
        int y = from.tileY;

        //delta x 
        int dx = to.tileX - from.tileX;
        //delta y
        int dy = to.tileY - from.tileY;

        //first of all, it is assumed that dx>=dy 
        bool inverted = false;
        //incrementation for X, integer needed
        int step = Math.Sign(dx);
        //change the Y value, if dx>=dy, using the following
        int gradientStep = Math.Sign(dy);

        //using these values we determine how the line is drawn(horizontall,y vertically, if X is further down the array then Y or other cases
        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        //if this is true, then they are inverted
        if (longest < shortest)
        {
            inverted = true;
            //change actual values to be representative
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            //also change the incrementation steps for Y
            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        //longest of the two divided by 2, later used in the loop to determine most points
        int gradientAccumulation = longest / 2;

        for (int i = 0; i < longest; i++)
        {
            //add point at location
            line.Add(new Position(x, y));

            //if inverted
            if (inverted)
            {
                //then Y increases and point will be with a new y location
                y += step;
            }
            else
            {
                //the opposite
                x += step;
            }

            //increase accumulation
            gradientAccumulation += shortest;

            //if >= than the longest
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    //increase the X location
                    x += gradientStep;
                }
                else
                {
                    //the opposite
                    y += gradientStep;
                }
                //now reduce accumulation
                gradientAccumulation -= longest;
            }
        }

        //return the entire list of tiles
        return line;
    }

    Vector3 PosToWorldPoint(Position tile)
    {
        //simple calculation to get distance, no SQRT in order to avoid memory usage
        return new Vector3(-width / 2 + tile.tileX, -height / 2 + tile.tileY);
    }

    //get all the existing regions(groups of tiles), by checking for similar tiletypes, uses GetRegionTiles
    List<List<Position>> GetRegions(Tiletype tileType, Tiletype[,] roomArray)
    {
        //create a list of lists of regions
        List<List<Position>> regions = new List<List<Position>>();
        //room array to flag tiles for being checked beforehand, 1 is checked, 0 is unchecked
        int[,] roomFlags = new int[width, height];

        //for each tile in the room
        for (int x = startX; x < width - 1; x++)
        {
            for (int y = startY; y < height - 1; y++)
            {
                //check if the tile hasn't been checked beforehand, and tile is of similar type with the given value(walls or floors)
                if (roomFlags[x, y] == 0 && roomArray[x, y] == tileType)
                {
                    //check in the vicinity of the current tile to check if there are any similar tiles of same type near its vicinity then create a region
                    List<Position> newRegion = GetRegionTiles(x, y, roomArray);
                    //add the region to the list
                    regions.Add(newRegion);

                    //flag all tiles in the added region as being checked once
                    foreach (Position tile in newRegion)
                    {
                        roomFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        //return the list of lists
        return regions;
    }

    //this functions gets tiles coordinates and checks around the tile for similar tiletypes
    List<Position> GetRegionTiles(int startX, int startY, Tiletype[,] roomArray)
    {
        //create a list of tiles
        List<Position> tiles = new List<Position>();
        //create the room array of tiles in order to flag already checked tiles
        int[,] roomFlags = new int[width, height];
        //acquire tiletype of the tile at the given array coordinates
        Tiletype tileType = roomArray[startX, startY];

        //create a queue of tile coordinates
        Queue<Position> queue = new Queue<Position>();
        //add the current tile at the given coordinates to the queue
        queue.Enqueue(new Position(startX, startY));
        //also flag the tile as already being checked once
        roomFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            //acquire a tile from the queue
            Position tile = queue.Dequeue();
            //add it to the list
            tiles.Add(tile);

            //check horizontally and vertically around the tile for similar tile types
            for (int i = tile.tileX - 1; i <= tile.tileX + 1; i++)
            {
                for (int j = tile.tileY - 1; j <= tile.tileY + 1; j++)
                {
                    //if the tile is inside the room constraints and the tile is on the same row or column
                    if (IsInRoomRange(i, j) && (j == tile.tileY || i == tile.tileX))
                    {
                        //and if the tile has not been checked before and is of similar type
                        if (roomFlags[i, j] == 0 && roomArray[i, j] == tileType)
                        {
                            //flag it
                            roomFlags[i, j] = 1;
                            //add it to the queue
                            queue.Enqueue(new Position(i, j));
                        }
                    }
                }
            }
        }

        //return the list of tiles
        return tiles;
    }

    //this function uses GetRegions to remove regions of certain type and size
    void ProcessRoom(Tiletype[,] roomArray,ref List<Region> regionList)
    {
        //get the wall regions in the current room
        List<List<Position>> wallRegions = GetRegions(Tiletype.Wall, roomArray);
        //a list of surviving regions will be created
        List<Region> survivingRegions = new List<Region>();

        //pass through each region
        foreach (List<Position> wallRegion in wallRegions)
        {
            //if the region has a number of tiles smaller than the target threshhold
            if (wallRegion.Count < wallThreshHoldSize)
            {
                //change tile type so it fits with the tiles around the region
                foreach (Position tile in wallRegion)
                {
                    //walls to floors
                    roomArray[tile.tileX, tile.tileY] = Tiletype.Floor;
                }
            }
        }

        //same as above but from floors to walls check
        List<List<Position>> floorRegions = GetRegions(Tiletype.Floor, roomArray);

        foreach (List<Position> floorRegion in floorRegions)
        {
            if (floorRegion.Count < floorThreshHoldSize)
            {
                foreach (Position tile in floorRegion)
                {
                    roomArray[tile.tileX, tile.tileY] = 0;
                }
            }
            else
            {
                //if the region is not too small and survived the culling, add it to the list
                survivingRegions.Add(new Region(floorRegion, roomArray));
            }
        }

        //only if there are any existing regions
        if (survivingRegions.Count > 0)
        {
            //sort them so the first one will be the biggest region
            survivingRegions.Sort();
            //biggest region is the main region
            survivingRegions[0].isMainRegion = true;
            //set this to work with the rest of the code
            survivingRegions[0].isAccessibleFromMainRegion = true;

            //start connecting regions available in the list
            ConnectClosestRegions(survivingRegions, roomArray);
        }

        regionList = survivingRegions;
    }

    void SmoothTiles(Tiletype[,] roomArray)
    {
        //pass through each tile in the room
        for (int x = startX + 1; x < width - 1; x++)
        {
            for (int y = startY + 1; y < height - 1; y++)
            {
                //check for surrounding walls for the tiles
                int neighbourWallTiles = GetSurroundingWallCount(x, y, roomArray);

                //depending on how many walls are around the tile, tiletype may be changed
                //if a cell is a wall type but does not have enough neighbours
                if (roomArray[x, y] == (Tiletype)1)
                {
                    if (neighbourWallTiles < cellDeath)
                    {
                        roomArray[x, y] = 0;
                    }
                }
                else
                {
                    if (neighbourWallTiles > cellBirth)
                    {
                        roomArray[x, y] = (Tiletype)1;
                    }
                    else
                    {
                        roomArray[x, y] = 0;
                    }
                }

            }
        }
    }

    //function used mainly for smoothing, requires starting coordinates of tile to check around
    int GetSurroundingWallCount(int gridX, int gridY, Tiletype[,] roomArray)
    {
        //wall count, initially 0
        int wallCount = 0;
        //check around the tile
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                //if the tile is in the room bounds
                if (IsInRoomRange(neighbourX, neighbourY))
                {
                    //and the tile is not the same as the initial tile being checked around of
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        //wall count increases
                        wallCount += (int)roomArray[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        //return the number of wall type tiles
        return wallCount;
    }

    void RandomPercentFill(Tiletype[,] roomArray)
    {
        System.Random randomNumberGeneration = new System.Random();
        currentChangeOfChange = RandomlyPick(chanceToChange.m_Min, chanceToChange.m_Max);

        for (int i = startX; i < width; i++)
        {
            for (int j = startY; j < height; j++)
            {
                if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                {
                    roomArray[i, j] = 0;
                }
                else
                {
                    roomArray[i, j] = (randomNumberGeneration.Next(0, 100) < currentChangeOfChange) ? (Tiletype)1 : 0;
                }
            }
        }
    }

    ////give each tile type its respective sprite, black for walls, red for floors
    void SetUpTile(Tiletype[,] roomArray)
    {
        //if the room ahs been set up
        if (roomArray != null)
        {
            //for each tile
            for (int i = startX; i < width; i++)
            {
                for (int j = startY; j < height; j++)
                {
                    //get position of the tile by calculating it
                    Vector3 position = new Vector3(-width / 2 + i, -height / 2 + j);
                    if (roomArray[i, j] == Tiletype.Floor)
                    {
                        tileContainer.GetComponent<SpriteRenderer>().sprite = floor;
                        tileContainer.gameObject.tag = "Tile";
                        tileContainer.gameObject.name = "Floor";
                        GameObject.Instantiate(tileContainer, position, Quaternion.identity);
                    }
                    else
                    {
                        if (roomArray[i, j] == Tiletype.Wall)
                        {
                            tileContainer.GetComponent<SpriteRenderer>().sprite = wall;
                            tileContainer.gameObject.tag = "Tile";
                            tileContainer.gameObject.name = "Wall";
                            GameObject.Instantiate(tileContainer, position, Quaternion.identity);
                        }
                        else
                        {
                            if (roomArray[i, j] == Tiletype.Test)
                            {
                                tileContainer.GetComponent<SpriteRenderer>().sprite = connecter;
                                tileContainer.gameObject.tag = "Tile";
                                tileContainer.gameObject.name = "Test";
                                GameObject.Instantiate(tileContainer, position, Quaternion.identity);
                            }
                        }
                    }
                }
            }
        }
    }

    //function created as this check is being used more and more often
    public bool IsInRoomRange(int x, int y)
    {
        //check if the tile is inside the room bounds
        return x >= startX && x < width && y >= startY && y < height;
    }

    int RandomlyPick(int min, int max)
    {
        int result = 0;
        result = UnityEngine.Random.Range(min, max);
        return result;
    }

    bool RoomCheck(Tiletype[,] roomArray)
    {
        int floorCount = 0;
        int roomArea = width * height - (2 * height + 2 * (width - 4));
        int percentageToCover = currentChangeOfChange * roomArea / 100;
        percentageToCover = (int)Mathf.Round(percentageToCover);

        for(int i=0; i<width; i++)
        {
            for(int j=0; j<height; j++)
            {
                if(roomArray[i,j]==Tiletype.Floor)
                {
                    floorCount++;
                }
            }
        }

        if(floorCount>=percentageToCover)
        {
            return true;
        }

        return false;
    }
}
