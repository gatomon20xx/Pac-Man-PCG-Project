using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPellet : Pellet
{
    public float duration = 8.0f;

    protected override void Eat()
    {
        //base.Eat();  //this.gameObject.SetActive(false);

        FindObjectOfType<GameManager>().PowerPelletEaten(this);

    }
}
