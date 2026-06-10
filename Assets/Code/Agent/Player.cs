using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float speed;

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal"); 
        float moveVertical = Input.GetAxisRaw("Vertical");   

        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical).normalized;
        Vector3 rotatedMovement = transform.rotation * movement;

        _rigidbody.velocity = new Vector3(rotatedMovement.x * speed, _rigidbody.velocity.y, rotatedMovement.z * speed);
    }
}
