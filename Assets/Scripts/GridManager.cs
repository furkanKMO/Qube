using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance; // Singleton instance for easy access to the grid system.

    private Dictionary<Vector2Int, TileData> gridData = new Dictionary<Vector2Int, TileData>(); // Stores tile and object data.
    public float tileSize = 1.0f; // The size of each tile in world units.

    public GameObject groundPrefab; // (Unused in manual mode) Prefab for ground tiles.
    public GameObject tilesParent, objParent;
    public GameObject[] objectPrefabs; // Prefabs for pushable objects.
    //  Dictionary to track tiles where objects have sunk
    private HashSet<Vector2Int> sunkenObjectsTiles = new HashSet<Vector2Int>();
    void Awake()
    {
        Instance = this;
        LoadGridFromScene(); // Instead of generating a grid, scan the scene for manually placed tiles & objects.
    }

    /// <summary>
    /// Loads the grid data from the scene instead of generating it.
    /// This method finds all `Tile` and `PushableObject` components and registers them in `gridData`.
    /// </summary>
    void LoadGridFromScene()
    {
        gridData.Clear();

        // Register all manually placed ground tiles
        foreach (Tile tile in tilesParent.GetComponentsInChildren<Tile>())
        {
            Vector2Int pos = tile.gridPosition;
            if (!gridData.ContainsKey(pos))
            {
                gridData[pos] = new TileData { Ground = tile.gameObject };
            }
        }

        //  Register all manually placed pushable objects
        foreach (PushableObject obj in objParent.GetComponentsInChildren<PushableObject>())
        {
            Vector2Int pos = obj.gridPos;
            if (gridData.ContainsKey(pos) && !gridData[pos].HasObject)
            {
                gridData[pos].Object = obj.gameObject;
            }
        }
    }
    
    /// <summary>
    /// Marks a specific water tile as having a sunken object.
    /// </summary>
    public void MarkTileWithSunkenObject(Vector2Int pos, bool hasSunkenObject)
    {
        if (hasSunkenObject)
        {
            sunkenObjectsTiles.Add(pos); // Track only this specific tile
        }
        else
        {
            sunkenObjectsTiles.Remove(pos); //  Remove if object is removed
        }
    }
    public bool CheckTileWithSunkenObject(Vector2Int pos)
    {
        return sunkenObjectsTiles.Contains(pos); //  Returns true if the tile contains a sunken object
    }
    /// <summary>
    /// Checks if a grid position is moveable.
    /// </summary>
    public bool IsMoveable(Vector2Int pos, bool isObject = false)
    {
        if (!gridData.ContainsKey(pos)) return false;

        if (gridData[pos].Ground.TryGetComponent(out Tile tile))
        {
            if (tile.IsBlock()) return false; // Blocks movement (e.g., walls)

            if (tile.tileType == TileType.Water)
            {
                if (isObject)
                {
                    //  Objects CAN move into water (but become stuck)
                    return true;
                }
                else
                {
                    //  Players CAN walk onto the tile IF an object has sunk there
                    return sunkenObjectsTiles.Contains(pos);
                }
            }
        }
        //  Allow objects to move onto sunken objects (bridges)
        if (isObject && sunkenObjectsTiles.Contains(pos))
        {
            return true;
        }
        return gridData[pos].HasGround && !gridData[pos].HasObject;
    }
    /// <summary>
    /// Moves an object from one grid position to another.
    /// </summary>
    public void MoveObject(Vector2Int oldPos, Vector2Int newPos)
    {
        if (!gridData.ContainsKey(oldPos) || !gridData.ContainsKey(newPos)) return; // Ensure valid movement.

        GameObject movingObj = gridData[oldPos].Object;
        if (movingObj != null)
        {
            gridData[newPos].Object = movingObj; // Move object to new position.
            gridData[oldPos].Object = null; // Clear old position.

            if (movingObj.TryGetComponent(out PushableObject pushable))
            {
                pushable.gridPos = newPos; // Update grid position in PushableObject.
            }
        }
    }

    /// <summary>
    /// Checks if a tile is Water.
    /// </summary>
    public bool IsWater(Vector2Int pos)
    {
        if (gridData.ContainsKey(pos) && gridData[pos].Ground.TryGetComponent(out Tile tile))
        {
            return tile.tileType == TileType.Water;
        }
        return false;
    }
    /// <summary>
    /// Checks if a tile is Ýce.
    /// </summary>
    public bool IsIce(Vector2Int pos)
    {
        if (gridData.ContainsKey(pos) && gridData[pos].Ground.TryGetComponent(out Tile tile))
        {
            return tile.tileType == TileType.Ice;
        }
        return false;
    }
    public bool IsSinkingBlock(Vector2Int pos)
    {
        if (gridData.ContainsKey(pos) && gridData[pos].Ground.TryGetComponent(out Tile tile))
        {
            return tile.tileType == TileType.SinkingBlock;
        }
        return false;
    }

    //  Revert objects & players if they're on a sinking tile
    public void RevertObjectsOnTile(Vector2Int pos)
    {
        GameObject obj = GetObjectAt(pos);
        if (obj != null && obj.TryGetComponent(out PushableObject pushable))
        {
            pushable.RevertToLastPosition(); // Move object back
        }

        if (IsPlayerAt(pos))
        {
            PlayerController.Instance.RevertToLastPosition(); // Move player back
        }
    }

    //  Check if player is at this position
    public bool IsPlayerAt(Vector2Int pos)
    {
        return PlayerController.Instance.gridPos == pos;
    }


    /// <summary>
    /// Converts a world position to a grid position.
    /// </summary>
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / tileSize),
            Mathf.RoundToInt(worldPos.z / tileSize)
        );
    }

    /// <summary>
    /// Converts a grid position to a world position.
    /// </summary>
    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * tileSize, 0.5f, gridPos.y * tileSize);
    }

    /// <summary>
    /// Retrieves the object at a given grid position.
    /// </summary>
    public GameObject GetObjectAt(Vector2Int gridPos)
    {
        return gridData.ContainsKey(gridPos) ? gridData[gridPos].Object : null;
    }
    /// <summary>
    /// Returns the Tile component at the given grid position.
    /// </summary>
    public Tile GetTileAt(Vector2Int pos)
    {
        if (gridData.ContainsKey(pos) && gridData[pos].Ground != null)
        {
            return gridData[pos].Ground.GetComponent<Tile>();
        }
        return null; //  Returns null if no tile is found
    }

    /// <summary>
    /// Stores information about each tile in the grid.
    /// </summary>
    public class TileData
    {
        public GameObject Ground; // The ground tile at this position.
        public GameObject Object; // The pushable object at this position.

        public bool HasGround => Ground != null; // Check if ground exists.
        public bool HasObject => Object != null; // Check if an object exists.
    }

   
  
}
