using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCreation : MonoBehaviour {

    //room sizes
    public int width;
    public int height;

    //a loose guideline to fill the room initially, percentage
    [Range(0,100)]
    public int randomFillPercent;
    //region size(groups of tiles of same value
    public int wallThreshHoldSize=5;
    public int floorThreshHoldSize=5;
    //the used gameobject prefab
    public GameObject tileContainer;//wall is 0, floor is 1
    //needed to change the sprite of the prefab
    SpriteRenderer tileSprite;
    //the specific sprite for each tile type currently available
    public Sprite floor;
    public Sprite wall;
    //room array of tiles
    int[,] room;
    //seed used to create a room
    public string seed;
    //the above can be used to add a manual seed or just use one randomly
    public bool useRandomSeed;
    //how many times will the room change until reaching a final result
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
        RandomFillRoom();


        //start iterating with the current room
        for (int i = 0; i < smoothCount; i++)
        {
            SmoothTiles();
        }

        //check for "regions"
        ProcessRoom();

        //for (int i = 1; i < width - 1; i++)
        //{
        //    for (int j = 1; j < height - 1; j++)
        //    {
        //        CheckTileTypes(i, j);
        //    }
        //}

        //add graphical view to the room
         SetUpTile();
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
                if (neighbourWallTiles > 4)
                    room[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    room[x, y] = 0;

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

    void CheckTileTypes(int gridX, int gridY)
    {
        int tileType;
        int percentage = 20;
        string seed=Time.time.ToString();
        System.Random roll = new System.Random(seed.GetHashCode());
        tileType = room[gridX, gridY];

        if(tileType==room[gridX,gridY-1])
        {
            percentage += 20;
        }
        if (tileType == room[gridX, gridY + 1])
        {
            percentage += 20;
        }
        if (tileType == room[gridX - 1, gridY])
        {
            percentage += 20;
        }
        if (tileType == room[gridX + 1, gridY])
        {
            percentage += 20;
        }

        if(roll.Next(0, 100) >= percentage)
        {
            if (tileType == 1)
                room[gridX, gridY] = 0;
            else
                room[gridX, gridY] = 1;
        }
    }

    //initially, fill the room randomly using a seed entered manually or one randomly generated
    void RandomFillRoom()
    {
        if(useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random randomNumberGeneration = new System.Random(seed.GetHashCode());

        for(int i=0; i<width; i++)
        {
            for(int j=0; j<height; j++)
            {
                //all the tiles at the edge of the room are walls
                if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                {
                    room[i, j] = 0;
                }
                //all the other tiles roll for a type
                else
                {
                    room[i, j] = (randomNumberGeneration.Next(0, 100) < randomFillPercent) ? 1 : 0;
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
    bool IsInRoomRange(int x, int y)
    {
        //check if the tile is inside the room bounds
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
