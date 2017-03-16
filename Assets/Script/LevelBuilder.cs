using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBuilder : MonoBehaviour {

    private enum Tiletype
    {
        Wall, Floor,
    }

    private Tiletype[][] tiles;

    public int LevelRows=100;
    public int LevelColumns=100;
    public IntRange roomNumber;
    public IntRange roomWidth;
    public IntRange roomHeight;

    public GameObject wallTiles;

	// Use this for initialization
	void Start () {
        SetupTileArray();
        InstantiateTiles();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void SetupTileArray()
    {
        //set array to correct width
        tiles = new Tiletype[LevelColumns][];

        //go through all the tiles
        for(int i=0; i<tiles.Length; i++)
        {
            //set at correct height
            tiles[i] = new Tiletype[LevelRows];
        }
    }

    void InstantiateTiles()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[i].Length; j++)
            {
                //instantiate a wall
                Instantiate(wallTiles, new Vector3(i, j, 0.0f), Quaternion.identity);
            }
        }
    }
}
