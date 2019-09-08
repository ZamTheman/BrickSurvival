using Assets.Scripts;
using UnityEngine;

public class MenuClickHandlers : MonoBehaviour
{
    public GameObject InstructionCanvas;

    public void NewButtonClicked()
    {
        GameState.GameInitialized = false;
        GameState.CurrentLevel = 1;
        GameState.TotalBricksDestroyed = 0;
        GameState.currentState = GameState.CurrentState.Starting;
    }

    public void InstructionsButtonClicked()
    {
        InstructionCanvas.SetActive(true);
    }

    public void BackButtonClicked()
    {
        InstructionCanvas.SetActive(false);
    }

    public void ExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
