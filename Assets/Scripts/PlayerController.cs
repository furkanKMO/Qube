using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public Vector2Int gridPos;
    public float moveSpeed = 5f;
    private Vector2Int lastValidPosition;

    void Awake()
    {
        Instance = this;
        lastValidPosition = gridPos; // Store initial valid position
    }
    private void Start()
    {
        RecordMovement();
    }
    public void RecordMovement()
    {
        //  Track object movement
        MovementHistory.Instance.AddMovement("Player", gridPos);
        foreach (Transform item in GridManager.Instance.objParent.transform)
        {
            if (item.TryGetComponent(out PushableObject pushable))
            {
                pushable.RecordMovement();
            }
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) Move(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.S)) Move(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.A)) Move(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.D)) Move(Vector2Int.right);
        if (Input.GetKeyDown(KeyCode.P)) MovementHistory.Instance.PrintHistory();
    }

    void Move(Vector2Int direction)
    {
        Vector2Int newPos = gridPos + direction;
        GameObject obj = GridManager.Instance.GetObjectAt(newPos);

        if (obj != null && obj.TryGetComponent(out PushableObject pushable))
        {
            Vector2Int pushPos = newPos + direction;
            if (GridManager.Instance.IsMoveable(pushPos)||GridManager.Instance.IsWater(pushPos)) // Only push if space available
            {
                pushable.TryMove(direction);
            }
            else return; // Stop movement if push isn't possible
        }

        if (GridManager.Instance.IsMoveable(newPos))
        {
            lastValidPosition = gridPos; //  Store last safe position before stepping on sinking block
            gridPos = newPos;
            StartCoroutine(SmoothMove(GridManager.Instance.GetWorldPosition(newPos)));
            RecordMovement();
            
            if (GridManager.Instance.IsSinkingBlock(newPos))
            {
                Tile tile = GridManager.Instance.GetTileAt(newPos);
                if (tile != null)
                {
                    tile.StartSinking();
                }

            }
        }
    }

    System.Collections.IEnumerator SmoothMove(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float elapsedTime = 0;
        while (elapsedTime < 0.1f)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / 0.1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
    }
    //  Move back to last valid position when tile sinks
    public void RevertToLastPosition()
    {
        StartCoroutine(SmoothMove(GridManager.Instance.GetWorldPosition(lastValidPosition)));
        gridPos = lastValidPosition;
    }
}
