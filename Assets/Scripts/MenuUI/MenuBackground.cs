using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MenuBackground : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] float _timeScale;
    [SerializeField] float _maxIntensity;
    [Space]
    [SerializeField] float _fadeDuration;
    [SerializeField] float _mapDuration;
    
    [Header("References")]
    [SerializeField] Light2D _globalLight;
    [SerializeField] MapData[] _allowedMaps;

    readonly List<MapData> _mapPool = new();
    GameObject _map;
    (float BaseIntensity, Light2D Light)[] _lights;

    void Start() {
        PopulatePool();
        
        _globalLight.intensity = _maxIntensity;
        Time.timeScale = _timeScale;
        LoadNewMap();
        
        StartCoroutine(MapLoop());
    }

    void OnDestroy() {
        Time.timeScale = 1;
    }

    void PopulatePool() {
        foreach (var map in _allowedMaps) {
            _mapPool.Add(map);
        }
    }
    
    void LoadNewMap() {
        if (_map != null) Destroy(_map);
        
        if (_mapPool.Count == 0) PopulatePool();
        var randomIndex = Random.Range(0, _mapPool.Count);
        
        _map = Instantiate(_mapPool[randomIndex].Prefab, transform);
        _mapPool.RemoveAt(randomIndex);
        _map.transform.localPosition = Vector3.zero;

        foreach (var text in _map.GetComponentsInChildren<TMP_Text>()) {
            text.enabled = false;
        }
        _lights = _map.GetComponentsInChildren<Light2D>().Select(l => (l.intensity, l)).ToArray();
    }

    IEnumerator MapLoop() {
        while (enabled) {
            yield return CoroutineUtils.Wait(_mapDuration);
            LeanTween
                .value(gameObject, 0, 1, _fadeDuration / 2)
                .setOnUpdate(f => {
                    _globalLight.intensity = Mathf.Lerp(_maxIntensity, 0, f);
                    foreach (var (baseIntensity, light2D) in _lights) {
                        light2D.intensity = Mathf.Lerp(baseIntensity, 0, f);
                    }
                });
            yield return CoroutineUtils.Wait(_fadeDuration / 2);
            LoadNewMap();
            foreach (var (_, light2D) in _lights) {
                light2D.intensity = 0;
            }   
            LeanTween
                .value(gameObject, 0, 1, _fadeDuration / 2)
                .setOnUpdate(f => {
                    _globalLight.intensity = Mathf.Lerp(0, _maxIntensity, f);
                    foreach (var (baseIntensity, light2D) in _lights) {
                        light2D.intensity = Mathf.Lerp(0, baseIntensity, f);
                    }
                });
            yield return CoroutineUtils.Wait(_fadeDuration / 2);
        }
    }
}