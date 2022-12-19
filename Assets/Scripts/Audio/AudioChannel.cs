using UnityEngine;

public class AudioChannel : MonoBehaviour {
    [SerializeField] AudioSource _source;
    
    public AudioSource Source => _source;
}