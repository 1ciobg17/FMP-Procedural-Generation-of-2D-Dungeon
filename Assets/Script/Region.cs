using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public enum Tiletype
{
    Wall, Floor, Test,
}

[Serializable]
public enum RoomDirection
{
    Left, Up, Right, Down, Impossible, 
}

[Serializable]
public struct Room
{
    public int originX;
    public int originY;
    public int height;
    public int width;
    public Tiletype[,] roomTiles;

    public Room(int x, int y, int H, int W, Tiletype[,] tiles)
    {
        roomTiles = tiles;
        originX = x;
        originY = y;
        height = H;
        width = W;
    }
}

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
                    if (x == tile.tileX || y == tile.tileY)
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