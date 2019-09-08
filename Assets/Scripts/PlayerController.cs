using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float JumpHeight;
    public GameObject ParticleSystem;
    public delegate void PlayerDied();
    public static event PlayerDied playerDied;

    private bool stillGrounded;
    private bool isCollidingLeft;
    private bool isCollidingRight;
    private bool isSwinging;
    private float swingDurationLeft;

    private int leftWall;
    private int floor;
    private int ceiling;
    private Rect rightWall;

    private float maxSpeed;
    private Vector3 velocity;
    private Vector3 gravity;
    private float damping;
    private List<GameObject> bricks;
    private Animator animator;

    private Rect head;
    private Rect left;
    private Rect right;
    private Rect bottom;
    private Rect grounded;

    // Start is called before the first frame update
    void Start()
    {
        stillGrounded = false;
        isCollidingLeft = isCollidingRight = false;
        maxSpeed = 0.5f;
        velocity = Vector3.zero;
        gravity = Vector3.down * 10;
        damping = 0.9f;
        leftWall = -32;
        rightWall = new Rect(32, 4, 8, 64);
        floor = 4;
        ceiling = 84;
        swingDurationLeft = 0.3f;
        animator = GetComponentInChildren<Animator>();

        head = left = right = bottom = grounded = new Rect();

    }

    // Update is called once per frame
    void Update()
    {
        if (GameState.currentState != GameState.CurrentState.Playing)
            return;

        if (bricks == null)
            bricks = GameObject.Find("GameWorld").GetComponent<GameHandler>().Bricks;

        MovePlayer();
    }

    void MovePlayer()
    {
        var moveX = Input.GetAxis("Horizontal");

        stillGrounded = false;
        isCollidingLeft = isCollidingRight = false;
        CheckForWalls();
        CheckForRightWall();
        CheckForBricks();

        if (stillGrounded && Input.GetButton("Jump"))
        {
            velocity = new Vector3(velocity.x, JumpHeight, 0);
            stillGrounded = false;
        }

        if (isSwinging)
        {
            swingDurationLeft -= Time.deltaTime;
            if (swingDurationLeft < 0)
            {
                isSwinging = false;
                animator.SetBool("Swinging", false);
                swingDurationLeft = 0.3f;
            }
        }

        if (!isSwinging && Input.GetButton("Fire1"))
        {
            isSwinging = true;
            animator.SetBool("Swinging", true);
            CheckForHit();
        }

        if (isCollidingLeft && moveX < 0)
            moveX = 0;

        if (isCollidingRight && moveX > 0)
            moveX = 0;

        velocity = new Vector3(GameState.PlayerSpeed * moveX * Time.deltaTime, velocity.y, 0);

        if (!stillGrounded)
            velocity += gravity * Time.deltaTime;
        else
            velocity = new Vector3(velocity.x, 0, 0);

        if (velocity.x > maxSpeed)
            velocity = new Vector3(1 * maxSpeed, velocity.y, velocity.z);

        if (velocity.x < -maxSpeed)
            velocity = new Vector3(-1 * maxSpeed, velocity.y, velocity.z);

        velocity *= damping;

        gameObject.transform.Translate(velocity);

        var position2D = new Vector2(transform.position.x, transform.position.y);
        head.Set(position2D.x - 1, position2D.y + 4, 2, 3);
        left.Set(position2D.x - 4, position2D.y - 5, 3, 11);
        right.Set(position2D.x + 1, position2D.y - 5, 3, 11);
        bottom.Set(position2D.x - 2, position2D.y - 8, 4, 2);
        grounded.Set(position2D.x - 2, position2D.y - 8.1f, 4, 2.1f);

        if (!stillGrounded)
            animator.SetBool("Falling", true);
        else
            animator.SetBool("Falling", false);

        if (stillGrounded && (velocity.x > 0 || velocity.x < 0))
            animator.SetBool("Walking", true);
        else
            animator.SetBool("Walking", false);

        if (velocity.x < 0)
            transform.localScale = new Vector2(-8, transform.localScale.y);
        else if (velocity.x > 0)
            transform.localScale = new Vector2(8, transform.localScale.y);
    }

    private void CheckForHit()
    {
        var playerAttackBox = new Rect();
        if (transform.localScale.x > 0)
            playerAttackBox = new Rect(transform.position.x, transform.position.y - 6.5f, 8, 6);
        else
            playerAttackBox = new Rect(transform.position.x - 8, transform.position.y - 6.5f, 0, 6);

        bool haveRemoved = false;
        float brickXPosition = 0;
        for (int i = 0; i < bricks.Count; i++)
        {
            if (haveRemoved)
            {
                ReactivateBricks(brickXPosition);
                break;
            }

            if (Intersects(playerAttackBox, bricks[i]))
            {
                Instantiate(ParticleSystem, bricks[i].transform.position, Quaternion.identity);
                brickXPosition = bricks[i].transform.position.x;
                Destroy(bricks[i]);
                bricks.RemoveAt(i);
                haveRemoved = true;
                GameState.ActiveBricks--;
                GameState.TotalBricksDestroyed++;
            }
        }
    }

    private void ReactivateBricks(float brickXPosition)
    {
        foreach (var brick in bricks)
        {
            if (brick.transform.position.x == brickXPosition)
            {
                brick.GetComponent<CubeHandler>().isFalling = true;
            }
        }
    }

    private void CheckForRightWall()
    {
        if (Intersects(right, rightWall))
        {
            isCollidingLeft = true;
            gameObject.transform.position = new Vector3(rightWall.xMin - 4, gameObject.transform.position.y, 0);
        }

        if (Intersects(bottom, rightWall))
        {
            stillGrounded = true;
            gameObject.transform.position = new Vector3(transform.position.x, rightWall.yMax + 8, 0);
        }

        if (Intersects(grounded, rightWall))
        {
            stillGrounded = true;
        }
    }

    private void CheckForBricks()
    {
        foreach (var brick in bricks)
        {
            var isSideCollision = false;

            if (brick.GetComponent<CubeHandler>().isFalling && Intersects(head, brick))
            {
                playerDied?.Invoke();
                animator.SetBool("Dying", true);
            }

            if (Intersects(left, brick))
            {
                isCollidingLeft = true;
                isSideCollision = true;
                gameObject.transform.position = new Vector3(brick.transform.position.x + 8, gameObject.transform.position.y, 0);
            }

            if (Intersects(right, brick))
            {
                isCollidingRight = true;
                isSideCollision = true;
                gameObject.transform.position = new Vector3(brick.transform.position.x - 8, gameObject.transform.position.y, 0);
            }

            if (Intersects(bottom, brick) && !isSideCollision)
            {
                stillGrounded = true;
                gameObject.transform.position = new Vector3(transform.position.x, brick.transform.position.y + 12, 0);
            }

            if (Intersects(grounded, brick))
            {
                stillGrounded = true;
            }
        }
    }

    private bool Intersects(Rect rect, GameObject brick)
    {
        var brickRect = new Rect(brick.transform.position.x - 4, brick.transform.position.y - 4, 8, 8);

        if (rect.xMin >= brickRect.xMax ||
            rect.xMax <= brickRect.xMin ||
            rect.yMax <= brickRect.yMin ||
            rect.yMin >= brickRect.yMax)
            return false;

        return Intersects(rect, brickRect);
    }

    private bool Intersects(Rect rect1, Rect rect2)
    {
        if (rect1.xMin >= rect2.xMax ||
            rect1.xMax <= rect2.xMin ||
            rect1.yMax <= rect2.yMin ||
            rect1.yMin >= rect2.yMax)
            return false;

        return true;
    }

    private void CheckForWalls()
    {
        if (transform.position.x < leftWall + 4)
        {
            isCollidingLeft = true;
            gameObject.transform.position = new Vector3(leftWall + 4, gameObject.transform.position.y, 0);
        }

        if (transform.position.y <= floor + 8)
        {
            stillGrounded = true;
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, floor + 8, 0);
        }

        if (transform.position.y + 8 >= ceiling)
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, ceiling - 8, 0);
        }
    }
}
