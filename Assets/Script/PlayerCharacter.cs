using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour {

    public float characterSpeed=0.1f;
    public Sprite horizontalMovement;
    public Sprite verticalMovement;
    public Sprite horizontalAttack;
    public Sprite verticalAttack;
    public Sprite idle;
    public GameObject uiText;
    SpriteRenderer characterSpriteRenderer;
    Rigidbody2D characterRB;
    CircleCollider2D attackCollider;
    bool horizontalPositioning = false;
    bool hasKey=false;
    Room levelLocation;
    Vector2 prevVelocity;


	// Use this for initialization
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

    void CharacterMovement(float horizontal, float vertical)
    {

        if (vertical != 0f)
        {
            horizontal = 0f;
            Vector3 movement = new Vector3(horizontal, vertical, 0);
            characterRB.velocity = movement * characterSpeed;
            prevVelocity = characterRB.velocity;
            return;
        }

        if (horizontal != 0f)
        {
            vertical = 0f;
            Vector3 movement = new Vector3(horizontal, vertical, 0);
            characterRB.velocity = movement * characterSpeed;
            prevVelocity = characterRB.velocity;
            return;

        }
        else
        {
            Vector3 noMovement = new Vector3(0, 0, 0);
            characterRB.velocity = noMovement;
        }
    }

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
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    attackCollider.enabled = true;
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
            hasKey = true;
            collision.gameObject.SetActive(false);
        }
    }
}
