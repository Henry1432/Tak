using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public enum Direction
{
    None,
    Up,
    Down,
    Left,
    Right,
}

[RequireComponent(typeof(SpriteRenderer))]
public class Selecter : MonoBehaviour
{
    public Sprite ring;
    public Tile selectedTile;
    public Vector3 offset;

    public TileColor moveColor = TileColor.White;
    public List<Stone> movingStones = new List<Stone>();
    private List<Stone> leaveStones = new List<Stone>();
    private bool moving = false;
    [SerializeField] private Direction moveDir = Direction.None;
    [SerializeField] private int moveDist = 0;

    private SpriteRenderer sr;
    private bool highlight = true;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        SelectMoveCheck();
        ShowStoneSet();

        if(selectedTile != null && !highlight)
        {
            foreach (Stone stone in selectedTile.stonesOnTile) 
            { 
                if(!leaveStones.Contains(stone) && !movingStones.Contains(stone))
                {
                    movingStones.Add(stone);
                }
            }

            if(Input.GetKeyDown(KeyCode.Space) && movingStones.Count > 0)
            {
                leaveStones.Add(movingStones.First());
                movingStones.Remove(movingStones.First());
            }
            if(movingStones.Count > 0)
            {
                if(Input.GetKeyDown(KeyCode.UpArrow))
                {
                    MoveStones(Direction.Up);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    MoveStones(Direction.Down);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    MoveStones(Direction.Right);
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    MoveStones(Direction.Left);
                }
            }
        }
    }

    public void MoveStones(Direction dir)
    {
        if(moveDir ==  dir || moveDir == Direction.None)
        {
            if(GameController.instance.currentTurn == movingStones.Last().stoneColor)
            {
                if(moveDir == Direction.None) 
                {
                    moveDir = dir;
                }
                if(moveDist > 0)
                {
                    leaveStones.Add(movingStones.First());
                    movingStones.Remove(movingStones.First());
                }

                moving = true;
                Tile newTile = selectedTile;
                bool success = false;
                if (dir == Direction.Up)
                {
                    newTile = GenBoard.instance.getTile(selectedTile.transform.position + Vector3.up, out success);
                }
                else if (dir == Direction.Down)
                {
                    newTile = GenBoard.instance.getTile(selectedTile.transform.position + Vector3.down, out success);
                }
                else if (dir == Direction.Right)
                {
                    newTile = GenBoard.instance.getTile(selectedTile.transform.position + Vector3.right, out success);
                }
                else if (dir == Direction.Left)
                {
                    newTile = GenBoard.instance.getTile(selectedTile.transform.position + Vector3.left, out success);
                }

                if(success)
                {
                    bool moved = false;
                    //there is no limmit to stack height, just movable stones height, fix this later 
                    if(movingStones.Count + newTile.stonesOnTile.Count <= GenBoard.getSize())
                    {
                        moved = MoveStonesToTile(newTile);
                    }

                    if(moved)
                    {
                        moveDist++;
                    }
                }
            }

            ShowStoneSet();
        }
    }

    private bool MoveStonesToTile(Tile tile)
    {
        bool moved = false;
        foreach (Stone stone in movingStones)
        {
            if (tile.stonesOnTile.Count > 0)
            {
                if (!stone.cap)
                {
                    if (!tile.stonesOnTile.Last().wall && !tile.stonesOnTile.Last().cap)
                    {
                        stone.currentTile.stonesOnTile.Remove(stone);
                        stone.currentTile = tile;
                        stone.transform.position = stone.currentTile.transform.position;
                        stone.currentTile.stonesOnTile.Add(stone);

                        transform.position = stone.currentTile.transform.position + (Vector3)offset;
                        selectedTile = stone.currentTile;
                        moved = true;
                    }
                }
                else if(!tile.stonesOnTile.Last().cap)
                {
                    tile.stonesOnTile.Last().wall = false;
                    stone.currentTile.stonesOnTile.Remove(stone);
                    stone.currentTile = tile;
                    stone.transform.position = stone.currentTile.transform.position;
                    stone.currentTile.stonesOnTile.Add(stone);

                    transform.position = stone.currentTile.transform.position + (Vector3)offset;
                    selectedTile = stone.currentTile;
                    moved = true;
                }
                else
                    moved = false;
            }
            else
            {
                stone.currentTile.stonesOnTile.Remove(stone);
                stone.currentTile = tile;
                stone.transform.position = stone.currentTile.transform.position;
                stone.currentTile.stonesOnTile.Add(stone);

                transform.position = stone.currentTile.transform.position + (Vector3)offset;
                selectedTile = stone.currentTile;
                moved = true;
            }
        }
        movingStones.Clear();
        leaveStones.Clear();
        return moved;
    }

    private void SelectMoveCheck()
    {
        Vector3 tempPos = Input.mousePosition;
        tempPos.z = Camera.main.nearClipPlane;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(tempPos);
        bool click = Input.GetMouseButtonDown(0);
        if (click || highlight)
        {
            if (click)
            {
                if (moving)
                {
                    GameController.swapTurn();
                    moving = false;
                    moveDir = Direction.None; 
                    moveDist = 0;
                }
                highlight = false;
                leaveStones.Clear();
                movingStones.Clear();
            }

            bool success;
            Tile clickTile = GenBoard.instance.getTileMouse(mousePos, out success);

            if (success)
            {
                transform.position = clickTile.transform.position + (Vector3)offset;
                sr.sprite = ring;
            }
            else
            {
                sr.sprite = null;
                highlight = true;
            }

            selectedTile = clickTile;
        }

        if (moving && selectedTile == null)
        {
            GameController.swapTurn();
            moving = false;
            moveDir = Direction.None;
            moveDist = 0;
        }
    }

    private void ShowStoneSet()
    {
        for (int i = 0; i < StoneShow.instance.renderers.Count; i++)
        {
            StoneShow.instance.fixWall(i);
            StoneShow.instance.renderers[i].gameObject.SetActive(false);
        }
        if (selectedTile != null)
        {
            if (selectedTile.stonesOnTile.Count == 0)
            {
                for (int i = 0; i < StoneShow.instance.renderers.Count; i++)
                {
                    StoneShow.instance.fixWall(i);
                    StoneShow.instance.renderers[i].gameObject.SetActive(false);
                }
            }
            for (int i = 0; i < selectedTile.stonesOnTile.Count; i++)
            {
                if (i < StoneShow.instance.renderers.Count)
                {
                    StoneShow.instance.renderers[i].color = selectedTile.stonesOnTile[i].stoneColor == TileColor.White ? Color.white : Color.black;
                    if (selectedTile.stonesOnTile[i].wall)
                    {
                        StoneShow.instance.showWall(i);
                    }
                    else
                    {
                        StoneShow.instance.fixWall(i);
                    }

                    StoneShow.instance.renderers[i].gameObject.SetActive(true);
                }
            }
        }
    }
}
