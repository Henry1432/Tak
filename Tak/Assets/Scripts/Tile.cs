using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TileColor
{
    None = -1,
    White = 0,
    Black = 1,
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    public TileColor tileColor = TileColor.None;
    public List<Stone> stonesOnTile = new List<Stone>();
    public Vector2 boardPosition = -Vector2.one;
    private Vector3 boardOrigin;
    private SpriteRenderer sr;

    private void Start()
    {
        if(sr == null)
            sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        for (int i = 0; i < stonesOnTile.Count; i++)
        {
            stonesOnTile[i].transform.localPosition = new Vector3(stonesOnTile[i].transform.localPosition.x, stonesOnTile[i].transform.localPosition.y, -(i+1));
        }
    }

    public void InitTile(Vector3 boardOrigin, Vector2 TilePos, TileColor tileColor)
    {
        sr = GetComponent<SpriteRenderer>();
        this.boardOrigin = boardOrigin;
        this.boardPosition = TilePos;
        this.tileColor = tileColor;

        if (tileColor == TileColor.White)
        {
            sr.color = new Color32(255, 209, 163, 255);
        }
        else if (tileColor == TileColor.Black)
        {
            sr.color = new Color32(97, 47, 0, 255);
        }

        transform.position = this.boardOrigin + (Vector3)this.boardPosition;
    }

    public void SetSprite(Sprite sprite)
    {
        sr.sprite = sprite;
    }

    public int getWhiteStonesOnTile()
    {
        int whiteStonesOnTile = 0;
        foreach(Stone stone in stonesOnTile)
        {
            if(stone.stoneColor == TileColor.White)
            {
                whiteStonesOnTile++;
            }
        }

        return whiteStonesOnTile;
    }

    public int getBlackStonesOnTile()
    {
        int blackStonesOnTile = 0;
        foreach (Stone stone in stonesOnTile)
        {
            if (stone.stoneColor == TileColor.Black)
            {
                blackStonesOnTile++;
            }
        }

        return blackStonesOnTile;
    }
}
