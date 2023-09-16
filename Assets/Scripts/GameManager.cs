using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TinyGiantStudio.Text;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform[] cameraPoints;
    [SerializeField] private GameObject[] recordUI;
    [SerializeField] private CameraMover cameraMover;
    [SerializeField] private AutoTranscription autoTranscription;
    [SerializeField] private CountDown countDown;
    [SerializeField] private Modular3DText missionText;
    [SerializeField] private GameObject infoText;
    [SerializeField] private Modular3DText recordingViewText;
    [SerializeField] private Modular3DText katamariText;
    [SerializeField] private TypeWriter katamariViewText;

    private bool _isPlaying;
    public static string RecordingText;
    private Missions missions = new Missions();

    private async void Update()
    {
        if (_isPlaying) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            await GameFlow();
        }
    }

    private async UniTask GameFlow()
    {
        _isPlaying = true;
        RecordingText = "";
        countDown._modular3DText.UpdateText("10");
        countDown.gameObject.SetActive(true);
        
        // 初期設定
        var word = missions.Words[UnityEngine.Random.Range(0, missions.Words.Length)];
        katamariText.gameObject.SetActive(false);
        katamariViewText.gameObject.SetActive(false);
        recordUI[0].SetActive(true);
        missionText.UpdateText($"お題「{word}」");
        infoText.SetActive(true);
        recordingViewText.gameObject.SetActive(false);

        
        await cameraMover.MoveCameraOnEasing(cameraPoints[0], cameraPoints[1], 2f);

        await UniTask.Delay(1000);
        
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        autoTranscription.StartRecording();
        
        // 演出
        recordUI[0].SetActive(false);
        recordUI[1].SetActive(true);

        await countDown.Countdown();

        RecordingText = await autoTranscription.StopRecording();
        
        recordUI[1].SetActive(false);

        
        await UniTask.Delay(1000);
        
        // テキスト書き換え
        katamariViewText.gameObject.SetActive(true);
        await katamariViewText.Typing(RecordingText);
        
        await UniTask.Delay(3000);

        katamariViewText.gameObject.SetActive(false);
        katamariText.UpdateText(RecordingText);
        katamariText.gameObject.SetActive(true);

        await UniTask.Delay(2000);
        
        recordUI[2].SetActive(true);

        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        
        await cameraMover.MoveCameraOnEasing(cameraPoints[1], cameraPoints[2], 3f);
        
        await UniTask.Delay(2000);
        countDown.gameObject.SetActive(false);
        recordUI[2].SetActive(false);
        missionText.UpdateText($"正解「{word}」");
        infoText.SetActive(false);
        recordingViewText.UpdateText(RecordingText);
        recordingViewText.gameObject.SetActive(true);
        
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        
        await cameraMover.MoveCameraOnEasing(cameraPoints[2], cameraPoints[3], 3f);
        await cameraMover.MoveCameraOnEasing(cameraPoints[3], cameraPoints[4], 0.3f);
        
        await UniTask.Delay(2000);
        
        await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        
        await cameraMover.MoveCameraOnEasing(cameraPoints[4], cameraPoints[0], 2f);
        
        _isPlaying = false;
    }
}
