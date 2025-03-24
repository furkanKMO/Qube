using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPosition;
    public TileType tileType;
    public float floatTime = 2f; // Time before sinking
    public float sinkTime = 3f;  // Time before resurfacing
    private bool isSinking = false;

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

    //  Check if an object or player is on top
    public bool HasObjectOrPlayer()
    {
        return GridManager.Instance.GetObjectAt(gridPosition) != null || GridManager.Instance.IsPlayerAt(gridPosition);
    }

    //  Start the sinking process when something stands on the tile
    public void StartSinking()
    {
        if (!isSinking && tileType == TileType.SinkingBlock)
        {
            StartCoroutine(SinkAndRestore());
        }
    }

    IEnumerator SinkAndRestore()
    {
        isSinking = true;

        yield return new WaitForSeconds(floatTime); // Wait before sinking

        //  If something is still on top, revert movement
        if (HasObjectOrPlayer())
        {
            GridManager.Instance.RevertObjectsOnTile(gridPosition);
        }
        
            tileType = TileType.Block; // Sink the tile
            yield return new WaitForSeconds(sinkTime); // Wait before resurfacing
            tileType = TileType.SinkingBlock; // Restore tile
        

        isSinking = false;
    }
}


// Enum for different types of tiles (expandable)
public enum TileType
{
    Grass,
    Ice,
    Block,
    Water,
    SinkingBlock
}
