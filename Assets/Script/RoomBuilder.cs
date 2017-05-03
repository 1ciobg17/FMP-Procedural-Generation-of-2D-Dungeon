using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class RoomBuilder : MonoBehaviour {

    public InputField roomWidthField;
    public InputField roomHeightField;
    public InputField roomNameField;
    public Dropdown selectedTileType;
    public GameObject tileContainer;
    public Sprite wall;
    public Sprite floor;
    public Sprite entry;
    public Sprite empty;
    string roomFileName;
    int roomWidth=0;
    int roomHeight=0;
    bool generateRoom = false;
    Tiletype currentTileType=Tiletype.Null;
    GameObject[,] roomArray;
    Tiletype[,] tiletypeArray;
    string filepath= @"C:\Users\Public\Documents\";

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(generateRoom && roomWidth!=0 && roomHeight!=0)
        {
            generateRoom = false;
            Vector2 position;

            GameObject[] go = GameObject.FindGameObjectsWithTag("Tile");
            if (go.Length > 0)
            {
                foreach (GameObject tile in go)
                {
                    Destroy(tile);
                }
            }

            roomArray = new GameObject[roomWidth, roomHeight];
            tiletypeArray = new Tiletype[roomWidth, roomHeight];
            for(int i=0; i<roomWidth; i++)
            {
                for(int j=0; j<roomHeight; j++)
                {
                    position = new Vector2(i, j);
                    roomArray[i,j] = GameObject.Instantiate(tileContainer, position, Quaternion.identity) as GameObject;
                    tiletypeArray[i, j] = Tiletype.Null;
                }
            }
        }

        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit)
            {
                if (hit.collider.tag == "Tile")
                {
                    SetUpTile(hit);
                }
            }
        }
    }

    public void GetRoomWidth()
    {
        roomWidth = Convert.ToInt32(roomWidthField.text);
    }

    public void GetRoomHeight()
    {
        roomHeight = Convert.ToInt32(roomHeightField.text);
    }

    public void TriggerRoomGeneration()
    {
        generateRoom = true;
    }

    public void GetTileType()
    {
        switch(selectedTileType.value)
        {
            case 0:
                currentTileType = Tiletype.Null;
                break;
            case 1:
                currentTileType = Tiletype.Wall;
                break;
            case 2:
                currentTileType = Tiletype.Floor;
                break;
            case 3:
                currentTileType = Tiletype.Entry;
                break;
        }
    }

    void SetUpTile(RaycastHit2D hitTile)
    {
        SpriteRenderer tileSR = hitTile.transform.GetComponent<SpriteRenderer>();
        switch(currentTileType)
        {
            case Tiletype.Null:
                tileSR.sprite = empty;
                tiletypeArray[(int)hitTile.transform.position.x, (int)hitTile.transform.position.y] = Tiletype.Null;
                break;
            case Tiletype.Floor:
                tileSR.sprite = floor;
                tiletypeArray[(int)hitTile.transform.position.x, (int)hitTile.transform.position.y] = Tiletype.Floor;
                break;
            case Tiletype.Wall:
                tileSR.sprite = wall;
                tiletypeArray[(int)hitTile.transform.position.x, (int)hitTile.transform.position.y] = Tiletype.Wall;
                break;
            case Tiletype.Entry:
                tileSR.sprite = entry;
                tiletypeArray[(int)hitTile.transform.position.x, (int)hitTile.transform.position.y] = Tiletype.Entry;
                break;
        }
    }

    public void CreateRoomFile()
    {
        File.Create(filepath + roomFileName + ".txt").Dispose();
        StreamWriter sw = new StreamWriter(filepath + roomFileName + ".txt");
        sw.WriteLine(roomWidth+"_"+roomHeight);
        for(int i=0; i<roomWidth; i++)
        {
            for(int j=0; j<roomHeight; j++)
            {
                sw.WriteLine(i + "_" + j + "_" + tiletypeArray[i, j]);
            }
        }
        sw.Close();
        
    }

    public void UpdateFileName()
    {
        roomFileName = roomNameField.text;
    }
}
