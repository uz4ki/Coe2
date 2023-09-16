using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TinyGiantStudio.Text;
using UnityEngine;

public class TypeWriter : MonoBehaviour
{
    [SerializeField] private Modular3DText modular3DText;
    private int _currentLetter;


    public async UniTask Typing(string text)
    {
        _currentLetter = 0;
        var len = text.Length;
        var waitTime = 3000 / len;
        for (var i = _currentLetter; i <= len; i++)
        {
            modular3DText.Text = text[..i];

            await UniTask.Delay(waitTime);
            
            _currentLetter = i;
        }
    }
}
