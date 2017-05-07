//Student Name: George Alexandru Ciobanita
//Student ID: Q11598417
//Project: FINAL MAJOR PROJECT CGP601
//Class: PlayerCharacter
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour {

    //movement speed of the character
    public float characterSpeed=0.1f;
    //sprites specific to directional movement to signal this to the user
    public Sprite horizontalMovement;
    public Sprite verticalMovement;
    public Sprite horizontalAttack;
    public Sprite verticalAttack;
    public Sprite idle;
    //text to display leve completion
    public GameObject uiText;
    //components of the object
    SpriteRenderer characterSpriteRenderer;
    Rigidbody2D characterRB;
    //collider for the character attack
    CircleCollider2D attackCollider;
    //set the position of the character relative to its movement
    bool horizontalPositioning = false;
    //key is required for level completion
    bool hasKey=false;
    //current room location in the level
    Room levelLocation;
    Vector2 prevVelocity;


	//set up the components
	void Start () {
        characterSpriteRenderer = this.GetComponent<SpriteRenderer>();
        characterRB=this.GetComponent<Rigidbody2D>();
        attackCollider = this.GetComponent<CircleCollider2D>();
	}
	
	// Update is called once per frame
	void FixedUpdate()
    {
        Controls();
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        CharacterMovement(h, v);
    }

    //character movement to be either horizontal or vertical but never diagonal
    void CharacterMovement(float horizontal, float vertical)
    {

        if (vertical != 0f)
        {
            //no horizontal movement
            horizontal = 0f;
            //new movement vector
            Vector3 movement = new Vector3(horizontal, vertical, 0);
            //set the velocity
            characterRB.velocity = movement * characterSpeed;
            //and store the previous one
            prevVelocity = characterRB.velocity;
            return;
        }

        if (horizontal != 0f)
        {
            //no vertical movement
            vertical = 0f;
            Vector3 movement = new Vector3(horizontal, vertical, 0);
            characterRB.velocity = movement * characterSpeed;
            prevVelocity = characterRB.velocity;
            return;

        }
        else
        {
            //no input at all->no movement
            Vector3 noMovement = new Vector3(0, 0, 0);
            characterRB.velocity = noMovement;
        }
    }

    //controls determine what sprite is used, and its rotation, and set up the directional position bool
    void Controls()
    {
        if ((Input.GetAxis("Horizontal") > 0 || Input.GetAxis("Horizontal") < 0) && (Input.GetAxis("Vertical")==0))
        {
            attackCollider.enabled = false;
            characterSpriteRenderer.sprite = horizontalMovement;
            characterSpriteRenderer.flipX = (Input.GetAxis("Horizontal") > 0);
            horizontalPositioning = true;
        }
        else
        {
            if ((Input.GetAxis("Vertical") > 0 || Input.GetAxis("Vertical") < 0) && (Input.GetAxis("Horizontal") == 0))
            {
                attackCollider.enabled = false;
                characterSpriteRenderer.sprite = verticalMovement;
                characterSpriteRenderer.flipY = (Input.GetAxis("Vertical") > 0);
                horizontalPositioning = false;
            }
            else
            {
                //if the player attacks
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    attackCollider.enabled = true;
                    //depending on the characters position the collision box for the attack will be moved to the location represented in its attack sprites
                    if(horizontalPositioning)
                    {
                        characterSpriteRenderer.sprite = horizontalAttack;
                        if(prevVelocity.x>0)
                        {
                            attackCollider.offset = new Vector2(0.84f, 0);
                        }
                        else
                        {
                            attackCollider.offset = new Vector2(-0.84f, 0);
                        }
                    }
                    else
                    {
                        characterSpriteRenderer.sprite = verticalAttack;
                        if (prevVelocity.y > 0)
                        {
                            attackCollider.offset = new Vector2(0, 0.84f);
                        }
                        else
                        {
                            attackCollider.offset = new Vector2(0, -0.84f);
                        }
                    }
                }
            }
        }
    }

    public void SetLevelLocation(Room room)
    {
        levelLocation = room;
    }

    public Room GetLevelLocation()
    {
        return levelLocation;
    }

    public bool GetPlayerCharacterKey()
    {
        return hasKey;
    }

    public void LevelCompletionMessage()
    {
        uiText.gameObject.SetActive(true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag=="Key")
        {
            //get the key
            hasKey = true;
            collision.gameObject.SetActive(false);
        }
    }
}
