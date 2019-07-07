using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public Sprite openSprite;
    public GameObject randomItem;
    public GameObject weapon;

    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Open()
    {
        spriteRenderer.sprite = openSprite;
        GameObject toInstantiate = null;

        if (Random.Range(0, 2) == 1)
        {
            randomItem.GetComponent<Item>().RandomItemInit();
            toInstantiate = randomItem.gameObject;
        }
        else
        {
            toInstantiate = weapon.gameObject;
        }

        GameObject instance = Instantiate(toInstantiate, new Vector3(transform.position.x, transform.position.y, 0f), Quaternion.identity) as GameObject;
        instance.transform.SetParent(transform.parent);

        gameObject.layer = 10;
        spriteRenderer.sortingLayerName = "Items";
    }
}
