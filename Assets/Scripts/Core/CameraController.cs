using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraController : MonoBehaviour, IGameStateListener<InGameState>
{
    // ======== 일반 카메라 ========

    [SerializeField] private Camera _uiCamera;
    public Camera UICamera => _uiCamera;

    private Camera _mainCamera;
    public Camera MainCamera => _mainCamera;

    // ======== 카메라 크기 ========

    // private CameraContentFitter _contentFitter;

    public float CameraWidth => CameraHeight * CameraAspect;
    public float CameraHeight => _mainCamera.orthographicSize * 2f;
    public float CameraAspect => (float)Screen.width / (float)Screen.height;

    // ======== 시네머신 ========

    private const int PRIORITY_HIGH = 20;
    private const int PRIORITY_LOW = 10;
    
    // [SerializeField] private CinemachineImpulseSource _attachImpulse;
    // [SerializeField] private CinemachineImpulseSource _slingImpulse;
    private CinemachineBrain _brain; // 카메라 간 블렌딩 관리
    private CinemachineCamera[] _allVCams = {};

    [Space]

    // ======== Idle ========

    [SerializeField] private CinemachineCamera _idleVCam;
    public CinemachineCamera IdleVCam => _idleVCam;
    [SerializeField] private CinemachinePositionComposer _idleComposer;
    [SerializeField] private CinemachineConfiner2D _idleConfiner;
    [SerializeField] private float _defaultIdleSize = 16f;  // idle 상태 기본 크기
    [SerializeField] private float _minIdleSize = 16f;
    [SerializeField] private float _maxIdleSize = 35f;
    [SerializeField] private float _aimZoomLerp = 8f;
    [SerializeField] private float _aimLeadDistance = 2f;

    [Space]

    // ======== Follow ========

    [SerializeField] private CinemachineCamera _followVCam;
    public CinemachineCamera FollowVCam => _followVCam;

    [Space]

    // ======== Enemy ========

    [SerializeField] private CinemachineCamera _enemyVCam;
    public CinemachineCamera EnemyVCam => _enemyVCam;

    [Space]

    // ======== Combat ========

    [SerializeField] private CinemachineCamera _combatVCam;
    public CinemachineCamera CombatVCam => _combatVCam;

    private void OnDestroy()
    {

    }

    public void Initialize()
    {
        _mainCamera = GetComponent<Camera>();

        // _contentFitter = GetComponent<CameraContentFitter>();
        // _contentFitter.Initialize(this);

        _brain = _mainCamera.GetComponent<CinemachineBrain>();
        _allVCams = new[] { _idleVCam, _followVCam, _combatVCam, _enemyVCam };
    }

    public void OnStateChanged(InGameState prevState, InGameState currState)
    {

    }

    // ======== VCam 전환 ========

    private void SetActiveVCam(CinemachineCamera activeVCam)
    {
        foreach (var vCam in _allVCams)
        {
            vCam.Priority = vCam == activeVCam ? PRIORITY_HIGH : PRIORITY_LOW;
        }
    }

    public void ActivateIdleVCam() => SetActiveVCam(_idleVCam);
    public void ActivateFollowVCam() => SetActiveVCam(_followVCam);
    public void ActivateEnemyVCam(Transform target)
    {
        _enemyVCam.Target.TrackingTarget = target;
        SetActiveVCam(_enemyVCam);
    }
    public void ActivateCombatVCam(Transform target)
    {
        _enemyVCam.Target.TrackingTarget = target;
        SetActiveVCam(_combatVCam);
    }

    // ======== Idle ========

    /// <summary>
    /// dragStrength: 0~1, dragDir: 발사 방향 정규화 벡터
    /// </summary>
    public void UpdateIdleVCam(float dragStrength, Vector2 dragDir)
    {
        var lens = _idleVCam.Lens;
        var targetSize = Mathf.Lerp(_minIdleSize, _maxIdleSize, dragStrength);
        lens.OrthographicSize = Mathf.Lerp(
            lens.OrthographicSize,
            targetSize,
            Time.deltaTime * _aimZoomLerp);
        _idleVCam.Lens = lens;
        _idleConfiner.InvalidateBoundingShapeCache();

        var targetOffset = (Vector3)(dragDir * _aimLeadDistance * dragStrength);
        _idleComposer.TargetOffset = Vector3.Lerp(_idleComposer.TargetOffset, targetOffset, Time.deltaTime * _aimZoomLerp);
    }

    public void ResetIdleVCam()
    {
        var lens = _idleVCam.Lens;
        var targetSize = _defaultIdleSize;
        lens.OrthographicSize = Mathf.Lerp(
            lens.OrthographicSize,
            targetSize,
            Time.deltaTime * _aimZoomLerp);
        _idleVCam.Lens = lens;
        _idleConfiner.InvalidateBoundingShapeCache();

        var targetOffset = Vector3.zero;
        _idleComposer.TargetOffset = Vector3.Lerp(_idleComposer.TargetOffset, targetOffset, Time.deltaTime * _aimZoomLerp);
    }

    // ======== Follow ========

    // ...

    // ======== Enemy ========

    // ...

    // ======== Combat ========

    // ...

    // ======== 카메라 제어 ========

    public void SetOrthographicSize(float size)
    {
        // var lens = _idleCinemachine.Lens;
        // lens.OrthographicSize = size;
        // _idleCinemachine.Lens = lens;
        // _uiCamera.orthographicSize = size;
    }

    // public void ShakeAttach(float force = 0.3f) => _attachImpulse?.GenerateImpulse(force);
    // public void ShakeSling(float force = 0.5f) => _slingImpulse?.GenerateImpulse(force);
}
