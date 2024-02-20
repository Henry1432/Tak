using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GenBoard : MonoBehaviour
{
    public static GenBoard instance;
    public Dictionary<(float,float), Tile> board = new Dictionary<(float, float), Tile>();

    [SerializeField] private float sizeOfBoard;
    [SerializeField] private Sprite sprite;

    private void Start()
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
    }

    private void Update()
    {
        if(board.Count == 0)
        {
            Generate();
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
                    swap = !swap;

                board.Add((x, y), temp);
            }
        }
    }

    public Tile getTile(Vector3 position, out bool success)
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
}
