//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class LevelBuilder : MonoBehaviour
//{

//    private Tiletype[,] tiles;

//    public int LevelRows = 100;
//    public int LevelColumns = 100;
//    public IntRange WidthRange;
//    int roomWidth;
//    public IntRange HeightRange;
//    int roomHeight;
//    public int howManyRooms;
//    int roomDistance;

//    int roomStartX;
//    int roomStartY;

//    GameObject roomBuilder;

//    //the used gameobject prefab
//    public GameObject tileContainer;//wall is 0, floor is 1
//    //the specific sprite for each tile type currently available
//    public Sprite floor;
//    public Sprite wall;
//    public Sprite connecter;

//    List<Region> allRegions;
//    List<Region> currentRegions;
//    List<Room> allRooms;


//    // Use this for initialization
//    void Start()
//    {
//        allRooms = new List<Room>();
//        allRegions = new List<Region>();
//        currentRegions = new List<Region>();
//        SetupTileArray();
//        roomBuilder = GameObject.Find("RoomBuilder");
//        for (int i = 0; i < howManyRooms; i++)
//        {
//            DetermineRandomValues();
//            //Debug.Log("Width: " + roomWidth + " Height: " + roomHeight + " RoomX: " + roomStartX + " RoomY: " + roomStartY);
//            roomBuilder.GetComponent<RoomCreation>().GenerateRoom(roomWidth, roomHeight, roomStartX, roomStartY, tiles, currentRegions);
//            allRooms.Add(new Room(roomStartX, roomStartY, roomHeight, roomWidth, currentRegions));
//            currentRegions.Clear();
//        }
//        SurvivalOfTheFittest(allRooms);
//        InstantiateTiles();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        //for testing purposes, SPACEBAR is used to create new rooms
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            allRooms.Clear();
//            //destroy all previous tiles
//            GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Tile");
//            foreach (GameObject go in allObjects)
//                Destroy(go);
//            //create a new room
//            tiles = new Tiletype[LevelRows, LevelColumns];
//            for (int i = 0; i < howManyRooms; i++)
//            {
//                DetermineRandomValues();
//                //Debug.Log("Width: " + roomWidth + " Height: " + roomHeight + " RoomX: " + roomStartX + " RoomY: " + roomStartY);
//                roomBuilder.GetComponent<RoomCreation>().GenerateRoom(roomWidth, roomHeight, roomStartX, roomStartY, tiles, currentRegions);
//                allRooms.Add(new Room(roomStartX, roomStartY, roomHeight, roomWidth, currentRegions));
//                currentRegions.Clear();
//            }
//            SurvivalOfTheFittest(allRooms);
//            InstantiateTiles();
//        }

//    }

//    void SetupTileArray()
//    {
//        //set array to correct width
//        tiles = new Tiletype[LevelRows, LevelColumns];
//    }

//    void InstantiateTiles()
//    {
//        for (int i = 0; i < LevelRows; i++)
//        {
//            for (int j = 0; j < LevelColumns; j++)
//            {
//                if (tiles[i, j] == Tiletype.Wall)
//                {
//                    tileContainer.GetComponent<SpriteRenderer>().sprite = wall;
//                    //instantiate a wall
//                    Instantiate(tileContainer, new Vector3(i, j, 0.0f), Quaternion.identity);
//                }
//                else
//                {
//                    if (tiles[i, j] == Tiletype.Floor)
//                    {
//                        tileContainer.GetComponent<SpriteRenderer>().sprite = floor;
//                        //instantiate a wall
//                        Instantiate(tileContainer, new Vector3(i, j, 0.0f), Quaternion.identity);
//                    }
//                }
//            }
//        }
//    }

//    void DetermineRandomValues(bool firstRoom = false)
//    {
//        roomStartX = Random.Range(0, LevelColumns);
//        roomWidth = WidthRange.Random();
//        if (!firstRoom)
//        {
//            if (CheckXAxis(roomStartX, roomWidth))
//            {
//                while (CheckXAxis(roomStartX, roomWidth))
//                {
//                    roomStartX = Random.Range(0, LevelColumns);
//                }
//            }
//        }
//        roomStartY = Random.Range(0, LevelRows);
//        roomHeight = HeightRange.Random();
//        if (!firstRoom)
//        {
//            if (CheckYAxis(roomStartY, roomHeight))
//            {
//                while (CheckYAxis(roomStartY, roomHeight))
//                {
//                    roomStartY = Random.Range(0, LevelRows);
//                }
//            }
//        }
//    }

//    Position RandomDirection(int x, int y, int w, int h)
//    {
//        Position result = new Position();
//        int newX=x, newY=y;
//        RoomDirection direction = (RoomDirection)Random.Range(0, 4);
//        roomDistance = 5;

//        roomWidth = WidthRange.Random();
//        roomHeight = HeightRange.Random();

//        switch (direction)
//        {
//            case RoomDirection.Left:
//                newX -= (w - roomDistance);
//                if (CheckXAxis(newX, roomWidth))
//                {
//                    result.tileX = newX;
//                    result.tileY = newY;
//                    break;
//                }
//                else
//                {
//                    newX = x;
//                    goto case RoomDirection.Right;
//                }
//            case RoomDirection.Right:
//                newX += (w + roomDistance);
//                if (CheckXAxis(newX, roomWidth))
//                {
//                    result.tileX = newX;
//                    result.tileY = newY;
//                    break;
//                }
//                else
//                {
//                    newX = x;
//                    goto case RoomDirection.Up;
//                }
//            case RoomDirection.Up:
//                newY -= (h - roomDistance);
//                if (CheckYAxis(newY, roomHeight))
//                {
//                    result.tileX = newX;
//                    result.tileY = newY;
//                    break;
//                }
//                else
//                {
//                    newY = y;
//                    goto case RoomDirection.Down;
//                }
//            case RoomDirection.Down:
//                newY += (h - +roomDistance);
//                if (CheckYAxis(newY, roomHeight))
//                {
//                    result.tileX = newX;
//                    result.tileY = newY;
//                    break;
//                }
//                else
//                {
//                    newY = y;
//                    goto case RoomDirection.Impossible;
//                }
//            case RoomDirection.Impossible:
//                Debug.Log("can't create room, in RandomDirection function, from X: "+x+" and Y: " +y);
//                break;
//        }
//        return result;
//    }

//    bool CheckXAxis(int x, int w)
//    {
//        if(x - w > LevelColumns || x + w > LevelColumns)
//        {
//            return true;
//        }
//        return false;
//    }

//    bool CheckYAxis(int y, int h)
//    {
//        if(y-h>LevelRows || y+h>LevelColumns)
//        {
//            return true;
//        }
//        return false;
//    }

//    void SurvivalOfTheFittest(List<Room> rooms)
//    {
//        int overlappCount = 0;
//        bool overlapping = true;
//        List<Room> overlappingRooms=new List<Room>();
//        while(overlapping)
//        {
//            overlapping = false;
//            for(int i=0; i<rooms.Count-1; i++)
//            {
//                for(int j=i+1; j<rooms.Count; j++)
//                {
//                    if(GetOverlap(rooms[i], rooms[j]))
//                    {
//                        overlappCount++;
//                        Debug.Log("room overlapping, a total of"+overlappCount);
//                        overlappingRooms.Add(rooms[i]);
//                    }
//                    else
//                    {
//                        if(GetOverlap(rooms[i], rooms[j]))
//                        {
//                            overlappCount++;
//                            Debug.Log("room overlapping, a total of" + overlappCount);
//                            overlappingRooms.Add(rooms[i]);
//                        }
//                    }
//                }
//            }
//        }
//        for (int i = 0; i < overlappingRooms.Count; i++)
//        {
//            allRooms.Remove(overlappingRooms[i]);
//        }
//        RemoveRoomsInTileArray(overlappingRooms);
//        //Debug.Log(overlappingRooms.Count);
//    }

//    void RemoveRoomsInTileArray(List<Room> rooms)
//    {
//        foreach(Room room in rooms)
//        {
//            for(int i=room.originX; i<room.originX+room.width; i++)
//            {
//                for(int j=room.originY; j<room.originY+room.height; j++)
//                {
//                    tiles[i, j] = Tiletype.Wall;
//                }
//            }
//        }
//    }

//    bool GetOverlap(Room first, Room second)
//    {
//        if ((first.originX < second.originX && first.originX + first.width > second.originX)&&((first.originY < second.originY && first.originY + first.height > second.originY)))
//        {
//            return true;
//        }
//        return false;
//    }
//}

