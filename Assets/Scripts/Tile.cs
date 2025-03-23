using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;  // Stores the tile's location in the grid
    public TileType tileType;        // Stores the type of the tile (can be expanded)
    void Awake()
    {
        if (GridManager.Instance != null)
        {
            gridPosition = GridManager.Instance.WorldToGrid(transform.position);
        }
        else
        {
            Debug.LogError("GridManager Instance is missing! Make sure GridManager exists in the scene.");
        }
    }
    public void Initialize(Vector2Int position, TileType type)
    {
        gridPosition = position;
        tileType = type;
    }
    public bool IsBlock()
    {
        return tileType == TileType.Block;
    }
}

// Enum for different types of tiles (expandable)
public enum TileType
{
    Grass,
    Ice,
    Block,
    Water
}
