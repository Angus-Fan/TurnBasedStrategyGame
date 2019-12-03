using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Tile
{

    public string name;
    public GameObject tileVisualPrefab;
    public GameObject unitOnTile;
    public float movementCost = 1;
    public bool isWalkable=true;
    
    /*
    private int x;
    private int y;

    
   
    
    public Tile( int xLocation, int yLocation)
    {
        x = xLocation;
        y = yLocation;
    }

    public void setCoords(int xLocation, int yLocation)
    {
        x = xLocation;
        y = yLocation;
    }
    */
}
