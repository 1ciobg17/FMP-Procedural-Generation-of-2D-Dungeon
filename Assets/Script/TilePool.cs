using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilePool : MonoBehaviour {

    public static List<Tile> Pool = new List<Tile>();
    public GameObject obj;
    public int SpawnCount;
    public float WaitTime = 0.5f;

	// Use this for initialization
	void Start () {
		for(int i=0; i<=SpawnCount; i++)
		{
			GameObject GO = (GameObject) Instantiate(obj);
			Pool.Add (GO.GetComponent<Tile> ());
			GO.SetActive(false); // set active to false
		}
		StartCoroutine (Spawn());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator Spawn()
    {
        while(true)
        {
            if(Pool.Count>0)
			{
				Tile ObjectToSpawn = Pool[0];
				Pool.Remove(ObjectToSpawn);
				//ObjectToSpawn.Spawn (this.gameObject);
			}

			yield return new WaitForSeconds(WaitTime);
        }
    }
}