using Assets.Scripts;
using TMPro;
using UnityEngine;

public class ScoreHandler : MonoBehaviour
{
    private TextMeshProUGUI text;

    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        var level = GameState.CurrentLevel;
        var activeBricks = GameState.ActiveBricks;
        var destroyedBricks = GameState.TotalBricksDestroyed;

        text.text =
            $"Level: { level } \n\n" +
            $"Active bricks: { activeBricks } \n\n" +
            $"Destroyed bricks: { destroyedBricks } \n\n";
    }
}
