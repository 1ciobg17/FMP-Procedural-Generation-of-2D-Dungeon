//Student Name: George Alexandru Ciobanita
//Student ID: Q11598417
//Project: FINAL MAJOR PROJECT CGP601
//Class: EntryTile
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryTile : MonoBehaviour {

    //room it currently residents in
    Room parentRoom;
    //sprites that represent its states
    //closing off the room
    public Sprite wall;
    //and when the room is open for entry/exit
    public Sprite floor;
    //used to access the functions of the object
    BoxCollider2D boxCollider;
    SpriteRenderer spriteRenderer;
    //vectors for characters that it interacts with
    Vector3 characterVelocity;
    Vector3 characterPosition;
    //used to permanently deactivate an entry tile once the room has been opened once
    bool active = true;

	// Use this for initialization
	void Start () {
        //gain access to components
        boxCollider=this.GetComponent<BoxCollider2D>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        //and set the initial state of being open, sprite wise
        spriteRenderer.sprite = floor;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //called by the room when entrances are to be closed
    public void BlockOffExits()
    {
        spriteRenderer.sprite = wall;
        boxCollider.enabled = true;
    }

    //the exact opposite
    public void OpenExits()
    {
        spriteRenderer.sprite = floor;
        boxCollider.enabled = false;
    }

    //set its parent room, currently residing 
    public void GetParentRoom(Room room)
    {
        parentRoom = room;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //when an object initially enters the trigger box acquire its velocity
        if (collision.tag == "PlayerCharacter")
        {
            characterVelocity = collision.GetComponent<Rigidbody2D>().velocity;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //if the this entry tile is part of has not been entered by the player character before
        if (active)
        {
            if (collision.tag == "PlayerCharacter")
            {
                //get the character position
                characterPosition = collision.transform.position;
                //if the character object is currently in the bounds of the parent room
                if (collision.GetComponent<PlayerCharacter>().GetLevelLocation().originX != parentRoom.originX || collision.GetComponent<PlayerCharacter>().GetLevelLocation().originY != parentRoom.originY)
                {
                    //deactivate
                    active = false;
                    //check the location of the player in order to properly set the leve location and trigger the room passages to close off
                    if (characterVelocity.x == 0)
                    {
                        if (characterVelocity.y < 0)
                        {
                            if (characterPosition.y < this.transform.position.y)
                            {
                                parentRoom.CloseOffRoomExits();
                                collision.GetComponent<PlayerCharacter>().SetLevelLocation(parentRoom);
                                parentRoom.hasBeenVisited = true;
                            }
                        }
                        else
                        {
                            if (characterVelocity.y > 0)
                            {
                                if (characterPosition.y > this.transform.position.y)
                                {
                                    parentRoom.CloseOffRoomExits();
                                    collision.GetComponent<PlayerCharacter>().SetLevelLocation(parentRoom);
                                    parentRoom.hasBeenVisited = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (characterVelocity.y == 0)
                        {
                            if (characterVelocity.x < 0)
                            {
                                if (characterPosition.x < this.transform.position.x)
                                {
                                    parentRoom.CloseOffRoomExits();
                                    collision.GetComponent<PlayerCharacter>().SetLevelLocation(parentRoom);
                                    parentRoom.hasBeenVisited = true;
                                }
                            }
                            else
                            {
                                if (characterVelocity.x > 0)
                                {
                                    if (characterPosition.x > this.transform.position.x)
                                    {
                                        parentRoom.CloseOffRoomExits();
                                        collision.GetComponent<PlayerCharacter>().SetLevelLocation(parentRoom);
                                        parentRoom.hasBeenVisited = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    //set entry tiles to never react towards character collisions
    public void DeactivateTrap()
    {
        active = false;
    }
}
