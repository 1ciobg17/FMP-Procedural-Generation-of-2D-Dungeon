using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    public IntRange XConstraints;
    public IntRange YConstraints;
    public IntRange RoomWidthRange;
    public IntRange RoomHeightRange;
    public GameObject tileContainer;//wall is 0, floor is 1
    //the specific sprite for each tile type currently available
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

	// Use this for initialization
	void Start () 
    {
        allRooms = new List<Room>();
        for(int i=0; i<RoomNumber; i++)
        {
            xPos = XConstraints.Random();
            yPos = YConstraints.Random();
            roomWidth = RoomWidthRange.Random();
            roomHeight = RoomHeightRange.Random();
            tiles = new Tiletype[roomWidth, roomHeight];
            roomCreator.GenerateRoom(roomWidth, roomHeight, xPos, yPos, tiles);
            allRooms.Add(new Room(xPos, yPos, roomWidth, roomHeight, tiles));
        }
        foreach(Room room in allRooms)
        {
            Debug.Log("Room Width: " + room.width + " Height: " + room.height + " X Pos: " + room.originX + " Y Pos: " + room.originY);
        }
        Draw(allRooms[0]);
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
                Vector3 position = new Vector3(-room.width / 2 + i, -room.height / 2 + j);
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
}
