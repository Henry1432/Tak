using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Selecter : MonoBehaviour
{
    public Sprite ring;
    public Tile selectedTile;
    public Vector2 offset;
    private SpriteRenderer sr;
    private bool highlight = true;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        Vector3 tempPos = Input.mousePosition;
        tempPos.z = Camera.main.nearClipPlane;
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(tempPos);
        if(Input.GetMouseButtonDown(0))
        {
            highlight = false;
        }

        if (Input.GetMouseButtonDown(0) || highlight)
        {
            bool success;
            Tile clickTile = GenBoard.instance.getTileMouse(mousePos, out success);
            
            if(success)
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

        if(selectedTile != null)
        {
            if(selectedTile.stonesOnTile.Count == 0)
            {
                for (int i = 0; i < StoneShow.instance.renderers.Count; i++)
                {
                    StoneShow.instance.fixWall(i);
                    StoneShow.instance.renderers[i].gameObject.SetActive(false);
                }
            }
            for (int i = 0; i < selectedTile.stonesOnTile.Count; i++)
            {
                if(i < StoneShow.instance.renderers.Count)
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
