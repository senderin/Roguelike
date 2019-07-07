using UnityEngine;
using System;
using System.Collections.Generic; 		//Allows us to use Lists.
using Random = UnityEngine.Random; 		//Tells Random to use the Unity Engine random number generator.

	
public class BoardManager : MonoBehaviour
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

    // holds all tiles
    private Transform boardHolder;

    // list of tiles in game board
    private Dictionary<Vector2, Vector2> gridPositions = new Dictionary<Vector2, Vector2>();

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
        }
    }
}
