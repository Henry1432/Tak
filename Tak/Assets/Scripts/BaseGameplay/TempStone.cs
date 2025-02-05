using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempStone
{
    public TileColor stoneColor = TileColor.None;
    public bool wall = false;
    public bool cap = false;
    Tile tile;

    public TempStone(TileColor color, bool wall, bool cap, Tile tile) 
    { 
        this.stoneColor = color;
        this.wall = wall;
        this.cap = cap;
        this.tile = tile;
    }

    public TempStone(Stone stone)
    {
        this.stoneColor = stone.stoneColor;
        this.wall = stone.wall;
        this.cap = stone.cap;
        this.tile = stone.currentTile;
    }
}
