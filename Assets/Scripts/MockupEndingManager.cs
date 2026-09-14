using UnityEngine;
using UnityEngine.SceneManagement;

public class MockupEndingManager : MonoBehaviour
{
    public static MockupEndingManager Instance { get; private set; }

    public bool DoorRead { get; private set; }
    public bool ContinueRead { get; private set; }
    public bool BothRead => DoorRead && ContinueRead;

    void Awake()
    {
        Instance = this;
    }

    public void MarkDoorRead() => DoorRead = true;
    public void MarkContinueRead() => ContinueRead = true;

    public void TriggerLeaveEnding()
    {
        GameTimer.Finish();
        EndingScreenController.PendingMessage = "Он покинул пещеру и больше никогда туда не возвращался.";
        SceneManager.LoadScene("EndingScreen");
    }

    public void TriggerContinueEnding()
    {
        GameTimer.Finish();
        EndingScreenController.PendingMessage = "Продолжение следует...";
        SceneManager.LoadScene("EndingScreen");
    }
}
