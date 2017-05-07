//Student Name: George Alexandru Ciobanita
//Student ID: Q11598417
//Project: FINAL MAJOR PROJECT CGP601
//Class: GameManager
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour {

    //the rooms in the level that have passed creation
    List<Room> levelRooms;
    //the gamepaths of the level starting from the rooms that have a single neighbour
    List<List<Room>> gamePaths;
    //the longest path
    List<Room> victoryPath;
    //the entities that exist in the level
    List<Entity> levelEntities;
    //the current room the player character is currently situated
    Room activeRoom;
    public GraphicsManager graphicsManager;
    public GameObject characterObject;
    GameObject playerCharacter;
    
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    //the gamemanager only starts on its initial call from the level manager
    public void SetUpGameArea(List<Room> roomList)
    {
        //set up the lists
        levelEntities = new List<Entity>();
        levelRooms = roomList;
        gamePaths = new List<List<Room>>();
        activeRoom = new Room();
        //fill the list of game paths
        AcquireRoads();
        //set up the entrances by picking the longest road from the list
        SetUpDungeonEntrances();
        //set up the rest of the rooms
        SetUpDungeonRooms();
        //pass level entity data
        graphicsManager.AcquireLevelEntityData(levelEntities);
        //and start the drawing process
        graphicsManager.Draw();
    }

    void AcquireRoads()
    {
        System.Random randomNumberGeneration = new System.Random();

        List<Room> singleRooms=new List<Room>();
        foreach(Room room in levelRooms)
        {
            if(room.neighbouringRooms.Count==1)
            {
                singleRooms.Add(room);
            }
        }

        //pick a random room from the list of single rooms and find its longest road starting with its neighbours
        FindAllRoads(singleRooms[randomNumberGeneration.Next(0, singleRooms.Count-1)], new Room(), new List<Room>());
    }

    void FindAllRoads(Room room, Room prevR, List<Room> roomPath)
    {
        //date is passed
        Room currentRoom=GetRoom(room);
        Room prevRoom = prevR;
        List<Room> path = new List<Room>();
        //if a path exists
        if(roomPath.Count>0)
        {
            foreach(Room pathR in roomPath)
            {
                path.Add(pathR);
            }
        }
 
        bool check = true;
        bool addToPaths = true;
        while (check)
        {
            check = false;
            addToPaths = true;
            //verify if this room is the first room in the list with a single neighbour
            if(path.Count==0 && currentRoom.neighbouringRooms.Count==1)
            {
                path.Add(currentRoom);
                prevRoom = currentRoom;
                currentRoom = GetRoom(currentRoom.neighbouringRooms[0]);
                check = true;
            }
            else
            {
                //if this is the last room
                if(currentRoom.neighbouringRooms.Count==1)
                {
                    path.Add(currentRoom);
                }
                else
                {
                    //if the room actually has two neighbours
                    if(currentRoom.neighbouringRooms.Count==2)
                    {
                        //add current room to the path list
                        path.Add(currentRoom);

                        //then determine which on of the neighbours is the new room to be added to the path list
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
                        //in rare cases that a room has 3 neighbours
                        if (currentRoom.neighbouringRooms.Count > 2)
                        {
                            path.Add(currentRoom);
                            addToPaths = false;
                            //find the longest path starting from there using recursivity and note all paths in the list
                            if (prevRoom.originX != currentRoom.neighbouringRooms[0].originX || prevRoom.originY != currentRoom.neighbouringRooms[0].originY)
                            {
                                FindAllRoads(currentRoom.neighbouringRooms[0], currentRoom, path);
                            }
                            if (prevRoom.originX != currentRoom.neighbouringRooms[1].originX || prevRoom.originY != currentRoom.neighbouringRooms[1].originY)
                            {
                                FindAllRoads(currentRoom.neighbouringRooms[1], currentRoom, path);
                            }
                            if (prevRoom.originX != currentRoom.neighbouringRooms[2].originX || prevRoom.originY != currentRoom.neighbouringRooms[2].originY)
                            {
                                FindAllRoads(currentRoom.neighbouringRooms[2], currentRoom, path);
                            }
                            prevRoom = currentRoom;
                        }
                    }
                }
            }
        }
        if(addToPaths)
            gamePaths.Add(path);
    }

    Room GetRoom(Room room)
    {
        //find the exact room to be added in the list
        for(int i=0; i<levelRooms.Count; i++)
        {
            if(room.originX==levelRooms[i].originX && room.originY==levelRooms[i].originY)
            {
                return levelRooms[i];
            }
        }

        return null;
    }

    //once the start and end rooms have been found, set up entrances/exits/key to open the exit
    void SetUpDungeonEntrances()
    {
        Entity dungeonEntry = new Entity();
        Entity dungeonExit = new Entity();
        Entity dungeonKey = new Entity();

        int biggestRoomCount = 0;
        int tileCount = 0;
        List<Room> longestRoad = new List<Room>();
        List<Room> keyRoad = new List<Room>();

        //find the longest road in the list in order to use its first and last room for entrance and exit
        for (int i = 0; i < gamePaths.Count; i++)
        {
            if (gamePaths[i].Count > biggestRoomCount)
            {
                keyRoad = longestRoad;
                biggestRoomCount = gamePaths[i].Count;
                longestRoad = gamePaths[i];
            }
            else
            {
                //second longest path is the keyroad, if it exists
                keyRoad = gamePaths[i];
            }
        }

        victoryPath = longestRoad;

        //find the biggest region in the entrance room then place the entrytile in the middle of it
        for (int i = 0; i < victoryPath[0].regionList.Count; i++)
        {
            if (victoryPath[0].regionList[i].tiles.Count > tileCount)
            {
                tileCount = victoryPath[0].regionList[i].tiles.Count;
                dungeonEntry = new Entity(victoryPath[0].regionList[i].tiles[victoryPath[0].regionList[i].tiles.Count / 2].tileX + victoryPath[0].originX, victoryPath[0].regionList[i].tiles[victoryPath[0].regionList[i].tiles.Count / 2].tileY + victoryPath[0].originY, EntityType.DungeonEntry, victoryPath[0]);
            }
        }

        //entry tiles are then activated for gameplay
        foreach (GameObject entryTile in victoryPath[0].entryTiles)
        {
            entryTile.GetComponent<EntryTile>().DeactivateTrap();
        }

        levelEntities.Add(dungeonEntry);
        tileCount = 0;

        //find the biggest region in the exit room then place the exittile in the middle of it
        for (int i = 0; i < victoryPath[victoryPath.Count - 1].regionList.Count; i++)
        {
            if (victoryPath[victoryPath.Count - 1].regionList[i].tiles.Count > tileCount)
            {
                tileCount = victoryPath[victoryPath.Count - 1].regionList[i].tiles.Count;
                dungeonExit = new Entity(victoryPath[victoryPath.Count - 1].regionList[i].tiles[victoryPath[victoryPath.Count - 1].regionList[i].tiles.Count / 2].tileX + victoryPath[victoryPath.Count - 1].originX, victoryPath[victoryPath.Count - 1].regionList[i].tiles[victoryPath[victoryPath.Count - 1].regionList[i].tiles.Count / 2].tileY + victoryPath[victoryPath.Count - 1].originY, EntityType.DungeonExit, victoryPath[victoryPath.Count - 1]);
            }
        }

        foreach (GameObject entryTile in victoryPath[victoryPath.Count - 1].entryTiles)
        {
            entryTile.GetComponent<EntryTile>().DeactivateTrap();
        }

        levelEntities.Add(dungeonExit);

        tileCount = 0;

        //if there is another road besides the one used for the longest road
        if (keyRoad.Count > 0)
        {
            for (int i = 0; i < keyRoad[keyRoad.Count - 1].regionList.Count; i++)
            {
                //use it to place the key, this is a shorter road
                if (keyRoad[keyRoad.Count - 1].regionList[i].tiles.Count > tileCount)
                {
                    tileCount = keyRoad[keyRoad.Count - 1].regionList[i].tiles.Count;
                    dungeonKey = new Entity(keyRoad[keyRoad.Count - 1].regionList[i].tiles[keyRoad[keyRoad.Count - 1].regionList[i].tiles.Count / 2].tileX + keyRoad[keyRoad.Count - 1].originX, keyRoad[keyRoad.Count - 1].regionList[i].tiles[keyRoad[keyRoad.Count - 1].regionList[i].tiles.Count / 2].tileY + keyRoad[keyRoad.Count - 1].originY, EntityType.Key, keyRoad[keyRoad.Count - 1]);
                }
            }
        }
        else
        {
            //else the key is in the same room as the exit
            dungeonKey = new Entity(dungeonExit.originX, dungeonExit.originY, EntityType.Key, dungeonExit.parentRoom);
        }

        levelEntities.Add(dungeonKey);

        PlacePlayerCharacter(new Vector3(dungeonEntry.originX, dungeonEntry.originY, 0.0f));
    }

    void PlacePlayerCharacter(Vector3 dungeonEntry)
    {
        //the position if the entrance tile is used
        characterObject.transform.position = new Vector3(dungeonEntry.x, dungeonEntry.y);
        playerCharacter=GameObject.Instantiate(characterObject) as GameObject;
        //current room the player is in
        activeRoom = victoryPath[0];
        activeRoom.hasBeenVisited = true;
        playerCharacter.GetComponent<PlayerCharacter>().SetLevelLocation(activeRoom);
    }

    //the other rooms in the level get either spikes of magic orbs as room objectives to get the rooms open
    void SetUpDungeonRooms()
    {
        foreach(Room room in levelRooms)
        {
            if((room.originX!=victoryPath[0].originX && room.originX!=victoryPath[victoryPath.Count-1].originX) ||(room.originY != victoryPath[0].originY && room.originY != victoryPath[victoryPath.Count - 1].originY))
            {
                RoomObjective(room);
            }
        }
    }

    public void RoomObjective(Room room)
    {
        Entity questEntity = new Entity();
        int regionSize = 0;
        Region targetRegion;

        //one or the other
        System.Random rng = new System.Random();
        int objective = rng.Next((int)EntityType.MagicOrb, (int)EntityType.Enemy+1);
        if((int)levelEntities[levelEntities.Count-1].entityType==objective)
        {
            if((int)levelEntities[levelEntities.Count-2].entityType==objective)
            {
                while(objective==(int)levelEntities[levelEntities.Count-1].entityType)
                {
                    //but if the same objective has been used the last few times then try to change it again
                    objective = rng.Next((int)EntityType.MagicOrb, (int)EntityType.Enemy + 1);
                }
            }
        }

        //place the enemy in the biggest region in the room similar to the entrance/exit
        if (objective == (int)EntityType.Enemy)
        {
            targetRegion = room.regionList[0];
            for (int i = 0; i < room.regionList.Count; i++)
            {
                if (room.regionList[i].regionSize > regionSize)
                {
                    regionSize = room.regionList[i].regionSize;
                    targetRegion = room.regionList[i];
                }
            }
            questEntity = new Entity(targetRegion.tiles[targetRegion.tiles.Count / 2].tileX + room.originX, targetRegion.tiles[targetRegion.tiles.Count / 2].tileY + room.originY, EntityType.Enemy, room);
            levelEntities.Add(questEntity);
        }
        else
        {
            //similar to the orbs
            targetRegion = room.regionList[0];
            for (int i = 0; i < room.regionList.Count; i++)
            {
                if (room.regionList[i].regionSize > regionSize)
                {
                    regionSize = room.regionList[i].regionSize;
                    targetRegion = room.regionList[i];
                }
            }
            questEntity = new Entity(targetRegion.tiles[targetRegion.tiles.Count / 2].tileX+room.originX, targetRegion.tiles[targetRegion.tiles.Count / 2].tileY+room.originY, EntityType.MagicOrb, room);
            levelEntities.Add(questEntity);
        }
    }

    //reload the scene
    public void RestartLevel()
    {
        StartCoroutine(SceneLoadDelay());
        SceneManager.LoadScene("test");
    }

    IEnumerator SceneLoadDelay()
    {
        yield return new WaitForSeconds(5.0f);
    }
}
