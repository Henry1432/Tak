using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStoneController : MonoBehaviour
{
    public TileColor playerColor = TileColor.White;
    public List<Stone> stones = new List<Stone>();
    public Sprite stoneSprite;
    private CircleCollider2D col;

    [SerializeField] private int nextTile = 0;

    private void Start()
    {
        col = GetComponent<CircleCollider2D>();
        stoneReset();
    }

    private void Update()
    {
        if (GameController.instance.placeColor == playerColor)
        {
            if (Input.GetMouseButtonDown(0) && nextTile < GenBoard.instance.maxTiles)
            {
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Camera.main.nearClipPlane;
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

                if(Vector3.Distance(col.ClosestPoint(worldPos), worldPos) == 0)
                {
                    stones[nextTile].gameObject.SetActive(true);
                    stones[nextTile].transform.position = worldPos;
                    nextTile = nextTile >= stones.Count ? stones.Count-1 : nextTile + 1;
                }
            }
        }
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

    public int GetNext() { return nextTile; }
    public void SetNext(int next) {  nextTile = next; }
}
