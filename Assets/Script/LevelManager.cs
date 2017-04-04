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
    List<Room> allRooms;
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
        allRooms = new List<Room>();
        prevX = xPos = XConstraints.Random();
        prevY = yPos = YConstraints.Random();
        prevW = roomWidth = RoomWidthRange.Random();
        prevH = roomHeight = RoomHeightRange.Random();
        tiles = new Tiletype[roomWidth, roomHeight];
        roomCreator.GenerateRoom(roomWidth, roomHeight, tiles);
        allRooms.Add(new Room(xPos, yPos, roomWidth, roomHeight, tiles));
        for (int i=1; i<RoomNumber; i++)
        {
            roomWidth = RoomWidthRange.Random();
            roomHeight = RoomHeightRange.Random();
            tiles = new Tiletype[roomWidth, roomHeight];
            roomCreator.GenerateRoom(roomWidth, roomHeight, tiles);
            allRooms.Add(new Room(roomPosition.tileX, roomPosition.tileY, roomWidth, roomHeight, tiles));
            roomPosition = RandomRoom(prevX, prevY, prevH, prevW, allRooms[i-1], allRooms[i]);
            allRooms[i].SetPos(roomPosition.tileX, roomPosition.tileY);
            allRooms[i].originX = roomPosition.tileX;
            allRooms[i].originY = roomPosition.tileY;
            prevX = roomPosition.tileX;
            prevY = roomPosition.tileY;
        }
        foreach(Room room in allRooms)
        {
            //Debug.Log("Room Width: " + room.width + " Height: " + room.height + " X Pos: " + room.originX + " Y Pos: " + room.originY);
        }
        foreach(Room room in allRooms)
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

    Position RandomRoom(int prevX, int prevY, int prevH, int prevW, Room prevRoom, Room currRoom)
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
                    RandomRoom(prevX, prevY, prevH, prevW, prevRoom, currRoom);
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

        Debug.Log(random+" X: "+x+" Y: "+y);

        return roomLocation;
    }
}
