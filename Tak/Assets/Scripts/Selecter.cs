using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Selecter : MonoBehaviour
{
    public Sprite ring;
    public Tile selectedTile;
    public Vector3 offset;

    public TileColor moveColor = TileColor.White;
    public List<Stone> movingTiles = new List<Stone>();
    private List<Stone> leaveTiles = new List<Stone>();

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
                if(!leaveTiles.Contains(stone) && !movingTiles.Contains(stone))
                {
                    movingTiles.Add(stone);
                }
            }

            if(Input.GetKeyDown(KeyCode.Space) && movingTiles.Count > 0)
            {
                leaveTiles.Add(movingTiles.First());
                movingTiles.Remove(movingTiles.First());
            }

            if(Input.GetKeyDown(KeyCode.DownArrow) && movingTiles.Count > 0)
            {
                //move tile down, think about how they are shown when placed
            }
        }
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
                highlight = false;
                leaveTiles.Clear();
                movingTiles.Clear();
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
    }

    private void ShowStoneSet()
    {
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
        else
        {
            for (int i = 0; i < StoneShow.instance.renderers.Count; i++)
            {
                StoneShow.instance.fixWall(i);
                StoneShow.instance.renderers[i].gameObject.SetActive(false);
            }
        }
    }
}
