using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class PushableObject : MonoBehaviour
{
	Controller2D controller;
	Vector2 velocity;

	public float gravity = 24f;

	void Start ()
	{
		controller = GetComponent<Controller2D> ();
	}

	void Update ()
	{
		if (controller.collisionData.below)
		{
			velocity.y = 0f;
		}
        else
		{
			velocity += Vector2.down * gravity * Time.deltaTime;
			controller.Move(velocity * Time.deltaTime, false);
		}
	}

    public Vector2 Push(Vector2 amount)
    {
        return controller.Move(amount, false);
    }
}
