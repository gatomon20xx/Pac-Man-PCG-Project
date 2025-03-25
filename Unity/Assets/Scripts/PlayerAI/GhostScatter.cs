using UnityEngine;

public class GhostScatter : GhostBehavior
{
    private void OnDisable()
    {
        ghost.chase.Enable();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Node node = collision.GetComponent<Node>();

        //Do nothing while the ghost is scared
        if (node != null && enabled && !ghost.scared.enabled)
        {
            // Pick a random available direction
            int index = Random.Range(0, node.availableDirections.Count);

            // Prefer not to go back the same direction so increment the index to
            // the next available direction
            if (node.availableDirections.Count > 1 && node.availableDirections[index] == -ghost.movement.direction)
            {
                //Ensures we don't overflow
                index %= index+1;

                // Wrap the index back around if overflowed
                //if (index >= node.availableDirections.Count)
                //{
                //    index = 0;
                //}
            }

            this.ghost.movement.SetDirection(node.availableDirections[index]);
        }
    }

}
