using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FPSMeter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float updateSpeed = 0.5f;
    private float timeElapsed = 0;
    private int frameCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.unscaledDeltaTime;
        frameCount++;

        if (timeElapsed >= updateSpeed)
        {
            UpdateFrameRate(timeElapsed, frameCount);
            frameCount = 0;
            timeElapsed -= updateSpeed;
        }
    }

    private void UpdateFrameRate(float timePassed, float framesPassed)
    {
        text.text = $"{Mathf.FloorToInt(framesPassed * (1 / timePassed))} FPS";

        if (Input.GetKey(KeyCode.Space))
        {
            text.text = $"{Mathf.FloorToInt((framesPassed * (1 / timePassed)) * 10)} FPS";
        }
    }
}
