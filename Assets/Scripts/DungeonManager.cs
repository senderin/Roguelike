using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum TileType
{
    ESSENTIAL,
    RANDOM,
    EMPTY,
    CHEST,
    ENEMY
}

[Serializable]
public class PathTile
{
    public TileType type;
    public Vector2 position;
    // stores the tiles next to the current PathTile
    public List<Vector2> adjacentPathTiles;

    public PathTile(TileType type, Vector2 position, int min, int max, Dictionary<Vector2, TileType> currentTiles)
    {
        this.type = type;
        this.position = position;

        adjacentPathTiles = GetAdjacentPath(min, max, currentTiles);
    }

    // calculates which tiles are adjacent to this tile
    public List<Vector2> GetAdjacentPath(int minBound, int maxBound, Dictionary<Vector2, TileType> currentTiles)
    {

        List<Vector2> pathTiles = new List<Vector2>();

        // checks top tile
        if(position.y + 1 < maxBound && !currentTiles.ContainsKey(new Vector2(position.x, position.y + 1)))
        {
            pathTiles.Add(new Vector2(position.x, position.y + 1));
        }

        // checks right tile
        if (position.x + 1 < maxBound && !currentTiles.ContainsKey(new Vector2(position.x + 1, position.y)))
        {
            pathTiles.Add(new Vector2(position.x + 1, position.y));
        }

        // checks bottom tile
        if (position.y - 1 > minBound && !currentTiles.ContainsKey(new Vector2(position.x, position.y - 1)))
        {
            pathTiles.Add(new Vector2(position.x, position.y - 1));
        }

        // checks left tile 
        // essential PathTiles cannot move to left
        if (position.x - 1 >= minBound && !currentTiles.ContainsKey(new Vector2(position.x - 1, position.y)) && type != TileType.ESSENTIAL)
        {
            pathTiles.Add(new Vector2(position.x - 1, position.y));
        }

        return pathTiles;
    }
}

public class Queue
{
    public List<PathTile> pathTileList;

    public bool hasNext
    {
        get
        {
            if (pathTileList.Count > 0)
                return true;
            return false;
        }

        private set { }
    }

    public PathTile nextTile {
        get
        {
            PathTile tile = pathTileList[0];
            Remove();
            return tile;
        }

        private set { }
    }

    public Queue()
    {
        pathTileList = new List<PathTile>();
    }

    public void Add(PathTile newTile)
    {
        pathTileList.Add(newTile);
    }

    public void Remove()
    {
        pathTileList.RemoveAt(0);
    }
}

public class DungeonManager : MonoBehaviour
{
    // stores structure of generated structure
    public Dictionary<Vector2, TileType> gridPositions = new Dictionary<Vector2, TileType>();

    // dimensions of board
    public int minBound = 0, maxBound;

    public static Vector2 startPosition;
    public Vector2 endPosition;

    public void StartDungeon()
    {
        /* in order to recreate the dungeon
         * for situations such as having a player return to a dungeon to complete a task    
         */
        // Random.InitState(1);
        gridPositions.Clear();
        maxBound = Random.Range(25, 50);

        BuildEssentialPath();
        BuildRandomPath();
    }

    private void BuildEssentialPath()
    {
        int randomYCoordinate = Random.Range(0, maxBound + 1);
        startPosition = new Vector2(0, randomYCoordinate);
        PathTile essentialPath = new PathTile(TileType.ESSENTIAL, startPosition, minBound, maxBound, gridPositions);

        // everytime essential path moves to right, increased by 1
        int boundTracker = 0;

        // dungeon over 200x200 is loaded more slowly
        while (boundTracker < maxBound)
        {
            gridPositions.Add(essentialPath.position, TileType.EMPTY);

            int adjacentTileCount = essentialPath.adjacentPathTiles.Count;

            // randomly, choose to follow from adjecent tiles
            int randomIndex = Random.Range(0, adjacentTileCount);

            Vector2 nextEssentialPathPosition;
            if (adjacentTileCount > 0)
            {
                nextEssentialPathPosition = essentialPath.adjacentPathTiles[randomIndex];
            }
            else
                break;

            PathTile nextEssentialPath = new PathTile(TileType.ESSENTIAL, nextEssentialPathPosition, minBound, maxBound, gridPositions);

            // checks whether essential path has moved right, then update BoundTracker 
            if(nextEssentialPath.position.x > essentialPath.position.x || (nextEssentialPath.position.x == maxBound - 1 && Random.Range(0, 2) == 1))
            {
                boundTracker++;
            }

            essentialPath = nextEssentialPath;
        }

        if (!gridPositions.ContainsKey(essentialPath.position))
            gridPositions.Add(essentialPath.position, TileType.EMPTY);

        endPosition = new Vector2(essentialPath.position.x, essentialPath.position.y);

    }


    private void BuildRandomPath()
    {

        Queue pathQueue = new Queue();

        // copies essential path to the pathQueue
        foreach(KeyValuePair<Vector2, TileType> tile in gridPositions)
        {
            Vector2 tilePosition = new Vector2(tile.Key.x, tile.Key.y);
            pathQueue.Add(new PathTile(TileType.RANDOM, tilePosition, minBound, maxBound, gridPositions));
        }

        // starts processing
        while(pathQueue.hasNext)
        {
            PathTile tile = pathQueue.nextTile;

            int adjacentTileCount = tile.adjacentPathTiles.Count;

            if (adjacentTileCount == 0)
                return;

            // 1 in 5 chance to create a chamber
            if(Random.Range(0, 5) == 1)
            {
                BuildRandomChamber(tile);
            }

            // 1 in 3 chance whether a random path is created
            // or if tile is RANDOM and more than one direction to move, then creates random path 
            else if(Random.Range(0, 3) == 1 || (tile.type == TileType.RANDOM && adjacentTileCount > 1))
            {
                int randomIndex = Random.Range(0, adjacentTileCount);
                Vector2 nextRandomPathPosition = tile.adjacentPathTiles[randomIndex];

                // if it isn't already part of dungeon
                if(!gridPositions.ContainsKey(nextRandomPathPosition))
                {
                    if(Random.Range(0, 20) == 1)
                    {
                        gridPositions.Add(nextRandomPathPosition, TileType.ENEMY);
                    }
                    else
                    {
                        gridPositions.Add(nextRandomPathPosition, TileType.EMPTY);
                    }

                    PathTile newRandomPath = new PathTile(TileType.RANDOM, nextRandomPathPosition, minBound, maxBound, gridPositions);
                    pathQueue.Add(newRandomPath);
                }
            }
        }
    }

    // adds 3x3 chamber to the end of the random path
    private void BuildRandomChamber(PathTile tile)
    {
        int chamberSize = 3; // todo can be ramdomized
        int adjacentTileCount = tile.adjacentPathTiles.Count;
        int randomIndex = Random.Range(0, adjacentTileCount);
        Vector2 chamberOrigin = tile.adjacentPathTiles[randomIndex];

        for(int x = (int)chamberOrigin.x; x < chamberOrigin.x + chamberSize; x++)
        {
            for (int y = (int)chamberOrigin.y; y < chamberOrigin.y + chamberSize; y++)
            {
                Vector2 chamberTilePosition = new Vector2(x, y);
                if(!gridPositions.ContainsKey(chamberTilePosition) &&
                (chamberTilePosition.x < maxBound && chamberTilePosition.x > 0) &&
                (chamberTilePosition.y < maxBound && chamberTilePosition.y > 0))
                {
                    // gridPositions.Add(chamberTilePosition, TileType.EMPTY);
                    if(Random.Range(0, 25) == 1)
                    {
                        gridPositions.Add(chamberTilePosition, TileType.CHEST);
                    }
                    else
                    {
                        gridPositions.Add(chamberTilePosition, TileType.EMPTY);
                    }
                }
            }
        }

    }
}


