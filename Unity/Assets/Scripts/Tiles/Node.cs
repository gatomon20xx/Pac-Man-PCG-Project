using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public LayerMask obstacleLayer;
    public LayerMask portalLayer;
    public List<Vector2> availableDirections { get; private set; }

    private void Start()
    {
        availableDirections = new List<Vector2>();

        // We determine if the direction is available by box casting to see if
        // we hit a wall. The direction is added to list if available.
        CheckAvailableDirection(Vector2.up);
        CheckAvailableDirection(Vector2.down);
        CheckAvailableDirection(Vector2.left);
        CheckAvailableDirection(Vector2.right);
    }

    private void CheckAvailableDirection(Vector2 direction)
    {
        RaycastHit2D hitA = Physics2D.BoxCast(transform.position, Vector2.one * 0.5f, 0f, direction, 1.0f, obstacleLayer);
        RaycastHit2D hitB = Physics2D.BoxCast(transform.position, Vector2.one * 5f, 0f, direction, 1.0f, portalLayer);

        // If no collider is hit then there is no obstacle in that direction
        if (hitA.collider == null && hitB.collider == null)
        {
            availableDirections.Add(direction);
        }
        else if (hitB.collider != null)
        {
            Debug.Log("Hit");
        }    
    }

    public void Reset()
    {
        // Since our mazes are made dynamically, we need to be able to reset our nodes whenever a new maze is made.
        availableDirections.Clear();

        CheckAvailableDirection(Vector2.up);
        CheckAvailableDirection(Vector2.down);
        CheckAvailableDirection(Vector2.left);
        CheckAvailableDirection(Vector2.right);
    }
}
