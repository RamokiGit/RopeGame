using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;


public class DraggableNode : MonoBehaviour
{
    
    private Vector3 _offset;
    private bool _isDragging = false;
    private float _areaX;
    private float _areaY;
    private Camera _mainCamera;
    private Coroutine _dragSound;
    [SerializeField] SpriteRenderer _shadow;
    [SerializeField] SpriteRenderer _glow;
    private bool _isGlow;
    private void Awake()
    {
        _mainCamera = Camera.main;
        _shadow.enabled = true;
        _glow.enabled = false;
    }

    void OnMouseDown()
    {
        if (GameStatus.CurrentGameState == GameState.Game)
        {
            AudioManager.Instance.PlaySoundClick();
            if (_dragSound != null) StopCoroutine(_dragSound);
            _isDragging = true;
            _dragSound = StartCoroutine(DragSoundCoroutine());
            _offset = transform.position - GetMouseWorldPosition();
            _shadow.enabled = false;
            
        }
    }

    IEnumerator DragSoundCoroutine()
    {
        while (_isDragging)
        {
            yield return new WaitForSeconds(Random.Range(1.8f, 2.8f));
            AudioManager.Instance.PlaySoundDrag();
        }
    }
    void OnMouseUp()
    {
        _isDragging = false;
        StopCoroutine(_dragSound);
        AudioManager.Instance.StopDrag();
        NodeManager.Instance.CheckForIntersections();
        _shadow.enabled = true;
    }

    private void OnMouseOver()
    {
        if (_isDragging) return;
        _isGlow = true;
        _glow.enabled = true;
    }

    private void OnMouseExit()
    {
        if (_isDragging) return;
        if (_isGlow) _glow.enabled = false;
    }

    void Update()
    {
        if (_isDragging) transform.position = ClampPosition();
    }
    
    public void SetArea(float areaX, float areaY)
    {
        _areaX = areaX;
        _areaY = areaY;
    }
    Vector3 ClampPosition()
    {
        Vector3 targetPosition = GetMouseWorldPosition() + _offset;
        targetPosition.x = Mathf.Clamp(targetPosition.x, -_areaX/2, _areaX/2);
        targetPosition.y = Mathf.Clamp(targetPosition.y, -_areaY/2, _areaY/2);
        return targetPosition;
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = _mainCamera.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
    

   
}