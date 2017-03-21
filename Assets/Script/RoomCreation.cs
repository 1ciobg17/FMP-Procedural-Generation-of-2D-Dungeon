using System.Collections;
using System.Collections.Generic;
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

        public Position(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

   class Room {
        public List<Position> tiles;
        public List<Position> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;

        public Room() {
        }

        public Room(List<Position> roomTiles, int[,] map) {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Position>();
            foreach (Position tile in tiles) {
                for (int x = tile.tileX-1; x <= tile.tileX+1; x++) {
                    for (int y = tile.tileY-1; y <= tile.tileY+1; y++) {
                        if (x == tile.tileX || y == tile.tileY) {
                            if (map[x,y] == 1) {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB) {
            roomA.connectedRooms.Add (roomB);
            roomB.connectedRooms.Add (roomA);
        }

        public bool IsConnected(Room otherRoom) {
            return connectedRooms.Contains(otherRoom);
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

    void ConnectClosestRooms(List<Room> allRooms)
    {

        int bestDistance = 0;
        Position bestTileA = new Position();
        Position bestTileB = new Position();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in allRooms)
        {
            possibleConnectionFound = false;

            foreach (Room roomB in allRooms)
            {
                if (roomA == roomB)
                {
                    continue;
                }
                if (roomA.IsConnected(roomB))
                {
                    possibleConnectionFound = false;
                    break;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Position tileA = roomA.edgeTiles[tileIndexA];
                        Position tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }
    }

    void CreatePassage(Room roomA, Room roomB, Position tileA, Position tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        Debug.Log("RoomA size in tiles=" + roomA.roomSize);
        Debug.Log("RoomB size in tiles=" + roomB.roomSize);
        Debug.DrawLine(PosToWorldPoint(tileA), PosToWorldPoint(tileB), Color.green, 1);
        Debug.Log("TileB position in array= X:" + tileA.tileX +" Y:"+tileA.tileY);
        Vector3 position = new Vector3(-width / 2 + tileA.tileX, -height / 2 + tileA.tileY);
        tileSprite.sprite = connecter;
        GameObject.Instantiate(tileContainer, position, Quaternion.identity);
        Debug.Log("TileB position in array= X:" + tileB.tileX + " Y:"+ tileB.tileY);
        position = new Vector3(-width / 2 + tileB.tileX, -height / 2 + tileB.tileY);
        GameObject.Instantiate(tileContainer, position, Quaternion.identity);
    }

    Vector3 PosToWorldPoint(Position tile)
    {
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
        List<Room> survivingRegion = new List<Room>();
         
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
                survivingRegion.Add(new Room(floorRegion, room));
            }
        }

        ConnectClosestRooms(survivingRegion);
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
