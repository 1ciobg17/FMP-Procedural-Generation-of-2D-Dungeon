using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    List<Room> levelRooms;
    List<List<Room>> gamePaths;
    List<Room> victoryPath;
    List<Room> tranversedPath;
    List<Entity> levelEntities;
    Room activeRoom;
    List<Room> neighbouringRooms;
    public GraphicsManager graphicsManager;
    public GameObject characterObject;
    GameObject playerCharacter;
    
	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    public void SetUpGameArea(List<Room> roomList)
    {
        levelEntities = new List<Entity>();
        levelRooms = roomList;
        gamePaths = new List<List<Room>>();
        tranversedPath = new List<Room>();
        neighbouringRooms = new List<Room>();
        activeRoom = new Room();
        AcquireRoads();
        SetUpDungeonEntrances();
        SetUpDungeonRooms();
        graphicsManager.AcquireLevelEntityData(levelEntities);
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

        FindAllRoads(singleRooms[randomNumberGeneration.Next(0, singleRooms.Count-1)], new Room(), new List<Room>());
    }

    void FindAllRoads(Room room, Room prevR, List<Room> roomPath)
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
        bool addToPaths = true;
        while (check)
        {
            check = false;
            addToPaths = true;
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
                            addToPaths = false;
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
        for(int i=0; i<levelRooms.Count; i++)
        {
            if(room.originX==levelRooms[i].originX && room.originY==levelRooms[i].originY)
            {
                return levelRooms[i];
            }
        }

        return null;
    }

    void SetUpDungeonEntrances()
    {
        Entity dungeonEntry = new Entity();
        Entity dungeonExit = new Entity();
        Entity dungeonKey = new Entity();

        int biggestRoomCount = 0;
        int tileCount = 0;
        List<Room> longestRoad = new List<Room>();
        List<Room> keyRoad = new List<Room>();

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
                keyRoad = gamePaths[i];
            }
        }

        victoryPath = longestRoad;

        for (int i = 0; i < victoryPath[0].regionList.Count; i++)
        {
            if (victoryPath[0].regionList[i].tiles.Count > tileCount)
            {
                tileCount = victoryPath[0].regionList[i].tiles.Count;
                dungeonEntry = new Entity(victoryPath[0].regionList[i].tiles[victoryPath[0].regionList[i].tiles.Count / 2].tileX + victoryPath[0].originX, victoryPath[0].regionList[i].tiles[victoryPath[0].regionList[i].tiles.Count / 2].tileY + victoryPath[0].originY, EntityType.DungeonEntry, victoryPath[0]);
            }
        }

        foreach (GameObject entryTile in victoryPath[0].entryTiles)
        {
            entryTile.GetComponent<EntryTile>().DeactivateTrap();
        }

        levelEntities.Add(dungeonEntry);
        tileCount = 0;

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

        for (int i = 0; i < keyRoad[keyRoad.Count - 1].regionList.Count; i++)
        {
            if (keyRoad[keyRoad.Count - 1].regionList[i].tiles.Count > tileCount)
            {
                tileCount = keyRoad[keyRoad.Count - 1].regionList[i].tiles.Count;
                dungeonKey = new Entity(keyRoad[keyRoad.Count - 1].regionList[i].tiles[keyRoad[keyRoad.Count - 1].regionList[i].tiles.Count / 2].tileX + keyRoad[keyRoad.Count - 1].originX, keyRoad[keyRoad.Count - 1].regionList[i].tiles[keyRoad[keyRoad.Count - 1].regionList[i].tiles.Count / 2].tileY + keyRoad[keyRoad.Count - 1].originY, EntityType.Key, keyRoad[keyRoad.Count - 1]);
            }
        }

        levelEntities.Add(dungeonKey);

        PlacePlayerCharacter(new Vector3(dungeonEntry.originX, dungeonEntry.originY, 0.0f));
    }

    void PlacePlayerCharacter(Vector3 dungeonEntry)
    {
        characterObject.transform.position = new Vector3(dungeonEntry.x, dungeonEntry.y);
        playerCharacter=GameObject.Instantiate(characterObject) as GameObject;
        activeRoom = victoryPath[0];
        activeRoom.hasBeenVisited = true;
        playerCharacter.GetComponent<PlayerCharacter>().SetLevelLocation(activeRoom);
        tranversedPath.Add(activeRoom);
        foreach (Room room in activeRoom.neighbouringRooms)
        {
            neighbouringRooms.Add(room);
        }
    }

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

        System.Random rng = new System.Random();
        int objective = rng.Next((int)EntityType.MagicOrb, (int)EntityType.Enemy+1);
        if((int)levelEntities[levelEntities.Count-1].entityType==objective)
        {
            if((int)levelEntities[levelEntities.Count-2].entityType==objective)
            {
                while(objective==(int)levelEntities[levelEntities.Count-1].entityType)
                {
                    objective = rng.Next((int)EntityType.MagicOrb, (int)EntityType.Enemy + 1);
                }
            }
        }

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
