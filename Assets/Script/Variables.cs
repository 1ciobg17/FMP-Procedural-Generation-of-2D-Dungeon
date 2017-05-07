//Student Name: George Alexandru Ciobanita
//Student ID: Q11598417
//Project: FINAL MAJOR PROJECT CGP601
//Classes & Variables: Tiletype, EntityType, Entity, Passage, Room, Position, Region, Section, IntRange
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

//tile types used especially when creating the room array and drawing the rooms
//noted: struct set up improper, noted in report
[Serializable]
public enum Tiletype
{
    Wall, Floor, Connector, Entry, Null,
}

//entities that can be found in a level
[Serializable]
public enum EntityType
{
    Null, DungeonEntry, DungeonExit, MagicOrb, Enemy, Key,
}

//entity struct
//does not need to be a class as its existance is at a level that is not required any feedback
[Serializable]
public struct Entity
{
    public int originX;
    public int originY;
    public EntityType entityType;
    public Room parentRoom;

    public Entity(int X, int Y, EntityType type, Room room)
    {
        originX = X;
        originY = Y;
        entityType = type;
        parentRoom = room;
    }
}

//struct used to represent the passages that connect each room
[Serializable]
public struct Passage
{
    //passage origin, where it is starting from
    public int originX;
    public int originY;
    //what tiles it contains 
    public Tiletype[,] passageTiles;
    //the rooms it connects
    public Room firstRoom;
    public Room secondRoom;

    //constructor
    public Passage(int x, int y, Tiletype[,] tiles, Room A, Room B)
    {
        originX = x;
        originY = y;
        passageTiles = tiles;
        firstRoom = A;
        secondRoom = B;
    }
}



[Serializable]
public class Room
{
    //room origin, the bottom left corner of the room
    public int originX;
    public int originY;
    //width and height, either random or section size
    public int height;
    public int width;
    //all the tiles in a room
    [NonSerialized]
    public Tiletype[,] roomTiles;
    //all the tiles separated in regions
    [NonSerialized]
    public List<Region> regionList;
    //all the wall tiles that surround the room
    [NonSerialized]
    public List<Position> wallTiles;
    [NonSerialized]
    public List<GameObject> entryTiles;
    [NonSerialized]
    public List<Room> neighbouringRooms;
    [NonSerialized]
    public bool hasBeenVisited = false;

    //default constructor
    public Room()
    {

    }

    //constructor
    public Room(int x, int y, int H, int W, Tiletype[,] tiles, List<Region> regions, Section section)
    {
        roomTiles = tiles;
        originX = x;
        originY = y;
        height = H;
        width = W;
        //variables that need to be set up in order to be used 
        regionList = new List<Region>();
        regionList = regions;
        wallTiles = new List<Position>();
        entryTiles = new List<GameObject>();
        neighbouringRooms = new List<Room>();
    }

    //soemthing to change the room position if needed
    public void SetPos(int x, int y)
    {
        originX = x;
        originY = y;
    }

    //determine the wall tiles that are at the edge of a room
    public void DetermineEnclosingWall()
    {
        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                //if the tile is a wall then check its surroundings and add it to the list
                if (roomTiles[x, y] == Tiletype.Wall)
                {
                    if ((y > 0) && roomTiles[x, y - 1] == Tiletype.Floor)
                    {
                        wallTiles.Add(new Position(x, y));
                    }
                    else
                    {
                        if ((y < width - 1) && roomTiles[x, y + 1] == Tiletype.Floor)
                        {
                            wallTiles.Add(new Position(x, y));
                        }
                        else
                        {
                            if ((x > 0) && roomTiles[x - 1, y] == Tiletype.Floor)
                            {
                                wallTiles.Add(new Position(x, y));
                            }
                            else
                            {
                                if ((x < height - 1) && roomTiles[x + 1, y] == Tiletype.Floor)
                                {
                                    wallTiles.Add(new Position(x, y));
                                }
                                else
                                {
                                    roomTiles[x, y] = Tiletype.Null;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    //for gameplay purposes
    //close the room off by calling its entry tiles
    public void CloseOffRoomExits()
    {
        foreach(GameObject tile in entryTiles)
        {
            tile.GetComponent<EntryTile>().BlockOffExits();
            tile.GetComponent<BoxCollider2D>().isTrigger = false;
        }
    }

    //open the rooms by calling its entry tiles
    public void OpenRoomExits()
    {
        foreach (GameObject tile in entryTiles)
        {
            tile.GetComponent<EntryTile>().OpenExits();
        }
    }

    //pass this room as the parent room once its entry tiles have been assigned
    public void PassParentRoom()
    {
        foreach(GameObject tile in entryTiles)
        { 
            tile.GetComponent<EntryTile>().GetParentRoom(this);
        }
    }

    //neighbouring rooms connected to the current room
    public void AddNeighbours(Room room)
    {
        neighbouringRooms.Add(room);
    }
}

//trying to move away from using unity too much
//creating personal containers for positions
[Serializable]
public struct Position
{
    public int tileX;
    public int tileY;

    //the constructor
    public Position(int x, int y)
    {
        tileX = x;
        tileY = y;
    }

    public Position(Position pos)
    {
        tileX = pos.tileX;
        tileY = pos.tileY;
    }

    public bool IsEqual(Position pos)
    {
        if(tileX==pos.tileX && tileY==pos.tileY)
        {
            return true;
        }
        return false;
    }
}

[Serializable]
//class required to better understand regions when trying to connect them - renamed to Room
public class Region : IComparable<Region>
{
    //a list of all the tiles in the region
    [NonSerialized]
    public List<Position> tiles;
    //a list of only the tiles at the edge of a room, as to avoid time wasted by checking all tiles for the smallest distance betweeen regions
    [NonSerialized]
    public List<Position> edgeTiles;
    //a list of all regions directly connected to one another
    [NonSerialized]
    public List<Region> connectedRegions;
    //number of tiles
    public int regionSize;
    //if the region is accessible from the main region, required in order to adress connectivity in the room
    public bool isAccessibleFromMainRegion;
    //if this region is the biggest then it becomes the main region
    public bool isMainRegion;

    //default constructor
    public Region()
    {
    }

    //public constructor
    public Region(List<Position> regionTiles, Tiletype[,] map)
    {
        tiles = regionTiles;
        regionSize = tiles.Count;
        connectedRegions = new List<Region>();

        edgeTiles = new List<Position>();
        foreach (Position tile in tiles)
        {
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    //only tiles horizontally or vertically positioned, ignoring diagonal tiles
                    if ((x == tile.tileX || y == tile.tileY))
                    {
                        //if any neighbouring tile is of wall type then it is an edge tile
                        if (map[x, y] == Tiletype.Floor)
                        {
                            edgeTiles.Add(tile);
                        }
                    }
                }
            }
        }
    }

    //constructor for regions part of a room created from a text tile
    public Region(List<Position> regionTiles, Tiletype[,] map, int mapW, int mapH)
    {
        //the region is the entirety of tiles that exist in the room
        tiles = regionTiles;
        regionSize = tiles.Count;
        connectedRegions = new List<Region>();

        //and the edge tiles are determined based on their position in the room
        edgeTiles = new List<Position>();
        for(int i=0; i<mapW; i++)
        {
            for(int j=0; j<mapH; j++)
            {
                if(map[i,j]==Tiletype.Floor)
                {
                    for (int x = i - 1; x <= i + 1; x++)
                    {
                        for (int y = j - 1; y <= j + 1; y++)
                        {
                            //only tiles horizontally or vertically positioned, ignoring diagonal tiles
                            if ((x == i || y == j))
                            {
                                //if any neighbouring tile is of wall type then it is an edge tile
                                if (map[x, y] == Tiletype.Floor)
                                {
                                    edgeTiles.Add(new Position(x,y));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    //set the accessability once it is possible to travel to the main region from the current region
    public void SetAccessibleFromMainRegion()
    {
        if (!isAccessibleFromMainRegion)
        {
            isAccessibleFromMainRegion = true;
            foreach (Region connectedRegion in connectedRegions)
            {
                //also set all regions connected to the current region that they are accessible
                connectedRegion.SetAccessibleFromMainRegion();
            }
        }
    }

    //set the accessability and add each room to each others list
    public static void ConnectRegions(Region regionA, Region regionB)
    {
        //if one region has access to the main region then the other should also have access now
        if (regionA.isAccessibleFromMainRegion)
        {
            regionB.SetAccessibleFromMainRegion();
        }
        else //same goes for the reverse
            if (regionB.isAccessibleFromMainRegion)
            {
                regionA.SetAccessibleFromMainRegion();
            }

        //then add each to the other's list of connected rooms
        regionA.connectedRegions.Add(regionB);
        regionB.connectedRegions.Add(regionA);
    }

    //check connection with other regions
    public bool IsConnected(Region otherRegion)
    {
        return connectedRegions.Contains(otherRegion);
    }

    //compare region sizes for the IComparable functionality, later used for sorting
    public int CompareTo(Region otherRegion)
    {
        return otherRegion.regionSize.CompareTo(regionSize);
    }
}

//sections/leafs of a BSP tree
//used in partitioning
[Serializable]
public class Section
{
    //minimum size of regions
    private const int SectionSize = 15;
    //region location, bottom right corner
    public Position location;
    //its size
    public int width;
    public int height;

    //its child sections after splitting
    public Section leftSection;
    public Section rightSection;
    //room contained in the region
    public Room insideRoom;
    //child rooms(part of child sections) contained in this region
    public List<Room> childSectionRooms;

    public Section()
    {
    }

    //constructor
    public Section(Position pos, int W, int H)
    {
        location = new Position(pos);
        width = W;
        height = H;
        childSectionRooms = new List<Room>();
    }

    //split the room either vertically or horizontally
    //this is where child sections are set
    public bool Split()
    {
        //randomly choose how
        System.Random randomNumberGeneration = new System.Random();

        //50/50
        bool splitHorizontally = ((randomNumberGeneration.Next(100) <= 50) ? true : false);

        //if this section was already split
        if (leftSection != null && rightSection != null)
        {
            return false;
        }

        //if the width is atleast 1/4 higher than the height then we force the split to be vertical
        if (width + width / 4 >= height)
        {
            splitHorizontally = false;
        }
        else
        {
            //the opposite
            if (height + height / 4 >= width)
            {
                splitHorizontally = true;
            }
        }

        //can the section be split anymore, in either of those directions?
        int max = (splitHorizontally ? height : width) - SectionSize;

        //if its not possible
        if (max < SectionSize)
        {
            return false;
        }

        //select split point randomly
        int split = randomNumberGeneration.Next(SectionSize, max);
        Position newPos;

        //now split and create the child sections
        if (splitHorizontally)
        {
            leftSection = new Section(location, width, split);
            newPos = new Position(location.tileX, location.tileY + split);
            rightSection = new Section(newPos, width, height - split);
        }
        else
        {
            leftSection = new Section(location, split, height);
            newPos = new Position(location.tileX + split, location.tileY);
            rightSection = new Section(newPos, width - split, height);
        }

        return true; // split successful
    }

    //get the rooms of ALL child sections(child of child and on..)
    public List<Room> GetChildSectionRooms()
    {
        //create a list
        List<Room> sectionRooms = new List<Room>();
        //add this sections room to the list
        if (this.insideRoom != null)
        {
            sectionRooms.Add(this.insideRoom);
        }
        else
        {
            //if it doesn't actually have a room aka is not a final sections
            //go after its child sections
            if (this.leftSection != null)
            {
                sectionRooms.AddRange(this.leftSection.GetChildSectionRooms());
            }
            if (this.rightSection != null)
            {
                sectionRooms.AddRange(this.rightSection.GetChildSectionRooms());
            }
        }

        //return the list
        return sectionRooms;
    }
}

    [Serializable]
    public class IntRange
    {
        public int m_Min;//min value
        public int m_Max;//max value

        //constructor
        public IntRange(int min, int max)
        {
            m_Min = min;
            m_Max = max;
        }

        //get random value from the range
        public int Random()
        {
            return UnityEngine.Random.Range(m_Min, m_Max);
        }
}