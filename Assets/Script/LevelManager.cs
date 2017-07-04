//Student Name: George Alexandru Ciobanita
//Student ID: Q11598417
//Project: FINAL MAJOR PROJECT CGP601
//Class: LevelManager
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{ 
    public IntRange RoomWidthRange;
    public IntRange RoomHeightRange;
    //max size of a section
    public int maxSectionSize = 30;
    //the initial section size
    public int SectionWidth = 70;
    public int SectionHeight = 70;
    //the tile gameobject for rooms
    public GameObject EntryContainer;
    //the specific sprite for each tile type currently available
    //all the rooms in the level
    List<Room> allRooms;
    //the tiles of all the passages in the level
    List<Position> passageTiles;
    //all the surrounding walls of a passage in the entire level
    List<Position> passageWalls;
    public RoomCreation roomCreator;
    public GameManager gameManager;
    public GraphicsManager graphicsManager;
    Tiletype[,] tiles;
    List<Section> sections;
    bool premadeRoom=true;

    //the entire process from creation to gameplay is started from the level managers completion
    void Start()
    {
        Initialiaze();

        //split the initial section
        SplitSection();

        //determine the rooms to be created
        CreateRoomsFromSection();

        //connect section rooms
        ConnectSectionRooms();

        //set up the entire list of rooms
        SetUpRoomList();

        //pass data required to draw on screen
        graphicsManager.AcquireLevelGraphicsData(allRooms, passageTiles, passageWalls);

        //pass the room list to set up gameplay
        gameManager.SetUpGameArea(allRooms);
    }
    //create rooms out of sections
    void CreateRoom(Section section)
    {
        List<Region> currentRegions = new List<Region>();

        System.Random randomNumberGeneration = new System.Random();

        //if the section has been split and need rooms to be created in its children
        if (section.leftSection != null || section.rightSection != null)
        {
            //create rooms in the child sections
            if (section.leftSection != null)
            {
                CreateRoom(section.leftSection);
            }
            if (section.rightSection != null)
            {
                CreateRoom(section.rightSection);
            }
        }
        else
        {
            //take section values/position into consideration
            int xPos;
            int yPos;
            int roomWidth;
            int roomHeight;

            //if this section is one that need a room
            xPos = section.location.tileX;
            yPos = section.location.tileY;
            //values are the same as the IntRange values for RoomWidth/HeightRange
            roomWidth = randomNumberGeneration.Next(RoomWidthRange.m_Min, section.width);
            roomHeight = randomNumberGeneration.Next(RoomHeightRange.m_Min, section.height);
            //generate the room
            if(premadeRoom==false)
            {
                roomCreator.GenerateRoom(roomWidth, roomHeight, ref tiles, ref currentRegions);
                //then give it to the section
                section.insideRoom = new Room(xPos, yPos, roomWidth, roomHeight, tiles, currentRegions, section);
                section.insideRoom.DetermineEnclosingWall();
            }
            else
            {
                roomCreator.GenerateRoom(ref roomWidth, ref roomHeight, ref tiles, ref currentRegions);
                //then give it to the section
                section.insideRoom = new Room(xPos, yPos, roomWidth, roomHeight, tiles, currentRegions, section);
                premadeRoom = false;
            }
        }
    }

    //very similar to how regions are connected within a room but now trying to connect them between sections
    void ConnectRegions(ref Room roomA, ref Room roomB, int xA, int yA, int xB, int yB)
    {
        int bestDistance = 0;
        int distanceBetweenRooms = 0;
        Position bestTileA = new Position();
        Position bestTileB = new Position();
        bool possibleConnectionFound = false;

        //find the closest region between the tworooms
        foreach (Region regionA in roomA.regionList)
        {
            foreach (Region regionB in roomB.regionList)
            {
                for (int i = 0; i < regionA.edgeTiles.Count; i++)
                {
                    for (int j = 0; j < regionB.edgeTiles.Count; j++)
                    {
                        Position tileA = regionA.edgeTiles[i];
                        tileA.tileX = tileA.tileX + xA;
                        tileA.tileY = tileA.tileY + yA;
                        Position tileB = regionB.edgeTiles[j];
                        tileB.tileX = tileB.tileX + xB;
                        tileB.tileY = tileB.tileY + yB;
                        distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            //add best cases 
                            bestTileA = tileA;
                            bestTileB = tileB;
                            possibleConnectionFound = true;
                        }
                    }
                }
            }
        }

        Vector3 position;

        //place the entry tiles at the location of the closest tiles in the regions
        roomA.roomTiles[bestTileA.tileX - roomA.originX, bestTileA.tileY - roomA.originY] = Tiletype.Entry;
        position = new Vector3(bestTileA.tileX, bestTileA.tileY);
        roomA.entryTiles.Add(Instantiate(EntryContainer, position, Quaternion.identity));
        //entry tiles and other entities get their parent room variable assigned
        roomA.PassParentRoom();
        roomB.roomTiles[bestTileB.tileX - roomB.originX, bestTileB.tileY - roomB.originY] = Tiletype.Entry;
        position = new Vector3(bestTileB.tileX, bestTileB.tileY);
        roomB.entryTiles.Add(Instantiate(EntryContainer, position, Quaternion.identity));
        roomB.PassParentRoom();
        //update eachothers neighbours list
        UpdateRoomNeighbours(roomA, roomB);
        RemoveEntranceWalls(roomA, roomB, bestTileA, bestTileB);

        //create passage ways between rooms
        DeterminePassageWay(bestTileA, bestTileB);
    }

    //find the closest rooms to be connected between sections
    void GetClosestRooms(List<Room> ListA, List<Room> ListB, ref Room left, ref Room right)
    {
        int distance = 0;
        int bestDistance = 0;
        bool connectionFound = false;
        foreach (Room roomA in ListA)
        {
            foreach (Room roomB in ListB)
            {
                distance = (int)(Mathf.Pow(roomA.originX - roomB.originX, 2) + Mathf.Pow(roomA.originY - roomB.originY, 2));
                if (distance < bestDistance || !connectionFound)
                {
                    bestDistance = distance;
                    connectionFound = true;
                    left = roomA;
                    right = roomB;
                }
            }
        }
    }

    void Initialiaze()
    {
        passageTiles = new List<Position>();
        passageWalls = new List<Position>();
        allRooms = new List<Room>();

        sections = new List<Section>();
        Section root = new Section(new Position(0, 0), SectionWidth, SectionHeight);
        sections.Add(root);
    }

    //split each section
    void SplitSection()
    {
        bool doSplit = true;
        while (doSplit)
        {
            //until spliting is required no longer
            doSplit = false;
            for (int i = 0; i < sections.Count; i++)
            {
                Section section = sections[i];
                if (section.leftSection == null && section.rightSection == null)
                {
                    //if its sizes are acceptable for splitting, else this must be a final section
                    if (section.width > maxSectionSize || section.height > maxSectionSize)
                    {
                        if (section.Split())
                        {
                            sections.Add(section.leftSection);
                            sections.Add(section.rightSection);
                            doSplit = true;
                        }
                    }
                }
            }
        }
    }

    //tranverse through each section
    void CreateRoomsFromSection()
    {
        //in a reverse order so we start with the children
        for (int i = sections.Count - 1; i >= 0; i--)
        {
            //if both sections are empty, create the rooms
            if (sections[i].leftSection == null && sections[i].rightSection == null)
            {
                CreateRoom(sections[i]);
            }
        }

        //set up the child rooms list that exist within each section, including rooms contained in grandchildren and on
        for (int i = 0; i < sections.Count - 1; i++)
        {
            sections[i].childSectionRooms = sections[i].GetChildSectionRooms();
        }
    }

    //when sections need to have their rooms connected
    void ConnectSectionRooms()
    {
        Room leftRoom;
        Room rightRoom;

        for (int i = sections.Count - 1; i >= 0; i--)
        {
            //if they have a room
            if (sections[i].leftSection != null & sections[i].rightSection != null)
            {
                //if the section contains more than 1 child aka is not an end section of the BSP tree
                if (sections[i].leftSection.childSectionRooms.Count > 1 || sections[i].rightSection.childSectionRooms.Count > 1)
                {
                    //find what rooms are closest between the two sections
                    leftRoom = new Room();
                    rightRoom = new Room();
                    GetClosestRooms(sections[i].leftSection.childSectionRooms, sections[i].rightSection.childSectionRooms, ref leftRoom, ref rightRoom);
                    //then connect
                    ConnectRegions(ref leftRoom, ref rightRoom, leftRoom.originX, leftRoom.originY, rightRoom.originX, rightRoom.originY);
                }
                else
                {
                    //if section has only one room, then just connect them
                    ConnectRegions(ref sections[i].leftSection.insideRoom, ref sections[i].rightSection.insideRoom, sections[i].leftSection.insideRoom.originX, sections[i].leftSection.insideRoom.originY, sections[i].rightSection.insideRoom.originX, sections[i].rightSection.insideRoom.originY);
                }
            }
        }
    }

    //create a passage way between two tiles
    void DeterminePassageWay(Position tileA, Position tileB)
    {
        List<Position> newPassageList = new List<Position>();
        //two new positions will be calculated in order to create a passageway from 4 different points, 1 for start, 1 for finish, and 2 inbetween when a turn is needed to create a passage
        Position newTileC = new Position(tileA);
        Position newTileD = new Position(tileB);
        int horizontalDistance = 0, verticalDistance = 0;
        horizontalDistance = Mathf.Abs(tileA.tileX - tileB.tileX);
        verticalDistance = Mathf.Abs(tileA.tileY - tileB.tileY);

        //determine the biggest distance between the start and finish tiles
        if (horizontalDistance < verticalDistance)
        {
            //the result determines the location of the other two tiles in between them
            if (tileA.tileY > tileB.tileY || tileA.tileY < tileB.tileY)
            {
                newTileC.tileX = tileA.tileX;
                newTileD.tileX = tileB.tileX;
                newTileC.tileY = Mathf.Abs(tileA.tileY + tileB.tileY) / 2;
                newTileD.tileY = Mathf.Abs(tileA.tileY + tileB.tileY) / 2;
            }
        }
        else
        {
            if (verticalDistance < horizontalDistance)
            {
                if (tileA.tileX > tileB.tileX || tileA.tileX < tileB.tileX)
                {
                    newTileC.tileY = tileA.tileY;
                    newTileD.tileY = tileB.tileY;
                    newTileC.tileX = Mathf.Abs(tileA.tileX + tileB.tileX) / 2;
                    newTileD.tileX = Mathf.Abs(tileA.tileX + tileB.tileX) / 2;
                }
            }
        }

        //tiles are added to a list
        Position pos;
        //create a line between each of the 4 points then add each point in the line to the list of points
        List<Position> line = roomCreator.GetLine(tileA, newTileC);
        for (int i = 1; i < line.Count; i++)
        {
            pos = new Position(line[i].tileX, line[i].tileY);
            newPassageList.Add(pos);
        }
       
        line = roomCreator.GetLine(newTileC, newTileD);
        foreach (Position position in line)
        {
            newPassageList.Add(new Position(position.tileX, position.tileY));
        }
        line = roomCreator.GetLine(newTileD, tileB);
        for (int j = 0; j < line.Count; j++)
        {
            pos = new Position(line[j].tileX, line[j].tileY);
            newPassageList.Add(pos);
        }

        //added to the main list of passages
        foreach (Position position in newPassageList)
        {
            passageTiles.Add(position);
        }

        //then the new tiles are passed to be surrounded by walls
        WallPassages(newPassageList, passageWalls);
    }

    void WallPassages(List<Position> passageList, List<Position> wallList)
    {
        //set up the first 3 tiles in the following positions
        Position prevTile = passageList[0];
        Position currTile = passageList[1];
        Position nextTile = passageList[passageList.Count - 1];
        //depending on what locations the neighbours of a tile have , they add accumulation depending on position
        int accumulation = 0;

        //accumulation is calculated between the first two tiles
        accumulation = GetAccumulation(prevTile, currTile);

        //it is checked and walls are created based on its value
        CheckAccumulation(accumulation, passageList, wallList, prevTile);

        //the curr tile is then changed
        currTile = passageList[passageList.Count - 2];

        //a new accumulation is created between the last and previous tile
        accumulation = GetAccumulation(currTile, nextTile);

        //walls are added
        CheckAccumulation(accumulation, passageList, wallList, nextTile);

        //then the rest of tiles that remained in the list are checked
        for (int i = 1; i < passageList.Count - 1; i++)
        {
            accumulation = 0;
            currTile = passageList[i];
            nextTile = passageList[i + 1];

            accumulation = GetAccumulation(prevTile, currTile, nextTile);

            CheckAccumulation(accumulation, passageList, wallList, currTile);

            prevTile = currTile;
        }
    }

    //based on what position a tile has against another tile on the X and Y axis, accumulation is calculated
    int GetAccumulation(Position prevTile, Position currTile, Position nextTile)
    {
        //this is specific to tiles that have the maximum of two neighbours
        int accumulation = 0;
        //tiles are checked to determine if they are on the same axis
        if (prevTile.tileX == currTile.tileX)
        {
            //left upwards or downwards
            if (prevTile.tileY > currTile.tileY)
            {
                accumulation += 2;
            }
            else
            {
                accumulation += 8;
            }
        }
        else
        {
            //left or right
            if (prevTile.tileX > currTile.tileX)
            {
                accumulation += 4;
            }
            else
            {
                accumulation += 1;
            }
        }

        //repeat
        if (currTile.tileX == nextTile.tileX)
        {
            if (nextTile.tileY > currTile.tileY)
            {
                accumulation += 2;
            }
            else
            {
                accumulation += 8;
            }
        }
        else
        {
            if (nextTile.tileX > currTile.tileX)
            {
                accumulation += 4;
            }
            else
            {
                accumulation += 1;
            }
        }

        return accumulation;
    }

    //this is specific for tiles that have 1 neighbour, specifically the first and last tile in a passage
    int GetAccumulation(Position aTile, Position bTile)
    {
        int accumulation = 0;

        if (aTile.tileX == bTile.tileX)
        {
            if (aTile.tileY > bTile.tileY)
            {
                accumulation += 2;
            }
            else
            {
                accumulation += 8;
            }
        }
        else
        {
            if (aTile.tileX > bTile.tileX)
            {
                accumulation += 4;
            }
            else
            {
                accumulation += 1;
            }
        }

        return accumulation;
    }

    //the total cases of accumulation is 10 depending on the possibles positions two tiles can have with another origin tile
    void CheckAccumulation(int accumulation, List<Position> passageList, List<Position> wallList, Position currTile)
    {
        switch (accumulation)
        {
            //a case is found then tiles are placed accordingly
            case 1:
                if (!passageList.Contains(new Position(currTile.tileX, currTile.tileY + 1)))
                {
                    wallList.Add(new Position(currTile.tileX, currTile.tileY + 1));
                }
                if (!passageList.Contains(new Position(currTile.tileX, currTile.tileY - 1)))
                {
                    wallList.Add(new Position(currTile.tileX, currTile.tileY - 1));
                }
                break;
            case 2:
                if (!passageList.Contains(new Position(currTile.tileX + 1, currTile.tileY)))
                {
                    wallList.Add(new Position(currTile.tileX + 1, currTile.tileY));
                }
                if (!passageList.Contains(new Position(currTile.tileX - 1, currTile.tileY)))
                {
                    wallList.Add(new Position(currTile.tileX - 1, currTile.tileY));
                }
                break;
            case 4:
                if (!passageList.Contains(new Position(currTile.tileX, currTile.tileY + 1)))
                {
                    wallList.Add(new Position(currTile.tileX, currTile.tileY + 1));
                }
                if (!passageList.Contains(new Position(currTile.tileX, currTile.tileY - 1)))
                {
                    wallList.Add(new Position(currTile.tileX, currTile.tileY - 1));
                }
                break;
            case 8:
                if (!passageList.Contains(new Position(currTile.tileX + 1, currTile.tileY)))
                {
                    wallList.Add(new Position(currTile.tileX + 1, currTile.tileY));
                }
                if (!passageList.Contains(new Position(currTile.tileX - 1, currTile.tileY)))
                {
                    wallList.Add(new Position(currTile.tileX - 1, currTile.tileY));
                }
                break;
            case 3:
                if (!passageList.Contains(new Position(currTile.tileX + 1, currTile.tileY)))
                {
                    wallList.Add(new Position(currTile.tileX + 1, currTile.tileY));
                }
                if (!passageList.Contains(new Position(currTile.tileX, currTile.tileY - 1)))
                {
                    wallList.Add(new Position(currTile.tileX, currTile.tileY - 1));
                }
                if (!passageList.Contains(new Position(currTile.tileX + 1, currTile.tileY - 1)))
                {
                    wallList.Add(new Position(currTile.tileX + 1, currTile.tileY - 1));
                }
                break;
            case 6:
                if (!passageList.Contains(new Position(currTile.tileX - 1, currTile.tileY)))
                {
                    wallList.Add(new Position(currTile.tileX - 1, currTile.tileY));
                }
                if (!passageList.Contains(new Position(currTile.tileX, currTile.tileY - 1)))
                {
                    wallList.Add(new Position(currTile.tileX, currTile.tileY - 1));
                }
                if (!passageList.Contains(new Position(currTile.tileX - 1, currTile.tileY - 1)))
                {
                    wallList.Add(new Position(currTile.tileX - 1, currTile.tileY - 1));
                }
                break;
            case 9:
                if (!passageList.Contains(new Position(currTile.tileX + 1, currTile.tileY)))
                {
                    wallList.Add(new Position(currTile.tileX + 1, currTile.tileY));
                }
                if (!passageList.Contains(new Position(currTile.tileX, currTile.tileY + 1)))
                {
                    wallList.Add(new Position(currTile.tileX, currTile.tileY + 1));
                }
                if (!passageList.Contains(new Position(currTile.tileX + 1, currTile.tileY + 1)))
                {
                    wallList.Add(new Position(currTile.tileX + 1, currTile.tileY + 1));
                }
                break;
            case 10:
                if (!passageList.Contains(new Position(currTile.tileX + 1, currTile.tileY)))
                {
                    wallList.Add(new Position(currTile.tileX + 1, currTile.tileY));
                }
                if (!passageList.Contains(new Position(currTile.tileX - 1, currTile.tileY)))
                {
                    wallList.Add(new Position(currTile.tileX - 1, currTile.tileY));
                }
                break;
            case 5:
                if (!passageList.Contains(new Position(currTile.tileX, currTile.tileY + 1)))
                {
                    wallList.Add(new Position(currTile.tileX, currTile.tileY + 1));
                }
                if (!passageList.Contains(new Position(currTile.tileX, currTile.tileY - 1)))
                {
                    wallList.Add(new Position(currTile.tileX, currTile.tileY - 1));
                }
                break;
            case 12:
                if (!passageList.Contains(new Position(currTile.tileX, currTile.tileY + 1)))
                {
                    wallList.Add(new Position(currTile.tileX, currTile.tileY + 1));
                }
                if (!passageList.Contains(new Position(currTile.tileX - 1, currTile.tileY)))
                {
                    wallList.Add(new Position(currTile.tileX - 1, currTile.tileY));
                }
                if (!passageList.Contains(new Position(currTile.tileX - 1, currTile.tileY + 1)))
                {
                    wallList.Add(new Position(currTile.tileX - 1, currTile.tileY + 1));
                }
                break;
        }
    }

    //the connecting tiles are behind the walling tiles of a room, this method removes said walls
    void RemoveEntranceWalls(Room roomA, Room roomB, Position entryTileA, Position entryTileB)
    {
        
        int horizontalDistance = 0, verticalDistance = 0;
        horizontalDistance = Mathf.Abs(entryTileA.tileX - entryTileB.tileX);
        verticalDistance = Mathf.Abs(entryTileA.tileY - entryTileB.tileY);
        int aX = 0, aY = 0, bX = 0, bY = 0;

        //in order to avoid this the direction from which the passage is being built is determined
        if (horizontalDistance > verticalDistance)
        {
            //to determine the direction of walls based on tile positions
            if (entryTileA.tileX < entryTileB.tileX)
            {
                //horizontally remove the tiles 
                aX = entryTileA.tileX + 1;
                aY = entryTileA.tileY;
                bX = entryTileB.tileX - 1;
                bY = entryTileB.tileY;
                roomA.roomTiles[aX - roomA.originX, aY - roomA.originY] = Tiletype.Null;
                roomB.roomTiles[bX - roomB.originX, bY - roomB.originY] = Tiletype.Null;
                roomA.wallTiles.Remove(new Position(aX - roomA.originX, aY - roomA.originY));
                roomB.wallTiles.Remove(new Position(bX - roomB.originX, bY - roomB.originY));
            }
            else
            {
                //still horizontally
                aX = entryTileA.tileX - 1;
                aY = entryTileA.tileY;
                bX = entryTileB.tileX + 1;
                bY = entryTileB.tileY;
                roomA.roomTiles[aX - roomA.originX, aY - roomA.originY] = Tiletype.Null;
                roomB.roomTiles[bX - roomB.originX, bY - roomB.originY] = Tiletype.Null;
                roomA.wallTiles.Remove(new Position(aX - roomA.originX, aY - roomA.originY));
                roomB.wallTiles.Remove(new Position(bX - roomB.originX, bY - roomB.originY));
            }
        }
        else
        {
            if (entryTileA.tileY < entryTileB.tileY)
            {
                aX = entryTileA.tileX;
                aY = entryTileA.tileY + 1;
                bX = entryTileB.tileX;
                bY = entryTileB.tileY - 1;
                roomA.roomTiles[aX - roomA.originX, aY - roomA.originY] = Tiletype.Null;
                roomB.roomTiles[bX - roomB.originX, bY - roomB.originY] = Tiletype.Null;
                roomA.wallTiles.Remove(new Position(aX - roomA.originX, aY - roomA.originY));
                roomB.wallTiles.Remove(new Position(bX - roomB.originX, bY - roomB.originY));
            }
            else
            {
                aX = entryTileA.tileY;
                aY = entryTileA.tileY - 1;
                bX = entryTileB.tileX;
                bY = entryTileB.tileY + 1;
                roomA.roomTiles[aX - roomA.originX, aY - roomA.originY] = Tiletype.Null;
                roomB.roomTiles[bX - roomB.originX, bY - roomB.originY] = Tiletype.Null;
                roomA.wallTiles.Remove(new Position(aX - roomA.originX, aY - roomA.originY));
                roomB.wallTiles.Remove(new Position(bX - roomB.originX, bY - roomB.originY));
            }
        }
    }

    //call room add neighbour method between rooms
    void UpdateRoomNeighbours(Room roomA, Room roomB)
    {
        roomA.AddNeighbours(roomB);
        roomB.AddNeighbours(roomA);
    }

    void SetUpRoomList()
    {
        foreach (Room room in sections[0].childSectionRooms)
        {
            allRooms.Add(room);
        }
    }
}
