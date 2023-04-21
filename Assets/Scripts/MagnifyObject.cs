using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnifyObject : MonoBehaviour
{
    Renderer _renderer;
    public Camera _cam;
    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (_cam) {
            Vector3 screenPoint = _cam.WorldToScreenPoint(transform.position);
            screenPoint.x = screenPoint.x / Screen.width;
            screenPoint.y = screenPoint.y / Screen.height;
            _renderer.material.SetVector("_ObjScreenPosition", screenPoint);
        }
        
    }
}
