using UnityEngine;

public class PlayerControler : MonoBehaviour
{
    public float moveSpeed;
    public float jumpForce;
    public bool onGrownd;

    private Rigidbody2D rb;
    private Animator anim;
    private float horizontal;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Grownd"))
            onGrownd = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Grownd"))
            onGrownd = false;
    }

    private void FixedUpdate()
    {
        horizontal = Input.GetAxis("Horizontal");
        float jump = Input.GetAxisRaw("Jump");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(horizontal * moveSpeed,  rb.linearVelocity.y);

        if (horizontal > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (horizontal < 0)
            transform.localScale = new Vector3(1, 1, 1);

        if (vertical > 0.1f || jump > 0.1f)
        {
            if (onGrownd)
                movement.y = jumpForce;
        }

        rb.linearVelocity = movement;
    }

    private void Update()
    {
        anim.SetFloat("horizontal", horizontal);
    }
}
