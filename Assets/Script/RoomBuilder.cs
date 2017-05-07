//Student Name: George Alexandru Ciobanita
//Student ID: Q11598417
//Project: FINAL MAJOR PROJECT CGP601
//Class: RoomBuilder
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class RoomBuilder : MonoBehaviour {

    //the camera in the scene to control
    public Camera mainCamera;
    //UI input fields
    public InputField roomWidthField;
    public InputField roomHeightField;
    public InputField roomNameSave;
    public InputField roomNameLoad;
    public InputField filepathSaveLocation;
    //dropdown menu containing all tiletypes
    public Dropdown selectedTileType;
    public GameObject tileContainer;
    //tile sprites for easier representation
    public Sprite wall;
    public Sprite floor;
    public Sprite entry;
    public Sprite empty;
    string roomFileName;
    int roomWidth=0;
    int roomHeight=0;
    Tiletype currentTileType=Tiletype.Null;
    GameObject[,] roomArray;
    Tiletype[,] tiletypeArray;
    //filepath NEEDS to be a public filepath that does not require user to give permissions
    string filepath= @"C:\Users\Public\Documents\";
	
	// Update is called once per frame
	void Update () {

        //on mouse1 input
        if(Input.GetMouseButtonDown(0))
        {
            //detect if a tile was hit
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit)
            {
                if (hit.collider.tag == "Tile")
                {
                    //set it up with the selected tiletype in the dropdown list
                    SetUpTile(hit);
                }
            }
        }

        //camera control method
        CameraControl();
    }

    void CreateNewRoom()
    {
        //once height and width have been set
        if (roomWidth != 0 && roomHeight != 0)
        {
            Vector2 position;

            //if any tiles have been set before, delete them
            GameObject[] go = GameObject.FindGameObjectsWithTag("Tile");
            if (go.Length > 0)
            {
                foreach (GameObject tile in go)
                {
                    Destroy(tile);
                }
            }

            //make a new array and tiletype array
            roomArray = new GameObject[roomWidth, roomHeight];
            tiletypeArray = new Tiletype[roomWidth, roomHeight];
            for (int i = 0; i < roomWidth; i++)
            {
                for (int j = 0; j < roomHeight; j++)
                {
                    //set up the new room with default tiles
                    position = new Vector2(i, j);
                    roomArray[i, j] = GameObject.Instantiate(tileContainer, position, Quaternion.identity) as GameObject;
                    tiletypeArray[i, j] = Tiletype.Null;
                }
            }
        }
    }

    //when the UI data is set, sends a signal with the data
    public void GetRoomWidth()
    {
        roomWidth = Convert.ToInt32(roomWidthField.text);
    }

    public void GetRoomHeight()
    {
        roomHeight = Convert.ToInt32(roomHeightField.text);
    }

    //room generation is triggered once the "generate room" button has been pressed
    public void TriggerRoomGeneration()
    {
        tileContainer.GetComponent<SpriteRenderer>().sprite = empty;
        CreateNewRoom();
    }

    void CameraControl()
    {
        //directional camera control with arrow keys
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            mainCamera.transform.position = new Vector3(mainCamera.transform.position.x - 1.0f, mainCamera.transform.position.y, mainCamera.transform.position.z);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                mainCamera.transform.position = new Vector3(mainCamera.transform.position.x + 1.0f, mainCamera.transform.position.y, mainCamera.transform.position.z);
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + 1.0f, mainCamera.transform.position.z);
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        mainCamera.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y - 1.0f, mainCamera.transform.position.z);
                    }
                }
            }
        }
        //and camera zoom
        if(Input.GetKeyDown(KeyCode.Q))
        {
            mainCamera.orthographicSize -= 1.0f;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                mainCamera.orthographicSize += 1.0f;
            }
        }
    }

    //tiletype value is set on dropdown trigger
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
        //the tile gets its sprite and type changed based on the currently selected type
        SpriteRenderer tileSR = hitTile.transform.GetComponent<SpriteRenderer>();
        Tiletype newTiletype=Tiletype.Null;
        switch(currentTileType)
        {
            case Tiletype.Null:
                tileSR.sprite = empty;
                newTiletype = Tiletype.Null;
                break;
            case Tiletype.Floor:
                tileSR.sprite = floor;
                newTiletype = Tiletype.Floor;
                break;
            case Tiletype.Wall:
                tileSR.sprite = wall;
                newTiletype = Tiletype.Wall;
                break;
            case Tiletype.Entry:
                tileSR.sprite = entry;
                newTiletype = Tiletype.Entry;
                break;
        }
        //and the tile array also is changed
        tiletypeArray[(int)hitTile.transform.position.x, (int)hitTile.transform.position.y] = newTiletype;
    }

    public void CreateRoomFile()
    {
        //a file with the name is created and disposed to allow for edits to be made
        File.Create(filepath + roomFileName + ".txt").Dispose();
        StreamWriter sw = new StreamWriter(filepath + roomFileName + ".txt");
        //the first line represents room dimensions
        sw.WriteLine(roomWidth+"_"+roomHeight);
        for(int i=0; i<roomWidth; i++)
        {
            for(int j=0; j<roomHeight; j++)
            {
                //following lines are tile coordinates within a room and their type
                sw.WriteLine(i + "_" + j + "_" + tiletypeArray[i, j]);
            }
        }
        //close the edited file
        sw.Close();       
    }

    public void UpdateFileName()
    {
        roomFileName = roomNameSave.text;
    }

    public void LoadFile()
    {
        //read all file lines
        string[] fileLines = File.ReadAllLines(roomNameLoad.text);
        List<String> lineStrings = new List<string>(fileLines[0].Split('_'));
        //get the first line, the room sizes
        roomWidth = Convert.ToInt32(lineStrings[0]);
        roomHeight = Convert.ToInt32(lineStrings[1]);
        //set up the array from which the tiles will be set up on screen
        tiletypeArray = new Tiletype[roomWidth, roomHeight];

        for (int i = 1; i < fileLines.Length; i++)
        {
            lineStrings = new List<string>(fileLines[i].Split('_'));
            tiletypeArray[Convert.ToInt32(lineStrings[0]), Convert.ToInt32(lineStrings[1])] = (Tiletype)Enum.Parse(typeof(Tiletype), lineStrings[2]);
        }

        LoadSavedRoom();
    }

    void LoadSavedRoom()
    {
        SpriteRenderer tileSR = tileContainer.GetComponent<SpriteRenderer>();
        //if sizes have been specified
        if (roomWidth != 0 && roomHeight != 0)
        {
            Vector2 position;

            //delete previous tiles
            GameObject[] go = GameObject.FindGameObjectsWithTag("Tile");
            if (go.Length > 0)
            {
                foreach (GameObject tile in go)
                {
                    Destroy(tile);
                }
            }

            roomArray = new GameObject[roomWidth, roomHeight];

            for (int i = 0; i < roomWidth; i++)
            {
                for (int j = 0; j < roomHeight; j++)
                {
                    position = new Vector2(i, j);
                    switch (tiletypeArray[i,j])
                    {
                        case Tiletype.Null:
                            tileSR.sprite = empty;
                            break;
                        case Tiletype.Floor:
                            tileSR.sprite = floor;
                            break;
                        case Tiletype.Wall:
                            tileSR.sprite = wall;
                            break;
                        case Tiletype.Entry:
                            tileSR.sprite = entry;
                            break;
                    }

                    //place the tiles
                    roomArray[i, j] = GameObject.Instantiate(tileContainer, position, Quaternion.identity) as GameObject;
                }
            }
        }
    }

    public void GetFilePathLocation()
    {
        filepath = filepathSaveLocation.text+@"\";
    }
}
