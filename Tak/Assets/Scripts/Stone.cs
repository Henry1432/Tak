using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(SpriteRenderer))]
public class Stone : MonoBehaviour
{
    public TileColor stoneColor = TileColor.None;
    public bool wall = false;
    public Tile currentTile = null;
    public PlayerStoneController playerStone;

    private bool onTile = false;
    private bool placed = false;
    private Vector3 setSize = new Vector3(0.8f, 0.8f, 0.8f);
    private Vector3 wallSize = new Vector3(0.2f, 0.8f, 0.8f);
    private bool follow;
    private Vector3 mouseOffset = Vector3.negativeInfinity;
    private Vector2 mousePos = Vector3.negativeInfinity;
    private SpriteRenderer sr;
    private BoxCollider2D col;

    private void Start()
    {
        transform.localScale = Vector3.one * 0.8f;

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();


        if (stoneColor == TileColor.White)
        {
            sr.color = Color.white;
        }
        else if (stoneColor == TileColor.Black)
        {
            sr.color = Color.black;
        }

        col = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (!placed)
        {
            bool success;
            currentTile = GenBoard.instance.getTile(transform.position+Vector3.one/2, out success);

            if(wall)
            {
                transform.localScale = wallSize;
            }
            else
            {
                transform.localScale = setSize;
            }

            Vector3 tempPos = Input.mousePosition;
            tempPos.z = Camera.main.nearClipPlane;
            mousePos = Camera.main.ScreenToWorldPoint(tempPos);

            if (Vector3.Distance(col.ClosestPoint(mousePos), mousePos) == 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    follow = true;
                    mouseOffset = (Vector2)transform.position - mousePos;
                    mouseOffset.z = transform.position.z;

                    if (onTile)
                    {
                        currentTile.stonesOnTile.Remove(this);
                        onTile = false;
                    }
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    follow = false;
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    wall = !wall;
                }
            }


            if (follow)
            {
                transform.position = (Vector3)mousePos + mouseOffset;
            }
            else if (success)
            {
                if(!onTile) 
                { 
                    if (GenBoard.instance.board[(currentTile.boardPosition.x, currentTile.boardPosition.y)].stonesOnTile.Count == 0 || placed)
                    {
                        transform.position = new Vector3(currentTile.transform.position.x, currentTile.transform.position.y, transform.position.z);
                        GenBoard.instance.board[(currentTile.boardPosition.x, currentTile.boardPosition.y)].stonesOnTile.Add(this);
                        onTile = true;
                        placed = true;
                    }
                    else
                    {
                        Debug.LogWarning("attept to place stone on tile with stones");
                        gameObject.SetActive(false);
                        currentTile = null;
                        transform.position = playerStone.transform.position;
                        follow = true;
                        playerStone.SetNext(playerStone.GetNext()-1);
                    }
                }
            }
        }
    }

    public void setFollow(bool follow) { this.follow = follow; }
    public void setOffset(Vector3 offset) {  this.mouseOffset = offset; }
}