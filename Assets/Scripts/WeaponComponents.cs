using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponComponents : MonoBehaviour
{
    public Sprite[] modules;

    private Weapon parent;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        parent = GetComponentInParent<Weapon>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = modules[Random.Range(0, modules.Length)];
    }

    private void Update()
    {
        transform.eulerAngles = parent.transform.eulerAngles;
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }
}