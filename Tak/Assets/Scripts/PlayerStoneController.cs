using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStoneController : MonoBehaviour
{
    public TileColor playerColor = TileColor.White;
    public List<Stone> stones = new List<Stone>();
    public Sprite stoneSprite;
    private CircleCollider2D col;

    private void Start()
    {
        col = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && stones.Count < GenBoard.instance.maxTiles)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane;
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            if(Vector3.Distance(col.ClosestPoint(worldPos), worldPos) == 0)
            {
                GameObject tempObj = new GameObject("Stone" + (stones.Count + 1));
                tempObj.AddComponent<BoxCollider2D>();
                Stone tempStone = tempObj.AddComponent<Stone>();
                tempStone.playerStone = this;
                tempObj.GetComponent<SpriteRenderer>().sprite = stoneSprite;
                tempStone.stoneColor = playerColor;
                tempStone.setFollow(true);
                tempStone.setOffset(Vector3.zero);
                stones.Add(tempStone);
            }
        }
    }
}
