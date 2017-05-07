//Student Name: George Alexandru Ciobanita
//Student ID: Q11598417
//Project: FINAL MAJOR PROJECT CGP601
//Class: MagicOrb
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicOrb : MonoBehaviour {

    //the room that the orb object currently resides within
    Room parentRoom;

    //set its parent room
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
