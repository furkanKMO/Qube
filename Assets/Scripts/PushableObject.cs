using UnityEngine;
using System.Collections;
public class PushableObject : MonoBehaviour
{
    public Vector2Int gridPos;
    private bool isStuck = false;
    void Awake()
    {
        if (GridManager.Instance != null)
        {
            gridPos = GridManager.Instance.WorldToGrid(transform.position);
        }
        else
        {
            Debug.LogError("GridManager Instance is missing! Make sure GridManager exists in the scene.");
        }
    }
    public void TryMove(Vector2Int direction)
    {
        if (isStuck) return; //  If stuck in water, do nothing
        
        Vector2Int newPos = gridPos + direction;

        if (GridManager.Instance.IsMoveable(newPos, true)) //  Pass 'true' to allow objects
        {
           
            GridManager.Instance.MoveObject(gridPos, newPos);
           
            StartCoroutine(SmoothMove(GridManager.Instance.GetWorldPosition(newPos)));
            gridPos = newPos;

            
            if (GridManager.Instance.IsIce(newPos))
            {
                StartCoroutine(Slide(direction));
            }
            //  Check if object moved into Water
            checkWater(newPos);
        }
        
    }
    IEnumerator Slide(Vector2Int direction)
    {
        Vector2Int currentPos = gridPos;
        Vector2Int nextPos = currentPos + direction;

        // Keep sliding while the next tile is ice and there�s no obstacle
        while (GridManager.Instance.IsIce(nextPos) && GridManager.Instance.IsMoveable(nextPos, true))
        {
            GridManager.Instance.MoveObject(currentPos, nextPos);
            StartCoroutine(SmoothMove(GridManager.Instance.GetWorldPosition(nextPos)));

            yield return new WaitForSeconds(0.1f); // Small delay to simulate sliding

            currentPos = nextPos;
            nextPos = currentPos + direction;
        }
        if (GridManager.Instance.IsMoveable(nextPos, true))
        {
            GridManager.Instance.MoveObject(currentPos, nextPos);
            StartCoroutine(SmoothMove(GridManager.Instance.GetWorldPosition(nextPos)));
            currentPos = nextPos;
        }
        checkWater(currentPos);
        //  Final position update after sliding
        gridPos = currentPos;
    }
    IEnumerator SmoothMove(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;
        float moveDuration = 0.1f; // Adjust for faster/slower movement

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }
    private void checkWater(Vector2Int newPos)
    {
        if (GridManager.Instance.IsWater(newPos))
        {

            if (GridManager.Instance.CheckTileWithSunkenObject(newPos))
            {
                // If a sunken object is already there, do nothing (act as a bridge)
            }
            else
            {
                isStuck = true; // Object is now unpushable
                GridManager.Instance.MarkTileWithSunkenObject(newPos, true); //  Mark this specific tile
            }
        }
    }
    public bool IsStuck() => isStuck;// Allow GridManager to check if object is stuck
}