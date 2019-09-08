using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class CubeHandler : MonoBehaviour
{
    public delegate void CubeFirstLineWasGrounded();
    public static event CubeFirstLineWasGrounded cubeFirstLineWasGrounded;
    public delegate void CubeLastLineWasGrounded();
    public static event CubeLastLineWasGrounded cubeLastLineWasGrounded;
    public bool isFalling;
    public Material material1;
    public Material material2;
    public Material material3;
    public GameObject WorldGameObject;

    private bool isFirstLine;
    private bool isLastLine;
    private bool isSpawning;
    private const float fallSpeed = 20;
    private const float floor = 8;
    private GameHandler brickSpawner;
    private List<Material> materials;

    private void Awake()
    {
        isFalling = false;
        isFirstLine = transform.position.x == -28;
        isLastLine = transform.position.x == 28;
        isSpawning = true;
        transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
    }
    // Start is called before the first frame update
    void Start()
    {
        materials = new List<Material>
        {
           material1,
           material2,
           material3
        };
        GetComponent<Renderer>().material = (materials[Random.Range(0, 3)]);
        brickSpawner = WorldGameObject.GetComponent<GameHandler>();
    }

    private void OnDestroy()
    {
        if (isFirstLine)
            cubeFirstLineWasGrounded?.Invoke();

        if (isLastLine)
            cubeLastLineWasGrounded?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameState.currentState != GameState.CurrentState.Playing)
            return;

        if (isSpawning)
        {
            transform.localScale = new Vector3(
                transform.localScale.x + 1 * Time.deltaTime,
                transform.localScale.y + 1 * Time.deltaTime,
                transform.localScale.z + 1 * Time.deltaTime);

            if (transform.localScale.x > 1)
            {
                transform.localScale = new Vector3(1, 1, 1);
                isSpawning = false;
                isFalling = true;
            };
        }

        if (!isFalling)
            return;

        gameObject.transform.Translate(0, -(Time.deltaTime * fallSpeed), 0);

        if (gameObject.transform.position.y < floor)
        {
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, floor, 0);
            isFalling = false;

            if (isFirstLine)
                cubeFirstLineWasGrounded?.Invoke();

            if (isLastLine)
                cubeLastLineWasGrounded?.Invoke();
        }

        if (brickSpawner.Bricks.Count > 0)
        {
            foreach (var brick in brickSpawner.Bricks)
            {
                if (brick.transform.position.x != transform.position.x
                    || brick.transform == transform
                    || brick.GetComponent<CubeHandler>().isFalling)
                    continue;

                if (transform.position.y - brick.transform.position.y < 8 && transform.position.y - brick.transform.position.y > 0)
                {
                    transform.position = new Vector3(transform.position.x, brick.transform.position.y + 8, 0);
                    isFalling = false;

                    if (isFirstLine)
                        cubeFirstLineWasGrounded?.Invoke();

                    if (isLastLine)
                        cubeLastLineWasGrounded?.Invoke();
                }
            }
        }
    }
}
