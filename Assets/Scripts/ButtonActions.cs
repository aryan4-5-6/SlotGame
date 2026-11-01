using UnityEngine;

/// <summary>Reusable public actions to connect to Unity UI Button On Click events.</summary>
public class ButtonActions : MonoBehaviour
{
    [SerializeField] private GameObject winPopup;

    /// <summary>Closes the win popup, allowing the player to continue.</summary>
    public void Continue()
    {
        if (winPopup != null)
            winPopup.SetActive(false);
        else
            Debug.LogWarning("ButtonActions: Assign the Win Popup GameObject in the Inspector.", this);
    }

    /// <summary>Quits the built game. In the Unity Editor, logs a message instead.</summary>
    public void Exit()
    {
        Debug.Log("Exit button pressed.");
        Application.Quit();
    }
}
