//Student Name: George Alexandru Ciobanita
//Student ID: Q11598417
//Project: FINAL MAJOR PROJECT CGP601
//Class: LevelCompletion
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCompletion : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //trigger the player victory message then restart the scene
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag=="PlayerCharacter")
        {
            if(collision.gameObject.GetComponent<PlayerCharacter>().GetPlayerCharacterKey())
            {
                collision.gameObject.GetComponent<PlayerCharacter>().LevelCompletionMessage();
                GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().RestartLevel();
            }
        }
    }
}
