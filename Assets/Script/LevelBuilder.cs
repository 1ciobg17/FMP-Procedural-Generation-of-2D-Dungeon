using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour {

    private Tiletype[,] tiles;

    public int LevelRows=100;
    public int LevelColumns=100;
    public IntRange roomNumber;
    public IntRange WidthRange;
    int roomWidth;
    public IntRange HeightRange;
    int roomHeight;

    int roomStartX;
    int roomStartY;

    GameObject roomBuilder;

    //the used gameobject prefab
    public GameObject tileContainer;//wall is 0, floor is 1
    //the specific sprite for each tile type currently available
    public Sprite floor;
    public Sprite wall;
    public Sprite connecter;

    List<Region> allRegions;
    List<Region> currentRegions;
    List<Room> allRooms;
    

	// Use this for initialization
	void Start () {
        allRegions = new List<Region>();
        currentRegions = new List<Region>();
        SetupTileArray();
        roomBuilder = GameObject.Find("RoomBuilder");
        DetermineRandomValues();
        Debug.Log("Width: " + roomWidth + " Height: " + roomHeight + " RoomX: " + roomStartX + " RoomY: " + roomStartY);
        roomBuilder.GetComponent<RoomCreation>().GenerateRoom(roomWidth, roomHeight, roomStartX, roomStartY, tiles, currentRegions);
        InstantiateTiles();
	}
	
	// Update is called once per frame
	void Update () {
        //for testing purposes, SPACEBAR is used to create new rooms
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //destroy all previous tiles
            GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Tile");
            foreach (GameObject go in allObjects)
                Destroy(go);
            //create a new room
            DetermineRandomValues();
            tiles = new Tiletype[LevelRows, LevelColumns];
            Debug.Log("Width: " + roomWidth + " Height: " + roomHeight + " RoomX: " + roomStartX + " RoomY: " + roomStartY);
            roomBuilder.GetComponent<RoomCreation>().GenerateRoom(roomWidth, roomHeight, roomStartX, roomStartY, tiles, currentRegions);
            Debug.Log(currentRegions.Count);
            InstantiateTiles();
        }

	}

    void SetupTileArray()
    {
        //set array to correct width
        tiles = new Tiletype[LevelRows, LevelColumns];
    }

    void InstantiateTiles()
    {
        for (int i = 0; i < LevelRows; i++)
        {
            for (int j = 0; j < LevelColumns; j++)
            {
                if (tiles[i, j] == Tiletype.Wall)
                {
                    tileContainer.GetComponent<SpriteRenderer>().sprite = wall;
                    //instantiate a wall
                    Instantiate(tileContainer, new Vector3(i, j, 0.0f), Quaternion.identity);
                }
                else
                {
                    if(tiles[i,j]==Tiletype.Floor)
                    {
                        tileContainer.GetComponent<SpriteRenderer>().sprite = floor;
                        //instantiate a wall
                        Instantiate(tileContainer, new Vector3(i, j, 0.0f), Quaternion.identity);
                    }
                }
            }
        }
    }

    void DetermineRandomValues()
    {
        roomStartX = Random.Range(0, LevelColumns);
        roomWidth = WidthRange.Random();
        if(roomStartX-roomWidth>LevelColumns || roomStartX + roomWidth>LevelColumns)
        {
            while(roomStartX-roomWidth>LevelColumns || roomStartX + roomWidth>LevelColumns)
            {
                roomStartX = Random.Range(0, LevelColumns);
            }
        }
        roomStartY = Random.Range(0, LevelRows);
        roomHeight = HeightRange.Random();
        if (roomStartY - roomHeight > LevelRows || roomStartY + roomHeight > LevelRows)
        {
            while (roomStartY - roomHeight > LevelRows || roomStartY + roomHeight > LevelRows)
            {
                roomStartY = Random.Range(0, LevelRows);
            }
        }
    }

    void BuildingProcess()
    {
        //create the first room
        DetermineRandomValues();
        Debug.Log("Width: " + roomWidth + " Height: " + roomHeight + " RoomX: " + roomStartX + " RoomY: " + roomStartY);
        roomBuilder.GetComponent<RoomCreation>().GenerateRoom(roomWidth, roomHeight, roomStartX, roomStartY, tiles, currentRegions);
        //create room here
        //add room to list
        

        InstantiateTiles();
    }
}
