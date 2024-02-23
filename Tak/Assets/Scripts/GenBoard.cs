using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GenBoard : MonoBehaviour
{
    public static GenBoard instance;
    public Dictionary<(float,float), Tile> board = new Dictionary<(float, float), Tile>();
    public int maxTiles;
    public bool extraCapstone = false;

    [SerializeField] private float sizeOfBoard;
    [SerializeField] private Sprite sprite;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        transform.position = transform.position - (Vector3)((Vector2.one/2) * sizeOfBoard);

        if (sizeOfBoard == 5)
        {
            maxTiles = 21;
        }
        else if (sizeOfBoard == 6)
        {
            maxTiles = 30;
        }
        else if (sizeOfBoard == 7)
        {
            maxTiles = 40;
            extraCapstone = true;
        }
    }

    private void Update()
    {
        if(board.Count == 0)
        {
            Generate();
            StoneShow.instance.initRenderers();
        }
    }

    public void Generate()
    {
        bool swap = false;
        for(int y = 0;  y < sizeOfBoard; y++)
        {
            for(int x = 0; x < sizeOfBoard; x++)
            {
                GameObject obj = new GameObject();
                    obj.transform.parent = transform;
                    obj.name = "Tile " + x + ", " + y;
                Tile temp = obj.AddComponent<Tile>();
                    temp.InitTile(transform.position, new Vector2(x, y), swap ? TileColor.White : TileColor.Black);
                    temp.SetSprite(sprite);
                    temp.stonesOnTile.Clear();
                    swap = !swap;

                board.Add((x, y), temp);
            }
        }
    }

    public Tile getTile(Vector3 position, out bool success)
    {
        try
        {
            Vector3 offset = (position - transform.position);

            Vector2Int tilePos = new Vector2Int((int)offset.x, (int)offset.y);

            if(tilePos.x < sizeOfBoard && tilePos.y < sizeOfBoard)
            {
                success = true;
                return board[(tilePos.x, tilePos.y)];
            }
            success = false;
            return null;
        }
        catch
        {
            success = false;
            return null;
        }
    }

    public Tile getTileMouse(Vector3 position, out bool success)
    {
        try
        {
            Vector3 offset = (position - transform.position);
            offset += (Vector3)(Vector2.one / 2);

            Vector2Int tilePos = new Vector2Int((int)offset.x, (int)offset.y);

            if (tilePos.x < sizeOfBoard && tilePos.y < sizeOfBoard)
            {
                success = true;
                return board[(tilePos.x, tilePos.y)];
            }
            success = false;
            return null;
        }
        catch
        {
            success = false;
            return null;
        }
    }

    public float getSize() { return sizeOfBoard; }
}
