using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneShow : MonoBehaviour
{
    public static StoneShow instance;
    public List<SpriteRenderer> renderers;
    [SerializeField] private Vector2 size;
    [SerializeField] private Sprite square;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void initRenderers()
    {
        float squareHeight = size.y/GenBoard.getSize();

        for(int i = 0; i < GenBoard.getSize(); i++)
        {
            GameObject tempObj = new GameObject();
            tempObj.transform.parent = transform;
            tempObj.transform.localPosition = (Vector3)(Vector2.up * squareHeight * i);
            tempObj.name = i.ToString();
            tempObj.SetActive(false);
            SpriteRenderer tempSr = tempObj .AddComponent<SpriteRenderer>();
            tempSr.sprite = square;
            tempObj.transform.localScale = new Vector2 (size.x, squareHeight );
            renderers.Add(tempSr);
        }
    }

    public void showWall(int i)
    {
        if (i < renderers.Count)
        {
            float squareHeight = size.y / GenBoard.getSize();
            renderers[i].gameObject.transform.localScale = new Vector2(squareHeight, size.x);
            renderers[i].gameObject.transform.localPosition = new Vector3(renderers[i].gameObject.transform.localPosition.x, renderers[i].gameObject.transform.localPosition.y, -3);
        }
    }
    public void fixWall(int i)
    {
        if (i < renderers.Count)
        {
            float squareHeight = size.y / GenBoard.getSize();
            renderers[i].gameObject.transform.localScale = new Vector2(size.x, squareHeight);
            renderers[i].gameObject.transform.localPosition = new Vector3(renderers[i].gameObject.transform.localPosition.x, renderers[i].gameObject.transform.localPosition.y, 3);
        }
    }
}
