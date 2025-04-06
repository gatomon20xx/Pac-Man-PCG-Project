using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    //public Transform portalExit;

    // My update... :)
    public PortalExit portalExit;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        collision.transform.position = new Vector3(Mathf.Sign(collision.transform.position.x) * -13, collision.transform.position.y, collision.transform.position.z);
        Debug.Log("In collision");
        GameObject go = collision.gameObject;

        Movement movement = go.GetComponent<Movement>();
        if (movement != null)
            movement.SetDirection(new Vector2(movement.direction.x, 0), true);
    }
}
