using System.Collections;
using System.Collections.Generic;
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

    public void InitTile(Vector3 boardOrigin, Vector2 TilePos, TileColor tileColor)
    {
        sr = GetComponent<SpriteRenderer>();
        this.boardOrigin = boardOrigin;
        this.boardPosition = TilePos;
        this.tileColor = tileColor;

        if (tileColor == TileColor.White)
        {
            sr.color = new Color32(255, 220, 170, 255);
        }
        else if (tileColor == TileColor.Black)
        {
            sr.color = new Color32(115, 40, 5, 255);
        }

        transform.position = this.boardOrigin + (Vector3)this.boardPosition;
    }

    public void SetSprite(Sprite sprite)
    {
        sr.sprite = sprite;
    }
}
