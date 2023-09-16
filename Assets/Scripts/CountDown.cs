using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TinyGiantStudio.Text;
using UnityEngine;

public class CountDown : MonoBehaviour
{
    public Modular3DText _modular3DText;
    [Space]
    [SerializeField] string _textAfterCountdownEnds = "";
    [Space]
    [SerializeField] int _duration = 10;

    private void Awake()
    {
        _modular3DText = gameObject.GetComponent<Modular3DText>();
    }

    public async UniTask Countdown()
    {
        _modular3DText.UpdateText(_duration.ToString());

        for (int i = _duration - 1; i > 0; i--)
        {
            await UniTask.Delay(1000);
            _modular3DText.UpdateText(i.ToString());
        }
        await UniTask.Delay(1000);
        _modular3DText.UpdateText(_textAfterCountdownEnds);
    }
}
