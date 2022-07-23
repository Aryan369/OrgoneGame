using UnityEngine;

public class Slash : MonoBehaviour
{
    private BoxCollider2D hitbox;

    private void Start()
    {
        hitbox = GetComponentInChildren<BoxCollider2D>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            print("hit enemy");
            Player.Instance.slashCooldownCounter = Player.Instance.slashCooldown;
        }
        
        hitbox.enabled = false;
    }

    #region Methods

    public void DoSlash(float angle)
    {
        transform.eulerAngles = new Vector3(0f, 0f, angle);
        hitbox.enabled = true;
    }
    
    public void Initiate(float _range)
    {
        hitbox.size = new Vector2(_range, 0.2f);
        hitbox.offset = new Vector2(_range / 2f, 0f);
        hitbox.enabled = false;
    }
    
    #endregion
}
