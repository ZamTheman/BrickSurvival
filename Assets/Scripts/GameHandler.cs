using Assets.Scripts;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public GameObject EntryGate;
    public GameObject ExitGate;
    public GameObject SpawingBrick;
    public GameObject GameOverCanvas;
    public GameObject MenuCanvas;
    public GameObject Player;
    public GameObject Enemy;
    public GameObject LevelInfo;
    public GameObject LaserLine;
    public GameObject LaserBall;
    public GameObject ParticleSystem;
    public GameObject InstructionsCanvas;
    public TextMeshProUGUI LevelText;
    public List<GameObject> Bricks;

    private const int gateOpenYPosition = 92;
    private const int gateSpeed = 10;
    private const int winXPosition = 36;
    private const int gateStartY = 76;
    private const int infoDisplayTime = 3;
    private readonly Vector3 upperLeftCorner = new Vector3(-28, 88, 0);

    private GameObject fullScreenCanvas;
    private int entryGateTargetY;
    private int exitGateTargetY;
    private float timeToSpawn;
    private float infoHaveDisplayed;
    private GameObject player;
    private Animator playerAnimator;
    private GameObject enemy;
    private Animator enemyAnimator;
    private bool levelInfoShowing;
    private GameObject mainLine;
    private GameObject laserBall;
    private List<GameObject> lines;

    private bool entryGateOpen;
    private bool exitGateOpen;
    private bool mainLineCreated;
    private bool mainLineCompleted;
    private bool laserBallCreated;
    private bool laserBallCompleted;
    private bool lasersCreated;
    private bool lasersCompleted;

    void Start()
    {
        fullScreenCanvas = GameObject.Find("FullScreenCanvas");
        entryGateTargetY = exitGateTargetY = gateStartY;
        Bricks = lines = new List<GameObject>();
        levelInfoShowing = false;

        GameState.currentState = GameState.CurrentState.Menu;
        GameState.CurrentLevel = 1;
        GameState.SetSpawnSpeed();
        timeToSpawn = GameState.SpawnSpeed;

        PlayerController.playerDied += PlayerDiedHandler;
        CubeHandler.cubeFirstLineWasGrounded += BrickLine1GroundedHandler;
        CubeHandler.cubeLastLineWasGrounded += BrickLine2GroundedHandler;
    }

    void Update()
    {
        switch (GameState.currentState)
        {
            case GameState.CurrentState.Menu:
                return;
            case GameState.CurrentState.Starting:
                GameStartingUpdate();
                return;
            case GameState.CurrentState.Playing:
                PlayingUpdate();
                return;
            case GameState.CurrentState.Winning:
                WinningUpdate();
                return;
            case GameState.CurrentState.LevelChange:
                LevelChangeUpdate();
                return;
            case GameState.CurrentState.Ending:
                EndingUpdate();
                return;
            case GameState.CurrentState.BadGuyEnding:
                BadGuyEndingUpdate();
                return;
        }
    }

    private void BadGuyEndingUpdate()
    {
        if (!enemyAnimator.GetBool("Firing"))
            enemyAnimator.SetBool("Firing", true);

        if (!mainLineCreated)
        {
            mainLine = Instantiate(LaserLine, new Vector3(-33.9f, 76.73f, 0), Quaternion.identity);
            mainLine.transform.Rotate(new Vector3(0, 90, 0));
            mainLine.transform.localScale = new Vector3(1, 1, 0.1f);
            mainLineCreated = true;
        }

        if (!mainLineCompleted)
        {
            mainLine.transform.localScale = new Vector3(1, 1, mainLine.transform.localScale.z + Time.deltaTime * 20);
            if (mainLine.transform.localScale.z > 33)
            {
                mainLine.transform.localScale = new Vector3(1, 1, 33);
                mainLineCompleted = true;
            }

            return;
        }

        if (!laserBallCreated)
        {
            laserBall = Instantiate(LaserBall, new Vector3(0, 77, 0), Quaternion.identity);
            laserBall.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            laserBallCreated = true;
        }

        if (!laserBallCompleted)
        {
            laserBall.transform.localScale = new Vector3(
                laserBall.transform.localScale.x + Time.deltaTime * 10,
                laserBall.transform.localScale.y + Time.deltaTime * 10,
                laserBall.transform.localScale.z + Time.deltaTime * 10);

            if (laserBall.transform.localScale.x > 8)
            {
                laserBall.transform.localScale = new Vector3(8, 8, 8);
                laserBallCompleted = true;
            }

            return;
        }

        if (!lasersCreated)
        {
            lines = new List<GameObject>();
            for (int i = 0; i < 15; i++)
            {
                var line = Instantiate(LaserLine, new Vector3(0, 77, 0), Quaternion.identity);
                line.transform.Rotate(0, 90, 0);
                line.transform.Rotate(11.25f + 11.25f * i, 0, 0);
                line.transform.localScale = new Vector3(1, 1, 1);
                lines.Add(line);
            }

            lasersCreated = true;
        }

        if (!lasersCompleted)
        {
            foreach (var line in lines)
            {
                line.transform.localScale = new Vector3
                (
                    line.transform.localScale.x,
                    line.transform.localScale.y,
                    line.transform.localScale.z + Time.deltaTime * 200
                );

                if (line.transform.localScale.z > 75)
                {
                    line.transform.localScale = new Vector3
                    (
                        line.transform.localScale.x,
                        line.transform.localScale.y,
                        75
                    );

                    lasersCompleted = true;
                }
            }

            return;
        }

        foreach (var brick in Bricks)
        {
            Instantiate(ParticleSystem, brick.transform.position, Quaternion.identity);
            Destroy(brick);
        }

        GameState.currentState = GameState.CurrentState.Ending;
    }

    private void EndingUpdate()
    {
        if (!GameOverCanvas.activeSelf)
            GameOverCanvas.SetActive(true);

    }

    private void LevelChangeUpdate()
    {
        if (!levelInfoShowing)
        {
            LevelInfo.SetActive(true);
            LevelInfo.SetActive(true);
            LevelText.text = GameState.CurrentLevel.ToString();
            levelInfoShowing = true;
            infoHaveDisplayed = 0;
        }

        infoHaveDisplayed += Time.deltaTime;
        
        if (infoHaveDisplayed > infoDisplayTime)
        {
            LevelInfo.SetActive(false);
            levelInfoShowing = false;
            GameState.currentState = GameState.CurrentState.Starting;
            CleanPlayField();
        }
    }

    private void CleanPlayField()
    {
        foreach (var brick in Bricks)
        {
            Destroy(brick);
        }
        Destroy(mainLine);
        Destroy(laserBall);
        foreach (var line in lines)
        {
            Destroy(line);
        }
        lines.Clear();

        Bricks.Clear();
        Destroy(player);
        Destroy(enemy);
        GameState.GameInitialized = false;
    }

    private void WinningUpdate()
    {
        player.transform.position = new Vector3(
            player.transform.position.x + Time.deltaTime * GameState.PlayerSpeed,
            player.transform.position.y,
            player.transform.position.z);

        if (player.transform.position.x > 85)
        {
            GameState.currentState = GameState.CurrentState.LevelChange;
            GameState.CurrentLevel++;
            GameState.SetSpawnSpeed();
        }
    }

    private void GameStartingUpdate()
    {
        if (!GameState.GameInitialized)
            InitializeGame();

        if (player.transform.position.x > -28)
        {
            if (playerAnimator.GetBool("Walking") == true)
                playerAnimator.SetBool("Walking", false);

            if (playerAnimator.GetBool("Falling") == false)
                playerAnimator.SetBool("Falling", true);

            if (enemyAnimator.GetBool("Walking") == true)
                enemyAnimator.SetBool("Walking", false);

            player.transform.position = new Vector3(
                player.transform.position.x + GameState.PlayerSpeed * 0.25f * Time.deltaTime,
                player.transform.position.y - GameState.PlayerSpeed * Time.deltaTime,
                0);

            EntryGate.transform.position = new Vector3(
                EntryGate.transform.position.x,
                76,
                EntryGate.transform.position.z);

            ExitGate.transform.position = new Vector3(
                ExitGate.transform.position.x,
                76,
                ExitGate.transform.position.z);
        }
        else
        {
            player.transform.position = new Vector3(
                player.transform.position.x + GameState.PlayerSpeed * Time.deltaTime,
                player.transform.position.y,
                0);

            enemy.transform.position = new Vector3(
                enemy.transform.position.x + GameState.PlayerSpeed * Time.deltaTime,
                enemy.transform.position.y,
                0);
        }

        if (player.transform.position.y < 20)
        {
            player.transform.position = new Vector3(
                player.transform.position.x,
                20,
                0);

            GameState.currentState = GameState.CurrentState.Playing;
        }
    }

    private void InitializeGame()
    {
        GameOverCanvas.SetActive(false);
        MenuCanvas.SetActive(false);

        CleanPlayField();

        entryGateOpen = exitGateOpen = false;
        mainLineCreated = mainLineCompleted = laserBallCreated =
            laserBallCompleted = lasersCreated = lasersCompleted = false;

        player = Instantiate(Player, new Vector3(-80, 75.8f, 0), Quaternion.identity);
        playerAnimator = player.GetComponentInChildren<Animator>();
        playerAnimator.SetBool("Walking", true);

        enemy = Instantiate(Enemy, new Vector3(-90, 68, 0), Quaternion.identity);
        enemy.transform.localScale = new Vector3(5.5f, 5.5f, 5.5f);
        enemyAnimator = enemy.GetComponent<Animator>();
        enemyAnimator.SetBool("Walking", true);

        EntryGate.transform.position = new Vector3(
            EntryGate.transform.position.x,
            EntryGate.transform.position.y + 16,
            EntryGate.transform.position.z);

        ExitGate.transform.position = new Vector3(
            ExitGate.transform.position.x,
            ExitGate.transform.position.y + 16,
            ExitGate.transform.position.z);

        GameState.GameInitialized = true;
        GameState.ActiveBricks = 0;
    }

    private void PlayingUpdate()
    {
        timeToSpawn -= Time.deltaTime;
        if (timeToSpawn < 0)
        {
            SpawnObject();
            timeToSpawn = GameState.SpawnSpeed;
        }

        UpdateGates();
        UpdatePlayerIfNotGateOpen();
    }

    private void BrickLine1GroundedHandler() => BrickGroundedHandler(1);
    private void BrickLine2GroundedHandler() => BrickGroundedHandler(2);

    private void BrickGroundedHandler(int line)
    {
        int nrBricksInHeight = 0;
        int lineX = line == 1 ? GameState.firstLineX : GameState.lastLineX;

        foreach (var brick in Bricks)
        {
            if (brick == null)
                continue;

            if (brick.transform.position.x == lineX && !brick.GetComponent<CubeHandler>().isFalling)
                nrBricksInHeight++;
        }

        int targetY = gateStartY + 2 * nrBricksInHeight;

        if (line == 1)
            entryGateTargetY = targetY;
        else
            exitGateTargetY = targetY;
    }

    private void PlayerDiedHandler()
    {
        GameState.currentState = GameState.CurrentState.Ending;
        GameOverCanvas.SetActive(true);
        foreach (var brick in Bricks)
        {
            brick.GetComponent<CubeHandler>().isFalling = false;
        }
    }

    private void UpdatePlayerIfNotGateOpen()
    {
        if (!exitGateOpen && player.transform.position.x > 28)
            player.transform.position = new Vector3(28, player.transform.position.y, 0);

        if (player.transform.position.x >= winXPosition)
            GameState.currentState = GameState.CurrentState.Winning;
    }

    private void UpdateGates()
    {
        if (EntryGate.transform.position.y != entryGateTargetY && !entryGateOpen)
        {
            if (Math.Abs(entryGateTargetY - EntryGate.transform.position.y) < 0.1f)
                EntryGate.transform.position = new Vector3(EntryGate.transform.position.x, entryGateTargetY, 10);
            else
            {
                var direction = EntryGate.transform.position.y - entryGateTargetY < 0 ? 1 : -1;
                EntryGate.transform.position = new Vector3(EntryGate.transform.position.x, EntryGate.transform.position.y + direction * Time.deltaTime * gateSpeed, 10);
            }
        }

        if (EntryGate.transform.position.y == gateOpenYPosition)
            GameState.currentState = GameState.CurrentState.BadGuyEnding;

        if (ExitGate.transform.position.y != exitGateTargetY && !exitGateOpen)
        {
            if (Math.Abs(exitGateTargetY - ExitGate.transform.position.y) < 0.1f)
                ExitGate.transform.position = new Vector3(ExitGate.transform.position.x, exitGateTargetY, 10);
            else
            {
                var direction = ExitGate.transform.position.y - exitGateTargetY < 0 ? 1 : -1;
                ExitGate.transform.position = new Vector3(ExitGate.transform.position.x, ExitGate.transform.position.y + direction * Time.deltaTime * gateSpeed, 10);
            }
        }

        if (ExitGate.transform.position.y == gateOpenYPosition)
            exitGateOpen = true;
    }

    private void SpawnObject()
    {
        if (Bricks.Count > 63)
            return;

        var canSpawnBrick = false;
        var potentialSpawnPosition = Vector3.zero;
        while (!canSpawnBrick)
        {
            var position = UnityEngine.Random.Range(0, 8);
            potentialSpawnPosition = new Vector3(upperLeftCorner.x + position * 8, upperLeftCorner.y, upperLeftCorner.z);

            if (CanSpawnObject(potentialSpawnPosition))
                canSpawnBrick = true;
        }

        GameObject newObject = Instantiate(SpawingBrick, potentialSpawnPosition, Quaternion.identity);
        newObject.GetComponent<CubeHandler>().WorldGameObject = gameObject;
        Bricks.Add(newObject);
        GameState.ActiveBricks++;
    }

    private bool CanSpawnObject(Vector3 potentialSpawnPosition)
    {
        foreach (var brick in Bricks)
        {
            if (brick.transform.position.x != potentialSpawnPosition.x)
                continue;

            if (potentialSpawnPosition.y - brick.transform.position.y <= 24)
                return false;
        }

        return true;
    }
}
