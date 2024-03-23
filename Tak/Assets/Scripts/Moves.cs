using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

//trying to keep this small and eficient because I think this will be inportant
public class Moves
{
    private char placeStone; // 'n' = not placing a stone, 'w' = placing white stone, 'b' = placing black stone, capital letter means Capstone
    private char wall; // 'n' = not placing a stone, 't' = placing a wall, 'f' = placing path
    private short originX; //start pos on the x axis
    private short originY; //start pos on the y axis
    private char direction; //'n' = not moving tiles, 'u' = up, 'r' = right, 'd' = down, 'l' = left
    private short dist; //number of tiles this move is moving, -1 if not moving
    private short abandon; //the number of stones to abandon on the start tile when moving a stone, -1 if not moving

    public Moves(char placeStone, char wall, short originX, short originY)
    {
        this.placeStone = placeStone;
        this.wall = wall;
        this.originX = originX;
        this.originY = originY;
        this.direction = 'n';
        this.dist = -1;
        this.abandon = -1;
    }
    public Moves(short originX, short originY, char direction, short dist, short abandon)
    {
        this.placeStone = 'n';
        this.wall = 'n';
        this.originX = originX;
        this.originY = originY;
        this.direction = direction;
        this.dist = dist;
        this.abandon = abandon;
    }

    public Moves(char placeStone, char wall, Vector2 origin) : this(placeStone, wall, (short)origin.x, (short)origin.y) { }
    public Moves(Vector2 origin, char direction, short dist, short abandon) : this((short)origin.x, (short)origin.y, direction, dist, abandon) { }

    public char getPlaceStone() { return placeStone; }

    public short getOriginX() {  return originX; }
    public short getOriginY() {  return originY; }
    public Vector2 getOrigin() { return new Vector2(originX, originY); }

    public char getWall() { return wall; }
    public bool isWall() { return (wall == 't'); }
    public bool isCapstone() { return (placeStone == 'W' || placeStone == 'B'); }

    public char getDirection() { return direction; }
    public short getDist() { return dist; }
    public short getAbandon() { return abandon; }

    public bool isPlaceStone() { return placeStone != 'n'; }
    public bool isMoveStone() { return direction != 'n'; }

    public TileColor getPlaceStoneColor() 
    { 
        if(placeStone == 'w' || placeStone == 'W')
        {
            return TileColor.White;
        }
        else if (placeStone == 'b' || placeStone == 'B')
        {
            return TileColor.Black;
        }
        return TileColor.None;

    }
}
