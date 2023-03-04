using System.Collections;
using UnityEngine;

public class MusicPlayer : Singleton<MusicPlayer> {
    [SerializeField] SoundEffect _soundTrack;
    [SerializeField] float _delay;
    [SerializeField] [ReadOnly] bool _playing;

    public bool Playing {
        set {
            var prev = _playing;
            _playing = value;
            if (!prev && _playing) StartCoroutine(MusicLoop());
        }
    }

    void Start() {
        Playing = true;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        Playing = false;
    }

    IEnumerator MusicLoop() {
        while (_playing) {
            yield return AudioSystem.Instance.PlayAndWait(_soundTrack, AudioTrack.Music);
            yield return CoroutineUtils.Wait(_delay);
        }
    }
}