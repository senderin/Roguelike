﻿using UnityEngine;
using System.Collections;

using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
	public float turnDelay = 0.1f;							//Delay between each Player turn.
	public int healthPoints = 100;                          //Starting value for Player health points.

    public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
	[HideInInspector] public bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.

    private DungeonBoardManager boardScript;
	private List<Enemy> enemies;							//List of all Enemy units, used to issue them move commands.
	private bool enemiesMoving;								//Boolean to check if enemies are moving.

    private DungeonManager dungeonScript;
    private Player playerScript;
    private bool playerInDungeon;

    public bool enemiesFaster = false;
    public bool enemiesSmarter = false;
    public int enemySpawnRatio = 20;

    //Awake is always called before any Start functions
    void Awake()
	{
		//Check if instance already exists
		if (instance == null)
			//if not, set instance to this
			instance = this;
		
		//If instance already exists and it's not this:
		else if (instance != this)
			//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
			Destroy(gameObject);	
		
		//Sets this to not be destroyed when reloading scene
		DontDestroyOnLoad(gameObject);
		
		//Assign enemies to a new List of Enemy objects.
		enemies = new List<Enemy>();

        boardScript = GetComponent<DungeonBoardManager>();
        dungeonScript = GetComponent<DungeonManager>();
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        //Call the InitGame function to initialize the first level 
        InitGame();
	}
	
	//This is called each time a scene is loaded.
	void OnLevelWasLoaded(int index)
	{
		//Call InitGame to initialize our level.
		InitGame();
	}
	
	//Initializes the game for each level.
	void InitGame()
	{
		//Clear any Enemy objects in our List to prepare for next level.
		enemies.Clear();
        boardScript.BoardSetup();
        playerInDungeon = false;
	}
	
	//Update is called every frame.
	void Update()
	{
		//Check that playersTurn or enemiesMoving or doingSetup are not currently true.
		if(playersTurn || enemiesMoving)
			
			//If any of these are true, return and do not start MoveEnemies.
			return;
		
		//Start moving enemies.
		StartCoroutine (MoveEnemies ());
	}
	
	//GameOver is called when the player reaches 0 health points
	public void GameOver()
	{
		//Disable this GameManager.
		enabled = false;
	}
	
	//Coroutine to move enemies in sequence.
	IEnumerator MoveEnemies()
	{
		//While enemiesMoving is true player is unable to move.
		enemiesMoving = true;
		
		//Wait for turnDelay seconds, defaults to .1 (100 ms).
		yield return new WaitForSeconds(turnDelay);
		
		//If there are no enemies spawned (IE in first level):
		if (enemies.Count == 0) 
		{
			//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
			yield return new WaitForSeconds(turnDelay);
		}

        List<Enemy> enemiesToDestroy = new List<Enemy>();
        for(int i = 0; i < enemies.Count; i++)
        {
            if(playerInDungeon)
            {
                if (!(enemies[i].GetSpriteRenderer().isVisible)) {
                    if(i == enemies.Count -1)
                    {
                        yield return new WaitForSeconds(enemies[i].moveTime);
                        continue;
                    }
                }
            }
            else
            {
                if (!(enemies[i].GetSpriteRenderer().isVisible) || !(boardScript.CheckValidTile(enemies[i].transform.position)))
                {
                    enemiesToDestroy.Add(enemies[i]);
                    continue;
                }
            }

            enemies[i].MoveEnemy();
            yield return new WaitForSeconds(enemies[i].moveTime);

        }

        playersTurn = true;

		//Enemies are done moving, set enemiesMoving to false.
		enemiesMoving = false;

        if(enemiesToDestroy.Count > 0)
        {
            foreach(Enemy enemy in enemiesToDestroy)
            {
                enemies.Remove(enemy);
                Destroy(enemy.gameObject);
            }
        }

        enemiesToDestroy.Clear();
    }

    public void UpdateBoard(int horizontal, int vertical)
    {
        boardScript.AddBoard(horizontal, vertical);
    }

    // drives dungoon generation process
    internal void EnterDungeon()
    {
        dungeonScript.StartDungeon();
        boardScript.SetDungeonBoard(dungeonScript.gridPositions, dungeonScript.maxBound, dungeonScript.endPosition);
        playerScript.dungeonTransition = false;
        playerInDungeon = true;

        for(int i = 0; i< enemies.Count; i++)
        {
            Destroy(enemies[i].gameObject);
        }
        enemies.Clear();
    }

    // drives the return to the world board
    internal void ExitDungeon()
    {
        boardScript.SetWorldBoard();
        playerScript.dungeonTransition = false;
        playerInDungeon = false;
        enemies.Clear();

        SoundManager.instance.FormAudio(false);
    }

    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
        SoundManager.instance.FormAudio(true);
    }

    public void RemoveEnemyFromList(Enemy script)
    {
        enemies.Remove(script);
        if (enemies.Count == 0)
            SoundManager.instance.FormAudio(false);
    }

    public bool CheckValidTile(Vector2 pos)
    {
        return boardScript.CheckValidTile(pos);
    }

}