using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerController player;
    public GameObject startButton;

    [Header("Camera Transition")]
    public Transform cameraTransform; 

    public Vector3 startLocalPosition = new Vector3(0f, 5f, 7f);
    public Vector3 startLocalEuler = new Vector3(30f, 180f, 0f);

    public Vector3 gameplayLocalPosition = new Vector3(0f, 8f, -7f);
    public Vector3 gameplayLocalEuler = new Vector3(30f, 0f, 0f);

    public float transitionTime = 2f;

    private bool isTransitioning = false;
    private float elapsedTime = 0f;

    void Start()
    {
        cameraTransform.localPosition = startLocalPosition;
        cameraTransform.localEulerAngles = startLocalEuler;
    }

    void Update()
    {
        if (isTransitioning)
            UpdateCameraTransition();
    }

    public void StartGame()
    {
        player.Run();
        startButton.SetActive(false);
        StartCameraTransition();
    }

    private void StartCameraTransition()
    {
        elapsedTime = 0f;
        isTransitioning = true;
    }

    private void UpdateCameraTransition()
    {
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / transitionTime);
        t = t * t * (3f - 2f * t); 

        cameraTransform.localPosition = Vector3.Lerp(startLocalPosition, gameplayLocalPosition, t);
        cameraTransform.localEulerAngles = Vector3.Lerp(startLocalEuler, gameplayLocalEuler, t);

        if (t >= 1f)
            isTransitioning = false;
    }
}
