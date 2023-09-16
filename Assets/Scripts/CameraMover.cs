using Cysharp.Threading.Tasks;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField] private Transform start;

    private Transform _camera;

    private void Awake()
    {
        if (Camera.main != null) _camera = Camera.main.transform;
        _camera.position = start.position;
        _camera.rotation = start.rotation;
    }

    public async UniTask MoveCameraOnEasing(Transform pos1, Transform pos2, float time)
    {
        var curve = AnimationCurve.EaseInOut(0f, 0f,time ,1f);
        var timer = 0f;

        while (timer < time)
        {
            timer += Time.deltaTime;
            var progress = curve.Evaluate(timer);
            _camera.position = pos1.position * (1f - progress) + pos2.position * progress;
            _camera.rotation = Quaternion.Slerp(pos1.rotation, pos2.rotation, progress);
            await UniTask.DelayFrame(1);
        }
            
        _camera.position = pos2.position;
        _camera.rotation = pos2.rotation;

    }
}