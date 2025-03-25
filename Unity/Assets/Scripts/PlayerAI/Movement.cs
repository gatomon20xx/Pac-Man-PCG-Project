using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public new Rigidbody2D rigidbody { get; private set; }

    // PCG
    public float speed = 8.0f;

    // PCG
    public float speedMultiplier = 1.0f;

    public bool isRotatable = false;

    public Vector2 initialDireciton;
    public LayerMask obstacleLayer;

    public Vector2 direction { get; private set; }
    
    // For cueing up movements (since pacman fits exactly, would have to hit EXACTLY right)
    public Vector2 nextDirection { get; private set; }

    public Vector3 startingPosition { get; private set; }
    
    void Awake()
    {
        this.rigidbody = GetComponent<Rigidbody2D>();

        // PCG: REQUEST FOR POS FROM DATA FILE?
        this.startingPosition = this.transform.position;
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetState()
    {
        this.speedMultiplier = 1.0f;
        this.direction = this.initialDireciton;
        this.nextDirection = Vector2.zero;
        this.transform.position = this.startingPosition;
        this.rigidbody.isKinematic = false;
        Rotate();
        this.enabled = true;
        
    }

    private void Rotate() 
    {
        if (this.isRotatable)
        {
            float angle = Mathf.Atan2(this.direction.y, this.direction.x);
            this.transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
        }
    }

    void Update()
    {
        if (this.nextDirection != Vector2.zero) 
        {
            SetDirection(this.nextDirection);
        }
    }

    private void FixedUpdate()
    {
        Vector2 position = this.rigidbody.position;
        Vector2 translation = this.direction * this.speed * this.speedMultiplier * Time.fixedDeltaTime;

        this.rigidbody.MovePosition(position + translation);
    }

    public void SetDirection(Vector2 direction, bool isForced = false)
    {
        // If not occpuied (or forcing movement) then go in direction
        if (isForced || !Occupied(direction))
        {
            this.direction = direction;
            this.nextDirection = Vector2.zero;

            // Only need to rotate with movement when change direction (not every frame!)
            Rotate();
            //if (this.isRotatable)
            //{
            //    float angle = Mathf.Atan2(this.direction.y, this.direction.x);
            //    this.transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
            //}
        }
        else // Check next frame if I can go in direction
        {
            this.nextDirection = direction;
        }
    
    }

    public bool Occupied(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(this.transform.position, Vector2.one * 0.75f, 0.0f, direction, 1.5f, this.obstacleLayer);
        return hit.collider != null;
    }


}
