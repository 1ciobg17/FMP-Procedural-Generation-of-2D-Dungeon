using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicOrb : MonoBehaviour {

    Room parentRoom;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetParentRoom(Room room)
    {
        parentRoom = room;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag=="PlayerCharacter")
        {
            parentRoom.OpenRoomExits();
            this.gameObject.SetActive(false);
        }
    }
}
