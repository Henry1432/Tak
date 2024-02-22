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

    private Vector3 wallSize = new Vector3(0.2f, 0.8f, 0.8f);
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
        bool success;
        currentTile = GenBoard.instance.getTile(transform.position+Vector3.one/2, out success);

        if(wall)
        {
            transform.localScale = wallSize;
        }
        else
        {
            transform.localScale = Vector3.one * 0.8f;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane;
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            Debug.Log(col.ClosestPoint(worldPos) + "\t" + worldPos);
            if (Vector3.Distance(col.ClosestPoint(worldPos), worldPos) == 0)
            {
                transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
            }
        }
        else if (success)
        {
            transform.position = currentTile.transform.position + Vector3.one / 2;
        }
    }
}
