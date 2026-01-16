using Fusion;
using UnityEngine;
using System.Collections.Generic;


public class AsteroidVisual : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnSpriteChanged))]
    public int SpriteIndex { get; set; }

    [SerializeField] private Sprite[] sprites;
    private SpriteRenderer sr;
    private PolygonCollider2D col;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<PolygonCollider2D>();
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            SpriteIndex = Random.Range(0, sprites.Length);
        }

        ApplySprite();
    }

    void OnSpriteChanged()
    {
        ApplySprite();
    }

    void ApplySprite()
    {
        if (SpriteIndex < 0 || SpriteIndex >= sprites.Length)
            return;

        sr.sprite = sprites[SpriteIndex];

        col.pathCount = sr.sprite.GetPhysicsShapeCount();

        for (int i = 0; i < col.pathCount; i++)
        {
            var path = new List<Vector2>();
            sr.sprite.GetPhysicsShape(i, path);
            col.SetPath(i, path);
        }
    }
}
