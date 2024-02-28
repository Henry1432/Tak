using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GameController : MonoBehaviour
{
    public static GameController instance;

    public TileColor currentTurn = TileColor.White;
    public TileColor placeColor = TileColor.Black;
    private int firstPlaces = 0;
    private SpriteRenderer sr;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (instance.firstPlaces > 1)
        {
            instance.placeColor = instance.currentTurn;
        }
        else
        {
            instance.placeColor = instance.currentTurn == TileColor.White ? TileColor.Black : TileColor.White;
        }

        sr.color = currentTurn == TileColor.White ? Color.white : Color.black;
    }

    public static void swapTurn ()
    {
        instance.currentTurn = instance.currentTurn == TileColor.White ? TileColor.Black : TileColor.White;
    }

    public static void placeStone()
    {
        swapTurn();
        instance.firstPlaces++;
    }

    public static bool canWall()
    {
        return (instance.firstPlaces > 1);
    }
}
