using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public int points = 200;

    public Movement movement { get; private set; }

    // PATTERN: change to state machine??
    // // except, sometimes can have multiple behaviors active...
    public GhostHome home { get; private set; }
    public GhostScatter scatter { get; private set; }
    public GhostChase chase { get; private set; }
    public GhostScared scared { get; private set; }

    public GhostBehavior initialBehavior;

    // typically pacman
    public Transform target;

    //public SpriteRenderer body;
    //public SpriteRenderer eyes;
    //public SpriteRenderer bodyScared;
    //public SpriteRenderer bodyReturn2Reg;

    //public Sprite[] sprites_eyes;
    //public Sprite[] sprites_body;

    private void Awake()
     {
        this.movement = GetComponent<Movement>();
        this.home = GetComponent<GhostHome>();
        this.scatter = GetComponent<GhostScatter>();
        this.chase = GetComponent<GhostChase>();
        this.scared = GetComponent<GhostScared>();
    }

    private void Start()
    {
        //ResetState();
        this.scared.Disable();
        this.chase.Disable();
        this.scatter.Disable();
        this.home.Disable();

    }

    public void ResetState()
    {
        this.gameObject.SetActive(true);
        this.movement.ResetState();

        this.scared.Disable();
        this.chase.Disable();
        this.scatter.Enable();
        //this.home.Disable();

        // This SHOULD NOT be needed with the next if set...
        if (this.home != this.initialBehavior)
            this.home.Disable();

        if (this.initialBehavior != null)
            this.initialBehavior.Enable();

    }

    public void SetPosition(Vector3 position) 
    {
        // Keep the z-position the same since it determines draw depth
        position.z = transform.position.z;
        transform.position = position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            if(scared.enabled)
                FindObjectOfType<GameManager>().GhostEaten(this);
            else
                FindObjectOfType<GameManager>().PacmanEaten();
        }
    }
}
