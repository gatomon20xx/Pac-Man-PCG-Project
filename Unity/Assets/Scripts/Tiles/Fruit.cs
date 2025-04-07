using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit : MonoBehaviour
{
    public int[] pointsArray = { 100, 300, 500, 700, 1000, 2000, 3000, 5000 };
    public int points = 100;
    private SpriteRenderer spriteRenderer;

    public Sprite cherry;
    public Sprite strawberry;
    public Sprite peach;
    public Sprite apple;
    public Sprite melon;
    public Sprite galaxian;
    public Sprite bell;
    public Sprite keyItem;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 10;
        int random = (int)Random.Range(0, 7);
        switch (random)
        {
            case 0:
                spriteRenderer.sprite = cherry;
                points = pointsArray[0];
                break;
            case 1:
                spriteRenderer.sprite = strawberry;
                points = pointsArray[1];
                break;
            case 2:
                spriteRenderer.sprite = peach;
                points = pointsArray[2];
                break;
            case 3:
                spriteRenderer.sprite = apple;
                points = pointsArray[3];
                break;
            case 4:
                spriteRenderer.sprite = melon;
                points = pointsArray[4];
                break;
            case 5:
                spriteRenderer.sprite = galaxian;
                points = pointsArray[5];
                break;
            case 6:
                spriteRenderer.sprite = bell;
                points = pointsArray[6];
                break;
            case 7:
                spriteRenderer.sprite = keyItem;
                points = pointsArray[7];
                break;
        }
    }

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
        FindObjectOfType<GameManager>().FruitEaten(this);
    }


}
