using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;
using Audio;
using WhisperAPI;
using WhisperAPI.Models;
using TMPro;



public class AutoTranscription : MonoBehaviour
{
    private AudioClip _recordedClip;
    private string MicName = "マイク (Yeti Nano)"; //マイクデバイスの名前
    private const int SamplingFrequency = 44100; //サンプリング周波数
    private const int MaxTimeSeconds = 10; //最大録音時間[s]
    
    private bool _isRecording = false;

    private CancellationTokenSource _cts = new();
    private CancellationToken _token;

    private WhisperAPIConnection _whisperConnection;
    
    
    private void Start()
    {
        // ShowAllMicDevices();
        
        _token = _cts.Token;
        _whisperConnection = new(Constants.API_KEY);
        
    }


    public void StartRecording()
    {
        Debug.Log("recording start");
        MicName = Microphone.devices[0];
        _isRecording = true;
        _recordedClip = Microphone.Start(deviceName: MicName, loop: false, lengthSec: MaxTimeSeconds, frequency: SamplingFrequency);

    }



    public async UniTask<string> StopRecording()
    {
        if (Microphone.IsRecording(deviceName: MicName))
        {
            Debug.Log("recording stopped");
            Microphone.End(deviceName: MicName);
        }
        else
        {
            Debug.Log("not recording");
            return "";
        }
        _isRecording = false;

        byte[] recordWavData = WavConverter.ToWav(_recordedClip);
        
        // WhisperAPI
        return await DisplayWhisperResponse(recordWavData);
    }


    private async UniTask<string> DisplayWhisperResponse(byte[] recordData)
    {
        WhisperAPIResponseModel responseModel = await _whisperConnection.RequestWithVoiceAsync(recordData, _token);
        return responseModel.text;
    }



    /// <summary>
    /// 自分が使用中のマイクを特定する際に利用
    /// </summary>
    private void ShowAllMicDevices()
    {
        foreach (string device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }
    }
}