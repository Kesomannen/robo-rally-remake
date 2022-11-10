using UnityEngine;

public class Board : MonoBehaviour {
    [SerializeField] RebootToken _rebootToken;

    public RebootToken RebootToken => _rebootToken;
}