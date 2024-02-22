using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStoneController : MonoBehaviour
{
    public TileColor playerColor = TileColor.White;
    private CircleCollider2D col;

    private void Start()
    {
        col = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            if(Vector3.Distance(col.ClosestPoint(worldPos), worldPos) == 0)
            {

            }
        }
    }
}
