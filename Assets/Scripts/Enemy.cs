using UnityEngine;
using System.Collections;
using System;

public class Enemy : MovingObject
{
    public int playerDamage;

    private Animator animator;
    private Transform target; // player
    private bool skipMove; // to slow enemy down
    private SpriteRenderer spriteRenderer;

    protected override void Start()
    {
        GameManager.instance.AddEnemyToList(this);
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();

        base.Start();
    }

    protected override bool AttemptMove <T> (int xDir, int yDir)
	{
		if(skipMove && !GameManager.instance.enemiesFaster)
        {
            skipMove = false;
            return false;
        }

        base.AttemptMove<T>(xDir, yDir);

        skipMove = true;
        return true;
    }

	protected override void OnCantMove <T> (T component)
	{
        Player hitPlayer = component as Player;

        hitPlayer.LoseHealth(playerDamage);

        animator.SetTrigger("enemyAttack");
	}

    public void MoveEnemy()
    {
        int xDir = 0;
        int yDir = 0;

        if(GameManager.instance.enemiesSmarter)
        {
            int xHeading = (int)target.position.x - (int)transform.position.x;
            int yHeading = (int)target.position.y - (int)transform.position.y;
            bool moveOnX = false;

            if (Mathf.Abs(xHeading) >= Mathf.Abs(yHeading)) {
                moveOnX = true;
            }

            for(int attempt = 0; attempt < 2; attempt++)
            {
                if(moveOnX == true && xHeading < 0)
                {
                    xDir = -1;
                    yDir = 0;
                }

                else if (moveOnX == true && xHeading > 0)
                {
                    xDir = 1;
                    yDir = 0;
                }

                else if (moveOnX == false && xHeading < 0)
                {
                    xDir = 0;
                    yDir = -1;
                }

                else if (moveOnX == false && xHeading > 0)
                {
                    xDir = 0;
                    yDir = 1;
                }

                Vector2 start = transform.position;
                Vector2 end = start + new Vector2(xDir, yDir);
                GetComponent<BoxCollider2D>().enabled = false;
                RaycastHit2D hit = Physics2D.Linecast(start, end, base.blockingLayer);
                GetComponent<BoxCollider2D>().enabled = true;

                if(hit.transform != null)
                {
                    if (hit.transform.gameObject.tag == "Wall" || hit.transform.tag == "Chest")
                    {
                        if (moveOnX == true)
                            moveOnX = false;
                        else
                            moveOnX = true;
                    }
                    else
                        break;
                }
            }
        }

        else
        {
            if (Mathf.Abs(target.position.x - transform.position.y) < float.Epsilon)
            {
                yDir = target.position.y > transform.position.y ? 1 : -1;
            }
            else
            {
                xDir = target.position.x > transform.position.x ? 1 : -1;
            }
        }

        AttemptMove<Player>(xDir, yDir);
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    internal void DamageEnemy(int wallDamage)
    {
        // todo
        //Disable the gameObject.
        gameObject.SetActive(false);
    }
}
