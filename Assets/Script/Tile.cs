using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour, PoolableTile {

    public void ReturnToPool()
    {
        TilePool.Pool.Add(this);
        this.gameObject.SetActive(false);
    }

    public void Reset()
    {
        this.gameObject.SetActive(true);
    }

    public void Spawn(GameObject obj)
    {

    }
}


