using UnityEngine;
using Fusion;

public class Health : NetworkBehaviour
{
    public enum DeathAction { DoNothingWhenDead, DestroyedWhenDead }

    [Networked] public float HealthPoints { get; set; }

    [Networked, OnChangedRender(nameof(OnAliveChanged))]
    public NetworkBool IsAlive { get; set; } = true;

    public float maxHealth = 20f;
    public int numberOfLives = 1;
    public DeathAction onLivesGone = DeathAction.DoNothingWhenDead;

    private Rigidbody2D _rb;
    private Collider2D _col;
    private Renderer[] _renderers;

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
        _renderers = GetComponentsInChildren<Renderer>();

        if (Object.HasStateAuthority)
        {
            HealthPoints = maxHealth;
            IsAlive = true;
        }

        UpdateVisuals(IsAlive);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        if (IsAlive && HealthPoints <= 0)
        {
            numberOfLives--;

            if (numberOfLives > 0)
            {
                HealthPoints = maxHealth;
            }
            else
            {
                IsAlive = false;

                if (BattleManager.Instance != null)
                {
                    BattleManager.Instance.OnPlayerDied(Object.InputAuthority);
                }
            }
        }
    }

    public void ApplyDamage(float amount)
    {
        if (!Object.HasStateAuthority) return;
        HealthPoints -= amount;
    }

    public void OnAliveChanged()
    {
        UpdateVisuals(IsAlive);

        if (!IsAlive && onLivesGone == DeathAction.DestroyedWhenDead)
        {

        }
    }

    private void UpdateVisuals(bool state)
    {
        if (_col) _col.enabled = state;
        if (_rb) _rb.simulated = state;

        foreach (var r in _renderers)
        {
            r.enabled = state;
        }
    }
}