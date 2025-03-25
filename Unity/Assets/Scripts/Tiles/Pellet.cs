using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : MonoBehaviour
{
    public int points = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pacman"))
        {
            Eat();
        }
    }

    protected virtual void Eat()
    {
        //this.gameObject.SetActive(false);
        FindObjectOfType<GameManager>().PelletEaten(this);
    }


}
