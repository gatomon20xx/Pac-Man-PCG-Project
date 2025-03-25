using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Require Component: https://docs.unity3d.com/ScriptReference/RequireComponent.html
// Automatically adds required components as dependencies
[RequireComponent(typeof(SpriteRenderer))]
public class AnimateSprite : MonoBehaviour
{
    public SpriteRenderer spriteRenderer { get; private set; }
    public Sprite[] sprites;

    public float animationTime = 0.125f;

    public int animationFrame { get; private set; }

    public bool loop = true;

    void Awake()
    {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(Advance), this.animationTime, this.animationTime);
    }

    // Update is called once per frame
    void Advance()
    {
        if (!this.spriteRenderer.enabled)
            return;

        // TODO: IMPROVE.... If we don't loop....???? NOT SURE HOW THIS WILL BE USED...

        this.animationFrame++;
        if (this.loop)
            this.animationFrame %= this.sprites.Length;
        
        if (this.animationFrame >= 0 && this.animationFrame < this.sprites.Length)
            this.spriteRenderer.sprite = this.sprites[this.animationFrame];
    }

    public void Restart()
    {
        this.animationFrame = -1;

        Advance();
    }

}
