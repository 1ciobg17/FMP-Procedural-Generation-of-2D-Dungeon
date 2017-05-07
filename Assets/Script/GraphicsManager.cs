//Student Name: George Alexandru Ciobanita
//Student ID: Q11598417
//Project: FINAL MAJOR PROJECT CGP601
//Class: GraphicsManager
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsManager : MonoBehaviour {

    public GameObject dungeonEntry;
    public GameObject dungeonExit;
    public GameObject wallContainer;//wall is 0, floor is 1
    public GameObject generalContainer;
    public GameObject entryContainer;
    public GameObject magicOrb;
    public GameObject enemy;
    public GameObject key;
    List<Room> allRooms;
    List<Position> passageTiles;
    List<Position> passageWalls;
    List<Entity> levelEntities;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Draw()
    {
        DrawEntities();
        DrawLevel();
    }

    //tranverse each tile then draw
    void DrawPassages()
    {
        Vector3 position;
        for (int i = 0; i < passageTiles.Count; i++)
        {
            position = new Vector3(passageTiles[i].tileX, passageTiles[i].tileY);
            generalContainer.gameObject.name = "Passage";
            GameObject.Instantiate(generalContainer, position, Quaternion.identity);
        }
        for (int i = 0; i < passageWalls.Count; i++)
        {
            position = new Vector3(passageWalls[i].tileX, passageWalls[i].tileY);
            GameObject.Instantiate(wallContainer, position, Quaternion.identity);
        }
    }

    void DrawRoom(Room room)
    {
        Vector3 position;
        //tranverse each tile
        for (int i = 0; i < room.height; i++)
        {
            for (int j = 0; j < room.width; j++)
            {
                //get position of the tile by calculating it
                position = new Vector3(room.originX + i, room.originY + j);
                if (room.roomTiles[i, j] == Tiletype.Floor)
                {

                    GameObject.Instantiate(generalContainer, position, Quaternion.identity);
                }
                if(room.roomTiles[i,j]==Tiletype.Wall)
                {
                    wallContainer.name = "WallTile_Placeholder";
                    GameObject.Instantiate(wallContainer, position, Quaternion.identity);
                }
            }
        }
        foreach (Position wallingTile in room.wallTiles)
        {
            position = new Vector3(room.originX + wallingTile.tileX, room.originY + wallingTile.tileY);
            wallContainer.name = "WallTile_Placeholder";
            GameObject.Instantiate(wallContainer, position, Quaternion.identity);
        }
    }

    void DrawLevel()
    {
        DrawPassages();
        foreach (Room room in allRooms)
        {
            DrawRoom(room);
        }
    }

    void DrawEntities()
    {
        GameObject go;
        for(int i=0; i<levelEntities.Count; i++)
        {
            if(levelEntities[i].entityType==EntityType.DungeonEntry)
            {
                go = Instantiate(dungeonEntry, new Vector3(levelEntities[i].originX, levelEntities[i].originY, 0.0f), Quaternion.identity) as GameObject;
            }
            if (levelEntities[i].entityType == EntityType.DungeonExit)
            {
                go = Instantiate(dungeonExit, new Vector3(levelEntities[i].originX, levelEntities[i].originY, 0.0f), Quaternion.identity) as GameObject;
            }
            if(levelEntities[i].entityType==EntityType.MagicOrb)
            {
                go = Instantiate(magicOrb, new Vector3(levelEntities[i].originX, levelEntities[i].originY, 0.0f), Quaternion.identity) as GameObject;
                go.GetComponent<MagicOrb>().SetParentRoom(levelEntities[i].parentRoom);
            }
            if (levelEntities[i].entityType == EntityType.Enemy)
            {
                go =Instantiate(enemy, new Vector3(levelEntities[i].originX, levelEntities[i].originY, 0.0f), Quaternion.identity) as GameObject;
                go.GetComponent<Enemy>().SetParentRoom(levelEntities[i].parentRoom);
            }
            if(levelEntities[i].entityType==EntityType.Key)
            {
                go = Instantiate(key, new Vector3(levelEntities[i].originX, levelEntities[i].originY, 0.0f), Quaternion.identity) as GameObject;
            }
        }
    }

    public void AcquireLevelGraphicsData(List<Room> levelRooms, List<Position> corridorTiles, List<Position> corridorWalls)
    {
        allRooms = levelRooms;
        passageTiles = corridorTiles;
        passageWalls = corridorWalls;
    }

    public void AcquireLevelEntityData(List<Entity> entities)
    {
        levelEntities = entities;
    }
}
