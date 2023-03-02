using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundEffect", menuName = "ScriptableObjects/Sound")]
public class SoundEffect : ScriptableObject {
    [SerializeField] AudioClip[] _clips;
    [SerializeField] [Range(0, 1)] float _volume = 0.7f;
    [SerializeField] [Range(0, 1)] float _volumeVariance = 0.1f;
    [SerializeField] [Range(0, 2)] float _pitch = 1f;
    [SerializeField] [Range(0, 1)] float _pitchVariance = 0.1f;
    [SerializeField] int _priority;
    [SerializeField] AudioTrack _preferredAudioTrack = AudioTrack.UI;

    public float Volume => _volume * Random.Range(1 - _volumeVariance, 1 + _volumeVariance);
    public float Pitch => _pitch * Random.Range(1 - _pitchVariance, 1 + _pitchVariance);
    public AudioClip Clip => _clips[Random.Range(0, _clips.Length)];
    public int Priority => _priority;
    
    public AudioTrack PreferredAudioTrack => _preferredAudioTrack;
}