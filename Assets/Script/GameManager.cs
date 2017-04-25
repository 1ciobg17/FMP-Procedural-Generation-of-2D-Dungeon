using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    List<Room> levelRooms;
    List<List<Room>> gamePaths;
    
	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void SetRoomList(List<Room> roomList)
    {
        levelRooms = roomList;
        gamePaths = new List<List<Room>>();
        DetermineRoad();
        for(int i=0; i<gamePaths.Count; i++)
        {
            Debug.Log(i);
            foreach(Room room in gamePaths[i])
            {
                Debug.Log(room.originX + "_" + room.originY);
            }
        }
    }

    void DetermineRoad()
    {
        List<Room> singleRooms=new List<Room>();
        foreach(Room room in levelRooms)
        {
            if(room.neighbouringRooms.Count==1)
            {
                singleRooms.Add(room);
            }
        }

        FindRoad(singleRooms[0], new Room(), new List<Room>());

    }

    void FindRoad(Room room, Room prevR, List<Room> roomPath)
    {
        Room currentRoom=GetRoom(room);
        Room prevRoom = prevR;
        List<Room> path = new List<Room>();
        if(roomPath.Count>0)
        {
            foreach(Room pathR in roomPath)
            {
                path.Add(pathR);
            }
        }
        bool check = true;
        bool addToPath = true;
        while (check)
        {
            check = false;
            addToPath = true;
            if(path.Count==0 && currentRoom.neighbouringRooms.Count==1)
            {
                path.Add(currentRoom);
                prevRoom = currentRoom;
                currentRoom = GetRoom(currentRoom.neighbouringRooms[0]);
                check = true;
            }
            else
            {
                if(currentRoom.neighbouringRooms.Count==1)
                {
                    path.Add(currentRoom);
                }
                else
                {
                    if(currentRoom.neighbouringRooms.Count==2)
                    {
                        path.Add(currentRoom);

                        if(prevRoom.originX!=currentRoom.neighbouringRooms[0].originX || prevRoom.originY!=currentRoom.neighbouringRooms[0].originY)
                        {
                            prevRoom = currentRoom;
                            currentRoom = GetRoom(currentRoom.neighbouringRooms[0]);
                            check = true;
                        }
                        else
                        {
                            if (prevRoom.originX != currentRoom.neighbouringRooms[1].originX || prevRoom.originY != currentRoom.neighbouringRooms[1].originY)
                            {
                                prevRoom = currentRoom;
                                currentRoom = GetRoom(currentRoom.neighbouringRooms[1]);
                                check = true;
                            }
                        }
                        
                    }
                    else
                    {
                        if (currentRoom.neighbouringRooms.Count > 2)
                        {
                            path.Add(currentRoom);
                            addToPath = false;
                            if (prevRoom.originX != currentRoom.neighbouringRooms[0].originX || prevRoom.originY != currentRoom.neighbouringRooms[0].originY)
                            {
                                FindRoad(currentRoom.neighbouringRooms[0], currentRoom, path);
                            }
                            if (prevRoom.originX != currentRoom.neighbouringRooms[1].originX || prevRoom.originY != currentRoom.neighbouringRooms[1].originY)
                            {
                                FindRoad(currentRoom.neighbouringRooms[1], currentRoom, path);
                            }
                            if (prevRoom.originX != currentRoom.neighbouringRooms[2].originX || prevRoom.originY != currentRoom.neighbouringRooms[2].originY)
                            {
                                FindRoad(currentRoom.neighbouringRooms[2], currentRoom, path);
                            }
                            prevRoom = currentRoom;
                        }
                    }
                }
            }
        }
        if(addToPath)
            gamePaths.Add(path);
    }

    Room GetRoom(Room room)
    {
        for(int i=0; i<levelRooms.Count; i++)
        {
            if(room.originX==levelRooms[i].originX && room.originY==levelRooms[i].originY)
            {
                return levelRooms[i];
            }
        }

        return null;
    }
}
