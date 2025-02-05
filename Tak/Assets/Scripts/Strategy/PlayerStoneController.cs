using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStoneController : MonoBehaviour
{
    public TileColor playerColor = TileColor.White;
    public List<Stone> stones = new List<Stone>();
    public Sprite stoneSprite;
    private CircleCollider2D col;

    [SerializeField] private int nextStone = 0;

    private void Start()
    {
        col = GetComponent<CircleCollider2D>();
        stoneReset();
    }

    private void Update()
    {
        if (GameController.instance.placeColor == playerColor)
        {
            if (Input.GetMouseButtonDown(0) && nextStone < GenBoard.instance.maxTiles)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Camera.main.nearClipPlane;
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

                if(Vector3.Distance(col.ClosestPoint(worldPos), worldPos) == 0)
                {
                    stones[nextStone].gameObject.SetActive(true);
                    stones[nextStone].transform.position = worldPos;
                    nextStone = nextStone >= stones.Count ? stones.Count-1 : nextStone + 1;
                }
            }
        }
    }

    public Stone PlaceNextStone(Vector2 pos)
    {
        stones[nextStone].gameObject.SetActive(true);
        stones[nextStone].transform.position = pos;
        int returnNext = nextStone;
        nextStone = nextStone >= stones.Count ? stones.Count - 1 : nextStone + 1;

        return stones[returnNext];
    }

    public void stoneReset()
    {
        GameObject stoneSave = new GameObject();
        stoneSave.name = playerColor.ToString();
        for (int i = 0; i < GenBoard.instance.maxTiles; i++)
        {
            GameObject tempObj = new GameObject("Stone" + (stones.Count + 1));
            tempObj.transform.parent = stoneSave.transform;
            tempObj.AddComponent<BoxCollider2D>();
            Stone tempStone = tempObj.AddComponent<Stone>();
            tempStone.playerStone = this;
            tempStone.wall = false;
            tempStone.cap = false;
            tempStone.placed = false;
            tempStone.onTile = false;
            tempObj.GetComponent<SpriteRenderer>().sprite = stoneSprite;
            tempStone.stoneColor = playerColor;
            tempStone.setFollow(true);
            tempStone.setOffset(Vector3.zero);
            stones.Add(tempStone);

            tempObj.SetActive(false);
        }
    }

    public int GetNext() { return nextStone; }
    public void SetNext(int next) {  nextStone = next; }
}
