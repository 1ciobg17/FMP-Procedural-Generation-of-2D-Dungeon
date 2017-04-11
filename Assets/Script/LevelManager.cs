using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    public IntRange XConstraints;
    public IntRange YConstraints;
    public IntRange RoomWidthRange;
    public IntRange RoomHeightRange;
    public IntRange DistanceBetweenRooms;
    public GameObject tileContainer;//wall is 0, floor is 1
    //the specific sprite for each tile type currently available
    Position roomPosition;
    public Sprite floor;
    public Sprite wall;
    public Sprite connecter;
    List<Room> allRooms;
    List<Position> passageTiles;
    List<Region> currentRegions;
    public int RoomNumber = 5;
    public RoomCreation roomCreator;
    int xPos;
    int yPos;
    int roomWidth;
    int roomHeight;
    Tiletype[,] tiles;
    enum Direction //room movement direction
    {
        Up, Down, Left, Right,
    }

	// Use this for initialization
	void Start () 
    {
        int prevX, prevY, prevH, prevW;
        roomPosition = new Position();
        passageTiles = new List<Position>();
        currentRegions = new List<Region>();
        allRooms = new List<Room>();
        prevX = xPos = XConstraints.Random();
        prevY = yPos = YConstraints.Random();
        prevW = roomWidth = RoomWidthRange.Random();
        prevH = roomHeight = RoomHeightRange.Random();
        tiles = new Tiletype[roomWidth, roomHeight];
        roomCreator.GenerateRoom(roomWidth, roomHeight, tiles, ref currentRegions);
        allRooms.Add(new Room(xPos, yPos, roomWidth, roomHeight, tiles, currentRegions));
        for (int i=1; i<RoomNumber; i++)
        {
            roomWidth = RoomWidthRange.Random();
            roomHeight = RoomHeightRange.Random();
            tiles = new Tiletype[roomWidth, roomHeight];
            roomCreator.GenerateRoom(roomWidth, roomHeight, tiles,ref currentRegions);
            allRooms.Add(new Room(roomPosition.tileX, roomPosition.tileY, roomWidth, roomHeight, tiles, currentRegions));
            roomPosition = RandomRoom(prevX, prevY, prevH, prevW, allRooms[i-1], allRooms[i]);
            allRooms[i].SetPos(roomPosition.tileX, roomPosition.tileY);
            allRooms[i].originX = roomPosition.tileX;
            allRooms[i].originY = roomPosition.tileY;
            prevX = roomPosition.tileX;
            prevY = roomPosition.tileY;
        }
        DrawPassages();
        foreach (Room room in allRooms)
        {
            Draw(room);

        }
    }
	
	// Update is called once per frame
    void Update()
    {

    }

    void Draw(Room room)
    {
        for (int i = 0; i < room.height; i++)
        {
            for (int j = 0; j < room.width; j++)
            {
                //get position of the tile by calculating it
                Vector3 position = new Vector3(room.originX + i, room.originY + j);
                if (room.roomTiles[i, j] == Tiletype.Floor)
                {
                    tileContainer.GetComponent<SpriteRenderer>().sprite = floor;
                    tileContainer.gameObject.tag = "Tile";
                    tileContainer.gameObject.name = "Floor";
                    GameObject.Instantiate(tileContainer, position, Quaternion.identity);
                }
                else
                {
                    if (room.roomTiles[i, j] == Tiletype.Wall)
                    {
                        tileContainer.GetComponent<SpriteRenderer>().sprite = wall;
                        tileContainer.gameObject.tag = "Tile";
                        tileContainer.gameObject.name = "Wall";
                        GameObject.Instantiate(tileContainer, position, Quaternion.identity);
                    }
                }
            }
        }
    }

    void DrawPassages()
    {
        for(int i=0; i<passageTiles.Count; i++)
        {
            Vector3 position = new Vector3(passageTiles[i].tileX, passageTiles[i].tileY);
            tileContainer.GetComponent<SpriteRenderer>().sprite = connecter;
            tileContainer.gameObject.name = "Passage";
            GameObject.Instantiate(tileContainer, position, Quaternion.identity);
        }
    }

    Position RandomRoom(int prevX, int prevY, int prevH, int prevW,Room prevRoom,Room currRoom)
    {
        int x = prevX;
        int y = prevY;
        int distance = DistanceBetweenRooms.Random();
        Position roomLocation = new Position();
        Direction random = (Direction)Random.Range(0, 4);
        switch (random)
        {
            case Direction.Up:
                if (prevRoom.surroundings.IsTopOccupied())
                {
                    goto case Direction.Down;
                }
                else
                {
                    currRoom.surroundings.BottomIsOccupied();
                    y += prevH + distance;
                }
                break;
            case Direction.Down:
                if (prevRoom.surroundings.IsBottomOccupied())
                {
                    goto case Direction.Left;
                }
                else
                {
                    prevRoom.surroundings.BottomIsOccupied();
                    currRoom.surroundings.TopIsOccupied();
                    y -= (prevH + distance);
                }
                break;
            case Direction.Left:
                if (prevRoom.surroundings.IsLeftOccupied())
                {
                    goto case Direction.Right;
                }
                else
                {
                    prevRoom.surroundings.LeftIsOccupied();
                    currRoom.surroundings.RightIsOccupied();
                    x -= (prevW + distance);
                }
                break;
            case Direction.Right:
                if (prevRoom.surroundings.IsRightOccupied())
                { 
                    RandomRoom(prevX, prevY, prevH, prevW,prevRoom,currRoom);
                    goto case Direction.Up;
                }
                else
                {
                    prevRoom.surroundings.RightIsOccupied();
                    currRoom.surroundings.LeftIsOccupied();
                    x += prevW + distance;
                }
                break;
        }

        roomLocation.tileX = x;
        roomLocation.tileY = y;

        ConnectRegions(prevRoom.regionList, currRoom.regionList, prevX, prevY, roomLocation.tileX, roomLocation.tileY);

        return roomLocation;
    }

    void ConnectRegions(List<Region> regionListA, List<Region> regionListB, int xA, int yA, int xB, int yB)
    {
        int bestDistance = 0;
        int distanceBetweenRooms = 0;
        Position bestTileA = new Position();
        Position bestTileB = new Position();
        bool possibleConnectionFound = false;

        foreach (Region regionA in regionListA)
        {
            foreach (Region regionB in regionListB)
            {
                for(int i=0; i<regionA.edgeTiles.Count; i++)
                {
                    for(int j=0; j<regionB.edgeTiles.Count; j++)
                    {
                        Position tileA = regionA.edgeTiles[i];
                        tileA.tileX = tileA.tileX + xA;
                        tileA.tileY = tileA.tileY + yA;
                        Position tileB = regionB.edgeTiles[j];
                        tileB.tileX = tileB.tileX + xB;
                        tileB.tileY = tileB.tileY + yB;
                        distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            //add best cases 
                            bestTileA = tileA;
                            bestTileB = tileB;
                            possibleConnectionFound = true;
                        }
                    }
                }
            }
        }

        Vector3 position = new Vector3(bestTileA.tileX, bestTileA.tileY);
        tileContainer.GetComponent<SpriteRenderer>().sprite = connecter;
        tileContainer.gameObject.name = "TileA";
        GameObject.Instantiate(tileContainer, position, Quaternion.identity);

        position = new Vector3(bestTileB.tileX, bestTileB.tileY);
        tileContainer.GetComponent<SpriteRenderer>().sprite = connecter;
        tileContainer.gameObject.name = "TileB";
        GameObject.Instantiate(tileContainer, position, Quaternion.identity);

        List<Position> line = roomCreator.GetLine(bestTileA, bestTileB);

        foreach(Position point in line)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i * i + j * j <= 1 * 1)
                    {
                        //change the tile type of both the current tile and the neighbouring tiles(inside the radius)
                        int x = point.tileX + i;
                        int y = point.tileY + j;
                        //create the corridor
                        Position tile = new Position(x, y);
                        passageTiles.Add(tile);
                    }
                }
            }
        }
    }
}
