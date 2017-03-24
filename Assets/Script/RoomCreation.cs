using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class RoomCreation : MonoBehaviour {

    //ADD OVERPOPLIMIT CHECK

    //room sizes
    public int width;
    public int height;

    //a loose guideline to fill the room, percentage only
    [Range(0,100)]
    public int changeToChange=50;
    //cell death happens at lower neighbour limit
    public int cellDeath;
    //cell birth happens at X number of neighbours
    public int cellBirth;
    //region size(groups of tiles of same value
    public int wallThreshHoldSize=5;
    public int floorThreshHoldSize=5;
    //tile changing threshhold
    public int tileThreshHold = 4;
    //passage way sizes
    public int passageWaySize = 1;
    //the used gameobject prefab
    public GameObject tileContainer;//wall is 0, floor is 1
    //needed to change the sprite of the prefab
    SpriteRenderer tileSprite;
    //the specific sprite for each tile type currently available
    public Sprite floor;
    public Sprite wall;
    public Sprite connecter;
    //room array of tiles
    int[,] room;
    [Range(0,10)]
    public int smoothCount;

    //struct for tile position in the room array
    struct Position
    {
        public int tileX;
        public int tileY;

        //the constructor
        public Position(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    //class required to better understand regions when trying to connect them - renamed to Room
   class Room : IComparable<Room> {
        //a list of all the tiles in the region
        public List<Position> tiles;
        //a list of only the tiles at the edge of a room, as to avoid time wasted by checking all tiles for the smallest distance betweeen regions
        public List<Position> edgeTiles;
        //a list of all regions directly connected to one another
        public List<Room> connectedRegions;
        //number of tiles
        public int regionSize;
        //if the region is accessible from the main region, required in order to adress connectivity in the room
        public bool isAccessibleFromMainRegion;
        //if this region is the biggest then it becomes the main region
        public bool isMainRegion;

        //default constructor
        public Room() {
        }

        //public constructor
        public Room(List<Position> regionTiles, int[,] map)
        {
            tiles = regionTiles;
            regionSize = tiles.Count;
            connectedRegions = new List<Room>();

            edgeTiles = new List<Position>();
            foreach (Position tile in tiles)
            {
                for (int x = tile.tileX-1; x <= tile.tileX+1; x++)
                {
                    for (int y = tile.tileY-1; y <= tile.tileY+1; y++)
                    {
                        //only tiles horizontally or vertically positioned, ignoring diagonal tiles
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            //if any neighbouring tile is of wall type then it is an edge tile
                            if (map[x,y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        //set the accessability once it is possible to travel to the main region from the current region
        public void SetAccessibleFromMainRegion()
        {
            if (!isAccessibleFromMainRegion)
            {
                isAccessibleFromMainRegion = true;
                foreach (Room connectedRegion in connectedRegions)
                {
                    //also set all regions connected to the current region that they are accessible
                    connectedRegion.SetAccessibleFromMainRegion();
                }
            }
        }

        //set the accessability and add each room to each others list
        public static void ConnectRegions(Room regionA, Room regionB)
        { 
            if (regionA.isAccessibleFromMainRegion)
            {
                regionB.SetAccessibleFromMainRegion ();
            }
            else 
            if (regionB.isAccessibleFromMainRegion)
            {
                regionA.SetAccessibleFromMainRegion();
            }

            regionA.connectedRegions.Add (regionB);
            regionB.connectedRegions.Add (regionA);
        }

        //check connection with other regions
        public bool IsConnected(Room otherRegion) {
            return connectedRegions.Contains(otherRegion);
        }

        //compare region sizes for the IComparable functionality, later used for sorting
        public int CompareTo(Room otherRegion)
        {
            return otherRegion.regionSize.CompareTo(regionSize);
        }
    }

    void Start()
    {
        //get the sprite renderer component
        tileSprite = tileContainer.GetComponent<SpriteRenderer>();
        //create a room as the scene is played
        GenerateRoom();
    }

    void Update()
    {
        //for testing purposes, SPACEBAR is used to create new rooms
        if(Input.GetKeyDown(KeyCode.Space))
        {
            //destroy all previous tiles
            GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Tile");
            foreach (GameObject go in allObjects)
                Destroy(go);
            //create a new room
            GenerateRoom();
        }
    }

    //room generating function
    void GenerateRoom()
    {
        //initialize the room
        room = new int[width, height];
        //fill the room with tiles
        //RandomFillRoom();
        RandomPercentFill();


        //start iterating with the current room
        for (int i = 0; i < smoothCount; i++)
        {
            //SmoothTiles();
            UpgradedSmoothTiles();
        }

        //check for "regions"
        ProcessRoom();

        //add graphical view to the room
         SetUpTile();
    }

    void ConnectClosestRegions(List<Room> allRegions, bool forceAccessibilityFromMainRoom = false)
    {

        List<Room> regionListA = new List<Room>();
        List<Room> regionListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room region in allRegions)
            {
                if (region.isAccessibleFromMainRegion)
                {
                    regionListB.Add(region);
                }
                else
                {
                    regionListA.Add(region);
                }
            }
        }
        else
        {
            regionListA = allRegions;
            regionListB = allRegions;
        }

        int bestDistance = 0;
        Position bestTileA = new Position();
        Position bestTileB = new Position();
        Room bestRegionA = new Room();
        Room bestRegionB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room regionA in regionListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (regionA.connectedRegions.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room regionB in regionListB)
            {
                if (regionA == regionB || regionA.IsConnected(regionB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < regionA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < regionB.edgeTiles.Count; tileIndexB++)
                    {
                        Position tileA = regionA.edgeTiles[tileIndexA];
                        Position tileB = regionB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRegionA = regionA;
                            bestRegionB = regionB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRegionA, bestRegionB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRegionA, bestRegionB, bestTileA, bestTileB);
            ConnectClosestRegions(allRegions, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRegions(allRegions, true);
        }
    }

    void CreatePassage(Room regionA, Room regionB, Position tileA, Position tileB)
    {
        //connect the two regions
        Room.ConnectRegions(regionA, regionB);
        //debug line code
        //Room.ConnectRooms(roomA, roomB);
        //Debug.Log("RoomA size in tiles=" + roomA.roomSize);
        //Debug.Log("RoomB size in tiles=" + roomB.roomSize);
        Debug.DrawLine(PosToWorldPoint(tileA), PosToWorldPoint(tileB), Color.green, 1);
        //Debug.Log("TileB position in array= X:" + tileA.tileX +" Y:"+tileA.tileY);
        //Vector3 position = new Vector3(-width / 2 + tileA.tileX, -height / 2 + tileA.tileY);
        //tileSprite.sprite = connecter;
        //GameObject.Instantiate(tileContainer, position, Quaternion.identity);
        //Debug.Log("TileB position in array= X:" + tileB.tileX + " Y:"+ tileB.tileY);
        //position = new Vector3(-width / 2 + tileB.tileX, -height / 2 + tileB.tileY);
        //GameObject.Instantiate(tileContainer, position, Quaternion.identity);

        //get the line between the two tiles in order to determine what tiles in between them need to have their type changed to create corridors
        List<Position> line = GetLine(tileA, tileB);
        //each point in the line
        foreach (Position pos in line)
        {
            //draw a circle, with a radius
            DrawCircle(pos, passageWaySize);
        }
    }

    //having the tile and a radius
    void DrawCircle(Position pos, int radius)
    {
        //for how big the radius should be in both X and Y
        for(int i=-radius; i<=radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if(i*i+j*j <= radius*radius)
                {
                    //change the tile type of both the current tile and the neighbouring tiles(inside the radius)
                    int x = pos.tileX + i;
                    int y = pos.tileY + j;
                    if(IsInRoomRange(x, y))
                    {
                        //create the corridor
                        room[x, y] = 1;
                    }
                }
            }
        }
    }

    List<Position> GetLine(Position from, Position to)
    {
        List<Position> line = new List<Position>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if(longest<shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;

        for(int i=0; i<longest; i++)
        {
            line.Add(new Position(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;

            if(gradientAccumulation>=longest)
            {
                if(inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 PosToWorldPoint(Position tile)
    {
        //simple calculation to get distance, no SQRT in order to avoid memory usage
        return new Vector3(-width / 2 + tile.tileX, -height / 2  + tile.tileY);
    }

    //get all the existing regions(groups of tiles), by checking for similar tiletypes, uses GetRegionTiles
    List<List<Position>> GetRegions(int tileType)
    {
        //create a list of lists of regions
        List<List<Position>> regions = new List<List<Position>>();
        //room array to flag tiles for being checked beforehand, 1 is checked, 0 is unchecked
        int[,] roomFlags = new int[width, height];

        //for each tile in the room
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //check if the tile hasn't been checked beforehand, and tile is of similar type with the given value(walls or floors)
                if(roomFlags[x,y]==0 && room[x,y]==tileType)
                {
                    //check in the vicinity of the current tile to check if there are any similar tiles of same type near its vicinity then create a region
                    List<Position> newRegion = GetRegionTiles(x, y);
                    //add the region to the list
                    regions.Add(newRegion);
                    
                    //flag all tiles in the added region as being checked once
                    foreach(Position tile in newRegion)
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
    List<Position> GetRegionTiles(int startX, int startY)
    {
        //create a list of tiles
        List<Position> tiles = new List<Position>();
        //create the room array of tiles in order to flag already checked tiles
        int[,] roomFlags = new int[width, height];
        //acquire tiletype of the tile at the given array coordinates
        int tileType = room[startX, startY];

        //create a queue of tile coordinates
        Queue<Position> queue = new Queue<Position>();
        //add the current tile at the given coordinates to the queue
        queue.Enqueue(new Position(startX, startY));
        //also flag the tile as already being checked once
        roomFlags[startX, startY] = 1;

        while(queue.Count>0)
        {
            //acquire a tile from the queue
            Position tile = queue.Dequeue();
            //add it to the list
            tiles.Add(tile);

            //check horizontally and vertically around the tile for similar tile types
            for(int i=tile.tileX-1; i<=tile.tileX+1; i++)
            {
                for(int j=tile.tileY-1; j<=tile.tileY+1; j++)
                {
                    //if the tile is inside the room constraints and the tile is on the same row or column
                    if(IsInRoomRange(i, j) && (j==tile.tileY || i==tile.tileX))
                    {
                        //and if the tile has not been checked before and is of similar type
                        if(roomFlags[i,j]==0 && room[i,j]==tileType)
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
    void ProcessRoom()
    {
        //get the wall regions in the current room
        List<List<Position>> wallRegions=GetRegions(0);
        //a list of surviving regions will be created
        List<Room> survivingRegions = new List<Room>();
         
        //pass through each region
        foreach(List<Position> wallRegion in wallRegions)
        {
            //if the region has a number of tiles smaller than the target threshhold
            if(wallRegion.Count < wallThreshHoldSize)
            {
                //change tile type so it fits with the tiles around the region
                foreach(Position tile in wallRegion)
                {
                    //walls to floors
                    room[tile.tileX, tile.tileY] = 1;
                }
            }
        }

        //same as above but from floors to walls check
        List<List<Position>> floorRegions = GetRegions(1);

        foreach (List<Position> floorRegion in floorRegions)
        {
            if (floorRegion.Count < floorThreshHoldSize)
            {
                foreach (Position tile in floorRegion)
                {
                    room[tile.tileX, tile.tileY] = 0;
                }
            }
            else
            {
                //if the region is not too small and survived the culling, add it to the list
                survivingRegions.Add(new Room(floorRegion, room));
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
            ConnectClosestRegions(survivingRegions);
        }
    }

    //smoothing function
    void SmoothTiles()
    {
        //pass through each tile in the room
        for (int x = 1; x < width-1; x++)
        {
            for (int y = 1; y < height-1; y++)
            {
                //check for surrounding walls for the tiles
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                //depending on how many walls are around the tile, tiletype may be changed
                if (neighbourWallTiles > tileThreshHold)
                    room[x, y] = 1;
                else if (neighbourWallTiles < tileThreshHold)
                    room[x, y] = 0;

            }
        }
    }

    void UpgradedSmoothTiles()
    {
        //pass through each tile in the room
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                //check for surrounding walls for the tiles
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                //depending on how many walls are around the tile, tiletype may be changed
                //if a cell is a wall type but does not have enough neighbours
                if (room[x, y]==1)
                {
                    if (neighbourWallTiles < cellDeath)
                    {
                        room[x, y] = 0;
                    }
                }
                else
                {
                    if(neighbourWallTiles>cellBirth)
                    {
                        room[x, y] = 1;
                    }
                    else 
                    {
                        room[x, y] = 0;
                    }
                }

            }
        }
    }

    //function used mainly for smoothing, requires starting coordinates of tile to check around
    int GetSurroundingWallCount(int gridX, int gridY)
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
                        wallCount += room[neighbourX, neighbourY];
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

    void RandomPercentFill()
    {
        System.Random randomNumberGeneration=new System.Random();

        for(int i=0; i<width; i++)
        {
            for(int j=0; j<height; j++)
            {
                if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                {
                    room[i, j] = 0;
                }
                else
                {
                    room[i, j] = (randomNumberGeneration.Next(0, 100) < changeToChange) ? 1 : 0;
                }
            }
        }
    }

    //give each tile type its respective sprite, black for walls, red for floors
    void SetUpTile()
    {
        //if the room ahs been set up
        if(room!=null)
        {
            //for each tile
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //get position of the tile by calculating it
                    Vector3 position = new Vector3(-width / 2 + i, -height / 2 + j);
                    if(room[i,j]==1)
                    {
                        tileSprite.sprite = floor;
                        //tileContainer.gameObject.tag = "Floor";
                        GameObject.Instantiate(tileContainer, position, Quaternion.identity);
                    }
                    else
                    {
                        tileSprite.sprite = wall;
                        //tileContainer.gameObject.tag = "Wall";
                        GameObject.Instantiate(tileContainer, position, Quaternion.identity);
                    }
                }
            }
        }
    }

    //function created as this check is being used more and more often
    public bool IsInRoomRange(int x, int y)
    {
        //check if the tile is inside the room bounds
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
