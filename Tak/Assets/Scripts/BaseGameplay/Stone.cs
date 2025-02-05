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
    public bool cap = false;
    public Sprite capSprite;
    public Tile currentTile = null;
    public PlayerStoneController playerStone;
    public bool onTile = false;
    public bool placed = false; 
    public bool follow;

    private Vector3 setSize = new Vector3(0.8f, 0.8f, 0.8f);
    private Vector3 wallSize = new Vector3(0.2f, 0.8f, 0.8f);
    
    private Vector3 mouseOffset = Vector3.negativeInfinity;
    private Vector2 mousePos = Vector3.negativeInfinity;
    private SpriteRenderer sr;
    private BoxCollider2D col;

    private bool tempStone = false;

    private void Start()
    {
        if (!tempStone)
        {
            transform.localScale = Vector3.one * 0.8f;

            if (sr == null)
                sr = GetComponent<SpriteRenderer>();

            if(cap)
            {
                sr.color = playerStone.GetComponent<SpriteRenderer>().color;
            }
            else if (stoneColor == TileColor.White)
            {
                sr.color = Color.white;
            }
            else if (stoneColor == TileColor.Black)
            {
                sr.color = Color.black;
            }

            sr.sprite = cap ? capSprite : sr.sprite;

            col = GetComponent<BoxCollider2D>();
        }
    }

    private void Update()
    {
        if(!tempStone)
        {

            if (wall && !cap)
            {
                transform.localScale = wallSize;
            }
            else
            {
                transform.localScale = setSize;
            }
            if (!placed && (cap ? GameController.canWall() : true))
            {
                bool success;
                currentTile = GenBoard.instance.getTile(transform.position+Vector3.one/2, out success);

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
                    else if (Input.GetMouseButtonDown(1) && GameController.canWall())
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
                        GameController.placeStone(); 
                        if (GenBoard.instance.board[(currentTile.boardPosition.x, currentTile.boardPosition.y)].stonesOnTile.Count == 0 || placed)
                        {
                            transform.position = new Vector3(currentTile.transform.position.x, currentTile.transform.position.y, transform.position.z);
                            GenBoard.instance.board[(currentTile.boardPosition.x, currentTile.boardPosition.y)].stonesOnTile.Add(this);
                            onTile = true;
                            placed = true;
                        }
                        else
                        {
                            if (!cap)
                            {
                                Debug.LogWarning("attept to place stone on tile with stones");
                                gameObject.SetActive(false);
                                currentTile = null;
                                transform.position = playerStone.transform.position;
                                follow = true;
                                playerStone.SetNext(playerStone.GetNext()-1);
                            }
                            else
                            {
                                Debug.LogWarning("attept to place stone on tile with stones");
                                gameObject.SetActive(true);
                                currentTile = null;
                                transform.position = playerStone.transform.position + Vector3.up*5;
                                follow = true;
                                playerStone.SetNext(playerStone.GetNext() - 1);
                            }
                        }
                    }
                }
            }

            if(placed)
            {
                transform.position = new Vector3(currentTile.transform.position.x, currentTile.transform.position.y, transform.position.z);
            }
        }
    }

    public static Stone getTempStone(TileColor color, bool wall, bool cap, Tile current)
    {
        GameObject stoneObj = new GameObject();
        stoneObj.name = "tempStone";
        Stone stone = stoneObj.AddComponent<Stone>();
        stone.col = stoneObj.AddComponent<BoxCollider2D>();
        stone.sr = stoneObj.GetComponent<SpriteRenderer>();
        stone.col.enabled = false;
        stone.sr.enabled = false;
        stone.stoneColor = color;
        stone.wall = wall;
        stone.cap = cap;
        stone.currentTile = current;
        stone.onTile = true;
        stone.placed = true;
        stone.follow = false;
        stone.tempStone = true;

        return stone;
}

    public bool isTemp() { return tempStone; }
    public void DeleteTemp()
    {
        if(tempStone)
        {
            Destroy(gameObject);
        }
    }

    public void setFollow(bool follow) { this.follow = follow; }
    public void setOffset(Vector3 offset) {  this.mouseOffset = offset; }
}
