using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneShow : MonoBehaviour
{
    public static StoneShow instance;
    public List<SpriteRenderer> renderers;
    public SpriteRenderer backdrop;
    [SerializeField] private Color setBackColor;
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
        Vector3 savePos = transform.position;

        GameObject tempObj = new GameObject();
        tempObj.transform.parent = transform;
        tempObj.name = "Backdrop";
        tempObj.SetActive(true);
        backdrop = tempObj.AddComponent<SpriteRenderer>();
        backdrop.sprite = square;
        backdrop.color = setBackColor;
        tempObj.transform.localScale = size * 1.25f;

        transform.position = new Vector3(0, size.y/2, 6);

        transform.position = savePos;
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
    
    public void addRenderer()
    {
        float squareHeight = size.y / GenBoard.getSize();
        GameObject tempObj = new GameObject();
        tempObj.transform.parent = transform;
        tempObj.transform.localPosition = (Vector3)(Vector2.up * squareHeight * renderers.Count);
        tempObj.name = renderers.Count.ToString();
        tempObj.SetActive(false);
        SpriteRenderer tempSr = tempObj.AddComponent<SpriteRenderer>();
        tempSr.sprite = square;
        tempObj.transform.localScale = new Vector2(size.x, squareHeight);
        renderers.Add(tempSr);

        transform.position = transform.position + (Vector3.down * squareHeight) * 0.5f;
        size.y += squareHeight;
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

    public static Vector2 getSize()
    {
        float squareHeight = instance.size.y / GenBoard.getSize();
        return new Vector2(squareHeight, instance.size.x);
    }

    public void setBackdrop()
    {
        backdrop.transform.localPosition = new Vector3(0, Mathf.Abs(size.y / 2), 7);
        backdrop.transform.localScale = size * 1.25f;
    }
}
