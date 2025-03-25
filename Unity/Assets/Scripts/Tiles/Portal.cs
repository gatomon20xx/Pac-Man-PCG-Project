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
        collision.transform.position = new Vector3(this.portalExit.transform.position.x, this.portalExit.transform.position.y, collision.transform.position.z);
        GameObject go = collision.gameObject;

        Movement movement = go.GetComponent<Movement>();
        if (movement != null)
            movement.SetDirection(portalExit.exitTrajectory, true);
    }
}
