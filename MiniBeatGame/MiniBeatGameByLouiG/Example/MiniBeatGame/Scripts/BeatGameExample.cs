using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameByLouiG;
using System;

public class BeatGameExample : MonoBehaviour
{
    [SerializeField]
    private MiniBeatGame beatGame = null;
    [SerializeField]
    private int perfectScore = 0;
    [SerializeField]
    private int normalScore = 0;
    [SerializeField]
    private int missScore = 0;
    [SerializeField]
    private Text gameDis = null;
    [SerializeField]
    private Text perfectCountText = null;
    [SerializeField]
    private Text normalCountText = null;
    [SerializeField]
    private Text missCountText = null;
    [SerializeField]
    private RectTransform hintArrowMark = null;
    [SerializeField]
    private Text timingText = null;
    [SerializeField]
    private float timingTextUpSpeed = 0f;
    [SerializeField]
    private float timingTextAlphaSpeed = 0f;

    private RectTransform timingTextRect = null;

    private float score = 0;

    private GameState gameState = GameState.idle;

    private int perfectCount = 0;
    private int normalCount = 0;
    private int missCount = 0;

    void Start()
    {
        gameState = GameState.idle;
        gameDis.text = "Press Left or right mouse button start";
        timingTextRect = timingText.GetComponent<RectTransform>();
        EventRegister();
        timingText.enabled = false;
    }

    void Update()
    {
        switch(gameState)
        {
            case GameState.idle:
                if (Input.GetButtonUp("Fire1"))
                {
                    beatGame.GameStart(0);
                    gameDis.text = "Press R re start";
                    gameState = GameState.gaming;
                }
                if (Input.GetButtonUp("Fire2"))
                {
                    beatGame.GameStart(1);
                    gameDis.text = "Press R re start";
                    gameState = GameState.gaming;
                }
                break;
            case GameState.gaming:
                if (!beatGame.GetSpinState())
                {
                    ScoreCount();
                    gameDis.text = "Score are " + score + "\nPress R re start";
                    gameState = GameState.end;
                }
                if (Input.GetKeyUp(KeyCode.R))
                {
                    ReSetGame();
                    gameDis.text = "Press Left or right mouse button start";
                }
                break;
            case GameState.end:
                if (Input.GetKeyUp(KeyCode.R))
                {
                    ReSetGame();
                    gameDis.text = "Press Left or right mouse button start";
                }
                break;
            case GameState.stop:
                break;
        }
    }

    private void ReSetGame()
    {
        perfectCount = 0;
        normalCount = 0;
        missCount = 0;
        perfectCountText.text = "0";
        normalCountText.text = "0";
        missCountText.text = "0";
        beatGame.ReSetGame();
        timingText.enabled = false;
        gameState = GameState.idle;
    }

    private void ScoreCount()
    {
        Vector3Int beatData = beatGame.GetBeatData();
        int beatCount = beatGame.GetBeatCount();
        int perfectCount = beatData.x;
        int normalCount = beatData.y;
        int missCount = beatData.z;
        score = ((float)((perfectCount * perfectScore) 
            + (normalCount * normalScore) 
            + (missCount * missScore)) 
            / beatCount) * 10f;
    }

    private void UpdatePerfectCount(object sender, EventArgs e)
    {
        perfectCount++;
        UpdateBeatTimingCount(BeatTimingType.Perfect);
    }

    private void UpdateNormalCount(object sender, EventArgs e)
    {
        normalCount++;
        UpdateBeatTimingCount(BeatTimingType.Normal);
    }

    private void UpdateMissCount(object sender, EventArgs e)
    {
        missCount++;
        UpdateBeatTimingCount(BeatTimingType.Miss);
    }

    private void UpdateBeatTimingCount(BeatTimingType beatTimingType)
    {
        switch(beatTimingType)
        {
            case BeatTimingType.Perfect:
                perfectCountText.text = perfectCount.ToString();
                break;
            case BeatTimingType.Normal:
                normalCountText.text = normalCount.ToString();
                break;
            case BeatTimingType.Miss:
                missCountText.text = missCount.ToString();
                break;
        }
    }

    private void ShowPerfectText(object sender, EventArgs e)
    {
        ShowTimingText(BeatTimingType.Perfect);
    }

    private void ShowNormalText(object sender, EventArgs e)
    {
        ShowTimingText(BeatTimingType.Normal);
    }

    private void ShowMissText(object sender, EventArgs e)
    {
        ShowTimingText(BeatTimingType.Miss);
    }

    private void ShowTimingText(BeatTimingType beatTimingType)
    {
        timingTextRect.position = hintArrowMark.position;
        timingText.enabled = true;
        switch (beatTimingType)
        {
            case BeatTimingType.Perfect:
                timingText.color = Color.green;
                timingText.text = "Perfect!!!";
                break;
            case BeatTimingType.Normal:
                timingText.color = Color.yellow;
                timingText.text = "Normal!";
                break;
            case BeatTimingType.Miss:
                timingText.color = Color.red;
                timingText.text = "Miss...";
                break;
        }
        StartCoroutine(textFloating());
    }

    IEnumerator textFloating()
    {
        while(timingText.color.a >=0 && timingText.enabled)
        {
            Vector3 oldPos = timingTextRect.localPosition;
            timingTextRect.localPosition = new Vector3
                (oldPos.x
                , oldPos.y + timingTextUpSpeed * Time.deltaTime
                , oldPos.z);
            Color oldColor = timingText.color;
            timingText.color = new Color(oldColor.r
                , oldColor.g
                , oldColor.b
                , oldColor.a - Time.deltaTime * timingTextAlphaSpeed);
            yield return null;
        }
    }

    private void EventRegister()
    {
        beatGame.AddBeatTimingTriggerEvent(ShowPerfectText, BeatTimingType.Perfect);
        beatGame.AddBeatTimingTriggerEvent(UpdatePerfectCount, BeatTimingType.Perfect);

        beatGame.AddBeatTimingTriggerEvent(ShowNormalText, BeatTimingType.Normal);
        beatGame.AddBeatTimingTriggerEvent(UpdateNormalCount, BeatTimingType.Normal);

        beatGame.AddBeatTimingTriggerEvent(ShowMissText, BeatTimingType.Miss);
        beatGame.AddBeatTimingTriggerEvent(UpdateMissCount, BeatTimingType.Miss);
    }
}


public enum GameState
{
    idle = 0,
    gaming,
    stop,
    end
}