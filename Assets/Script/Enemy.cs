using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    public float movementSpeed;
    public Sprite activeSprite;
    public Sprite normalSprite;
    Rigidbody2D enemyRB;
    SpriteRenderer enemySR;
    Room parentRoom;

    // Use this for initialization
    void Start () {
        enemyRB = this.GetComponent<Rigidbody2D>();
        enemySR = this.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void SetParentRoom(Room room)
    {
        parentRoom = room;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (enemyRB && enemySR)
        {
            if (other.gameObject.tag == "PlayerCharacter")
            {
                enemySR.sprite = activeSprite;
                int horizontalDistance = Mathf.Abs((int)(other.transform.position.x - this.transform.position.x));
                int verticalDistance = Mathf.Abs((int)(other.transform.position.y - this.transform.position.y));

                if (horizontalDistance < verticalDistance)
                {
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (enemyRB && enemySR)
        {
            if (collision.gameObject.tag != "PlayerCharacter")
            {
                Vector2 position;
                enemyRB.velocity = new Vector2(0, 0);
                enemySR.sprite = normalSprite;
                position.x = Mathf.Round(this.transform.position.x);
                position.y = Mathf.Round(this.transform.position.y);
                this.transform.position = position;
            }
            else
            {
                parentRoom.OpenRoomExits();
                this.gameObject.SetActive(false);
            }
        }
    }
}
