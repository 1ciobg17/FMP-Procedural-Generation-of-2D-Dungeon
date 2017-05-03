using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntryTile : MonoBehaviour {

    Room parentRoom;
    public Sprite wall;
    public Sprite floor;
    BoxCollider2D boxCollider;
    SpriteRenderer spriteRenderer;
    Vector3 characterVelocity;
    Vector3 characterPosition;
    bool active = true;

	// Use this for initialization
	void Start () {
        boxCollider=this.GetComponent<BoxCollider2D>();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = floor;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void BlockOffExits()
    {
        spriteRenderer.sprite = wall;
        boxCollider.enabled = true;
    }

    public void OpenExits()
    {
        spriteRenderer.sprite = floor;
        boxCollider.enabled = false;
    }

    public void GetParentRoom(Room room)
    {
        parentRoom = room;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "PlayerCharacter")
        {
            characterVelocity = collision.GetComponent<Rigidbody2D>().velocity;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (active)
        {
            if (collision.tag == "PlayerCharacter")
            {
                characterPosition = collision.transform.position;
                if (collision.GetComponent<PlayerCharacter>().GetLevelLocation().originX != parentRoom.originX || collision.GetComponent<PlayerCharacter>().GetLevelLocation().originY != parentRoom.originY)
                {
                    active = false;
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

    public void DeactivateTrap()
    {
        active = false;
    }
}
