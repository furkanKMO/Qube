using System.Collections.Generic;
using UnityEngine;

public class MovementHistory : MonoBehaviour
{
    public static MovementHistory Instance;
    private Dictionary<string, List<Vector2Int>> movementData = new Dictionary<string, List<Vector2Int>>();
    private void Awake()
    {
        Instance = this;
    }
    /// <summary>
    /// Adds a new movement entry for a player or object.
    /// </summary>
    public void AddMovement(string entityName, Vector2Int position)
    {
        if (!movementData.ContainsKey(entityName))
        {
            movementData[entityName] = new List<Vector2Int>();
        }

        movementData[entityName].Add(position);
    }

    /// <summary>
    /// Gets the movement history for a specific entity.
    /// </summary>
    public List<Vector2Int> GetHistory(string entityName)
    {
        return movementData.ContainsKey(entityName) ? movementData[entityName] : new List<Vector2Int>();
    }

    /// <summary>
    /// Prints the entire movement history in an Excel-like format.
    /// </summary>
    public void PrintHistory()
    {
        string output = "Key\t0\t1\t2\t3\t4\t5\n";

        foreach (var entry in movementData)
        {
            output += entry.Key + "\t";
            foreach (var pos in entry.Value)
            {
                output += $"({pos.x},{pos.y})\t";
            }
            output += "\n";
        }

        Debug.Log(output);
    }
}
