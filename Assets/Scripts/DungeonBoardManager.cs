using UnityEngine;
using System;
using System.Collections.Generic; 		//Allows us to use Lists.
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

	
public class DungeonBoardManager : MonoBehaviour
{
	// Using Serializable allows us to embed a class with sub properties in the inspector.
	[Serializable]
	public class Count
	{
		public int minimum; 			//Minimum value for our Count class.
		public int maximum; 			//Maximum value for our Count class.
		
		
		//Assignment constructor.
		public Count (int min, int max)
		{
			minimum = min;
			maximum = max;
		}
	}

    // game board grid
    public int columns = 5;
    public int rows = 5;

    // holds all floor prefabs
    public GameObject[] floorTiles;
    // holds all wall prefabs
    public GameObject[] wallTiles;
    // impassable wall tiles
    public GameObject[] outerWallTiles;
    public GameObject chestTile;

    public GameObject exit; // dungeon entrance and exit marker
    public GameObject enemy;

    // holds all tiles
    private Transform boardHolder;
    // list of tiles in game board
    private Dictionary<Vector2, Vector2> gridPositions = new Dictionary<Vector2, Vector2>();

    private Transform dungeonBoardHolder;
    private Dictionary<Vector2, Vector2> dungeonGridPositions = new Dictionary<Vector2, Vector2>();

    /// <summary>
    /// Create initial game board
    /// </summary>
    public void BoardSetup()
    {
        boardHolder = new GameObject("Board").transform;

        // initial 5x5 grid
        for(int  x = 0; x < columns; x++)
        {
            for(int y = 0; y < rows; y++)
            {
                gridPositions.Add(new Vector2(x, y), new Vector2(x, y));

                // randomly choosen tile
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                // instantiation of choosen tile
                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                // added to game board
                instance.transform.SetParent(boardHolder);

            }
        }
    }

    public void AddBoard(int horizontal, int vertical)
    {
        // player moves to right, vertical 0
        if(horizontal == 1)
        {
            // check if tiles exist
            int x = (int)Player.position.x;
            int sightX = x + 2;
            for( x += 1; x <= sightX; x++)
            {
                int y = (int)Player.position.y;
                int sightY = y + 1;

                for (y -= 1; y <= sightY; y++)
                {
                    AddTiles(new Vector2(x, y));
                }
            }
        }

        else if (horizontal == -1)
        {
            // check if tiles exist
            int x = (int)Player.position.x;
            int sightX = x - 2;
            for (x -= 1; x >= sightX; x--)
            {
                int y = (int)Player.position.y;
                int sightY = y + 1;

                for (y -= 1; y <= sightY; y++)
                {
                    AddTiles(new Vector2(x, y));
                }
            }
        }

        else if (vertical == 1)
        {
            // check if tiles exist
            int y = (int)Player.position.y;
            int sightY = y + 2;
            for (y += 1; y <= sightY; y++)
            {
                int x = (int)Player.position.x;
                int sightX = x + 1;

                for (x -= 1; x <= sightX; x++)
                {
                    AddTiles(new Vector2(x, y));
                }
            }
        }

        else if (vertical == -1)
        {
            // check if tiles exist
            int y = (int)Player.position.y;
            int sightY = y - 2;
            for (y -= 1; y >= sightY; y--)
            {
                int x = (int)Player.position.x;
                int sightX = x + 1;

                for (x -= 1; x <= sightX; x++)
                {
                    AddTiles(new Vector2(x, y));
                }
            }
        }
    }

    private void AddTiles(Vector2 tileToAdd)
    {
        if(!gridPositions.ContainsKey(tileToAdd))
        {
            gridPositions.Add(tileToAdd, tileToAdd);
            GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
            GameObject instance = Instantiate(toInstantiate, new Vector3(tileToAdd.x, tileToAdd.y, 0f), Quaternion.identity) as GameObject;
            instance.transform.SetParent(boardHolder);

            // choose at random a wall tile to lay
            if(Random.Range(0,3) == 1)
            {
                toInstantiate = wallTiles[Random.Range(0, wallTiles.Length)];
                instance = Instantiate(toInstantiate, new Vector3(tileToAdd.x, tileToAdd.y, 0f), Quaternion.identity) as GameObject;
                instance.transform.SetParent(boardHolder);
            }

            // exit tile will be spawned 1 in 100 tiles
            // acts as entrance to dungeon
            else if(Random.Range(0, 100) == 1)
            {
                toInstantiate = exit;
                instance = Instantiate(toInstantiate, new Vector3(tileToAdd.x, tileToAdd.y, 0f), Quaternion.identity) as GameObject;
                instance.transform.SetParent(boardHolder);
            }

            else if(Random.Range(0, GameManager.instance.enemySpawnRatio) == 1)
            {
                toInstantiate = enemy;
                instance = Instantiate(toInstantiate, new Vector3(tileToAdd.x, tileToAdd.y, 0f), Quaternion.identity) as GameObject;
                instance.transform.SetParent(boardHolder);
            }
        }
    }

    // takes dungeon data and apply the on screen graphics
    public void SetDungeonBoard(Dictionary<Vector2, TileType> dungeonTiles, int bound, Vector2 endPosition)
    {
        // instead of changing scene, sets world board as inactive
        boardHolder.gameObject.SetActive(false);
        dungeonBoardHolder = new GameObject("Dungeon").transform;
        GameObject toInstantiate, instance;

        foreach(KeyValuePair<Vector2, TileType> tile in dungeonTiles)
        {
            toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
            instance = Instantiate(toInstantiate, new Vector3(tile.Key.x, tile.Key.y, 0f), Quaternion.identity) as GameObject;
            instance.transform.SetParent(dungeonBoardHolder);

            if(tile.Value == TileType.CHEST)
            {
                toInstantiate = chestTile;
                instance = Instantiate(toInstantiate, new Vector3(tile.Key.x, tile.Key.y, 0f), Quaternion.identity) as GameObject;
                instance.transform.SetParent(dungeonBoardHolder);
            }

            if (tile.Value == TileType.ENEMY)
            {
                toInstantiate = enemy;
                instance = Instantiate(toInstantiate, new Vector3(tile.Key.x, tile.Key.y, 0f), Quaternion.identity) as GameObject;
                instance.transform.SetParent(dungeonBoardHolder);
            }
        }

        // places a layer of outer wall tiles to enclose dungeon
        for(int x = -1; x < bound + 1; x++)
        {
            for(int y = -1; y < bound + 1; y++)
            {
                if(!dungeonTiles.ContainsKey(new Vector2(x, y)))
                {
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                    instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                    instance.transform.SetParent(dungeonBoardHolder);
                }
            }
        }

        toInstantiate = exit;
        instance = Instantiate(toInstantiate, new Vector3(endPosition.x, endPosition.y, 0f), Quaternion.identity) as GameObject;
        instance.transform.SetParent(dungeonBoardHolder);
    }

    public void SetWorldBoard()
    {
        Destroy(dungeonBoardHolder.gameObject);
        boardHolder.gameObject.SetActive(true);
    }

    public bool CheckValidTile(Vector2 position)
    {
        if(gridPositions.ContainsKey(position))
        {
            return true;
        }

        return false;
    }
}
