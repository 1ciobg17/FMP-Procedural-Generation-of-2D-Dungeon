using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCreation : MonoBehaviour {

    public int width;
    public int height;
    [Range(0,100)]
    public int randomFillPercent;
    public GameObject tileContainer;//wall is 0, floor is 1
    SpriteRenderer tileSprite;
    public Sprite floor;
    public Sprite wall;
    int[,] room;
    public string seed;
    public bool useRandomSeed;
    [Range(0,100)]
    public int smoothCount;

    struct Position
    {
        public int tileX;
        public int tileY;

        public Position(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    void Start()
    {
        tileSprite = tileContainer.GetComponent<SpriteRenderer>();
        GenerateRoom();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Tile");
            foreach (GameObject go in allObjects)
                Destroy(go);
            GenerateRoom();
        }
    }

    void GenerateRoom()
    {
        room = new int[width, height];
        RandomFillRoom();


        for (int i = 0; i < smoothCount; i++)
        {
            SmoothTiles();
        }

        //for (int i = 1; i < width - 1; i++)
        //{
        //    for (int j = 1; j < height - 1; j++)
        //    {
        //        CheckTileTypes(i, j);
        //    }
        //}

         SetUpTile();
    }

    List<Position> GetSpaceTiles(int startX, startY)
    {
        List<Position>
    }

    void SmoothTiles()
    {
        for (int x = 1; x < width-1; x++)
        {
            for (int y = 1; y < height-1; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    room[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    room[x, y] = 0;

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += room[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    void CheckTileTypes(int gridX, int gridY)
    {
        int tileType;
        int percentage = 20;
        string seed=Time.time.ToString();
        System.Random roll = new System.Random(seed.GetHashCode());
        tileType = room[gridX, gridY];

        if(tileType==room[gridX,gridY-1])
        {
            percentage += 20;
        }
        if (tileType == room[gridX, gridY + 1])
        {
            percentage += 20;
        }
        if (tileType == room[gridX - 1, gridY])
        {
            percentage += 20;
        }
        if (tileType == room[gridX + 1, gridY])
        {
            percentage += 20;
        }

        if(roll.Next(0, 100) >= percentage)
        {
            if (tileType == 1)
                room[gridX, gridY] = 0;
            else
                room[gridX, gridY] = 1;
        }
    }

    void RandomFillRoom()
    {
        if(useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random randomNumberGeneration = new System.Random(seed.GetHashCode());

        for(int i=0; i<width; i++)
        {
            for(int j=0; j<height; j++)
            {
                if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                {
                    room[i, j] = 0;
                }
                else
                {
                    room[i, j] = (randomNumberGeneration.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void SetUpTile()
    {
        if(room!=null)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Vector3 position = new Vector3(-width / 2 + i, -height / 2 + j);
                    if(room[i,j]==1)
                    {
                        tileSprite.sprite = floor;
                        //tileContainer.gameObject.tag = "Floor";
                        GameObject.Instantiate(tileContainer, position, Quaternion.identity);
                    }
                    else
                    {
                        tileSprite.sprite = wall;
                        //tileContainer.gameObject.tag = "Wall";
                        GameObject.Instantiate(tileContainer, position, Quaternion.identity);
                    }
                }
            }
        }
    }
}
