using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class ARWorldMapManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private ARRaycastManager _raycastManager;
    [SerializeField] private ARAnchorManager _anchorManager;
    [SerializeField] private UnityEngine.XR.ARFoundation.Samples.ARWorldMapController _controller;

    [SerializeField] private Text _textLog;
    [SerializeField] private Button _buttonAddAnchor;

    [SerializeField] private GameObject _prefab;
    [SerializeField] private GameObject _tempAnchor;

    private Pose _hitPose;
    private ARPlane _hitPlane;
    private static List<ARRaycastHit> _hits = new List<ARRaycastHit>();

    [System.Obsolete]
    private void Start()
    {
        _controller.OnApplyARMap += OnLoadARMap;
        _buttonAddAnchor.onClick.AddListener(() => 
        {
            var anchor = _anchorManager.AttachAnchor(_hitPlane, _hitPose);
            var prefab = Instantiate<GameObject>(_prefab, anchor.transform);
            _textLog.text += $"Anchor Id: {anchor.trackableId}\n";
        });
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateRaycast();
    }

    public void UpdateRaycast()
    {
        if (!TryGetTouchPosition(out Vector2 touchPosition)) return;

        // AR Raycast
        if (_raycastManager.Raycast(touchPosition, _hits, TrackableType.PlaneWithinPolygon))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            var hitPose = _hits[0].pose;
            _hitPose = hitPose;
            _hitPlane = (ARPlane)_hits[0].trackable;
            _tempAnchor.transform.SetPositionAndRotation(_hitPose.position, _hitPose.rotation);
        }
    }

    private bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        bool status = false;
        touchPosition = default;
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                if (!IsPointOverUIObject(touch.position))
                {
                    touchPosition = touch.position;
                    status = true;
                }  
            }
        } 
        return status;
    }

    private bool IsPointOverUIObject(in Vector2 pos)
    {   
        var eventPosition = new PointerEventData(EventSystem.current);
        eventPosition.position = new Vector2(pos.x, pos.y);

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventPosition, results);
        
        return results.Count > 0;
    }

    private void OnLoadARMap()
    {
        _textLog.text += $"# of Anchors: {_anchorManager.trackables.count}";
        foreach (var anchor in _anchorManager.trackables)
        {
            _textLog.text += $"Anchor Id: {anchor.trackableId}\n";
            var newAnchor = Instantiate<GameObject>(_prefab, anchor.transform);
        }
    }
}
