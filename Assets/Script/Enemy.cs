//Student Name: George Alexandru Ciobanita
//Student ID: Q11598417
//Project: FINAL MAJOR PROJECT CGP601
//Class: Enemy
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    //affects the velocity of the enemy object
    public float movementSpeed;
    //for the enemy currently present in game, active or in motion
    public Sprite activeSprite;
    //for when the enemy is not in motion 
    public Sprite normalSprite;
    //rigidbody component
    Rigidbody2D enemyRB;
    //sprite render in order to have access and change sprites
    SpriteRenderer enemySR;
    //room it currently resides in
    Room parentRoom;

    // Use this for initialization
    void Start () {
        //acquire access to components
        enemyRB = this.GetComponent<Rigidbody2D>();
        enemySR = this.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {

	}

    //each enemy is part of a room, helps in sending responses back related to player interaction
    public void SetParentRoom(Room room)
    {
        parentRoom = room;
    }

    //when other objects interact with the enemy's trigger box
    private void OnTriggerEnter2D(Collider2D other)
    {
        //if the enemy has been declared
        if (enemyRB && enemySR)
        {
            //and the triggering object is the player character
            if (other.gameObject.tag == "PlayerCharacter")
            {
                enemySR.sprite = activeSprite;
                //find out how exactly is the player positioned towards the current enemy then move the enemy in a direction related to the player
                int horizontalDistance = Mathf.Abs((int)(other.transform.position.x - this.transform.position.x));
                int verticalDistance = Mathf.Abs((int)(other.transform.position.y - this.transform.position.y));

                //in a vertical direction
                if (horizontalDistance < verticalDistance)
                {
                    //up or down
                    if (other.transform.position.y > this.transform.position.y)
                    {
                        enemyRB.velocity = new Vector2(0, movementSpeed);
                    }
                    else
                    {
                        enemyRB.velocity = new Vector2(0, -movementSpeed);
                    }
                }
                else
                {
                    //horizontal direction
                    //left or right
                    if (other.transform.position.x > this.transform.position.x)
                    {
                        enemyRB.velocity = new Vector2(movementSpeed, 0);
                    }
                    else
                    {
                        enemyRB.velocity = new Vector2(-movementSpeed, 0);
                    }
                }
            }
        }
    }

    //on hitbox collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (enemyRB && enemySR)
        {
            //anything that is not a player and is hit stops the enemy
            if (collision.gameObject.tag != "PlayerCharacter")
            {
                Vector2 position;
                enemyRB.velocity = new Vector2(0, 0);
                //enemy is deactivated and its position rounded in order to be centered on the tiles
                enemySR.sprite = normalSprite;
                position.x = Mathf.Round(this.transform.position.x);
                position.y = Mathf.Round(this.transform.position.y);
                this.transform.position = position;
            }
            else
            {
                //else the player is hit 
                //signal the parent room to open the exits
                parentRoom.OpenRoomExits();
                //deactivate object
                this.gameObject.SetActive(false);
            }
        }
    }
}
