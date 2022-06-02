using UnityEngine;

public class LogFPS : MonoBehaviour
{
    public float refreshTime = 1f; 
    [ReadOnly] public int frameCounter = 0;
    [ReadOnly] public float timeCounter = 0.0f;
    [ReadOnly] public float lastFramerate = 0.0f;

    void Start()
    {
        InvokeRepeating("ShowFPS", refreshTime, refreshTime);
    }

    void Update()
    {
        if (timeCounter < refreshTime)
        {
            timeCounter += Time.deltaTime;
            frameCounter++;
        }
        else
        {
            lastFramerate = (float)frameCounter / timeCounter;
            frameCounter = 0;
            timeCounter = 0.0f;
        }
    }

    public void ShowFPS()
    {
        Debug.Log("FPS: " + lastFramerate);
    }
}
