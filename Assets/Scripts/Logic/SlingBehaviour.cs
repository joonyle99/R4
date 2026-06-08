using UnityEngine;

public class SlingBehaviour : MonoBehaviour
{
    [SerializeField] private int _aimingLinePointCount = 30; // 조준선 점 개수 (해상도)
    [SerializeField] private float _aimingLineLength = 5f; // 조준선 총 길이 (호 길이, 고정 / 월드 단위)
    [SerializeField] private float _minAimingDist = 0.5f; // 발사로 인정할 최소 당김 거리 (이하는 조준 무시)
    [SerializeField] private float _maxAimingDist = 4f; // 세기가 최대가 되는 당김 거리 (이 이상은 상한 고정)
    [SerializeField] private float _minShotSpeed = 8f; // 최소 당김(_minAimingDist)일 때 발사 속도
    [SerializeField] private float _maxShotSpeed = 20f; // 최대 당김(_maxAimingDist)일 때 발사 속도
    [SerializeField] private float _magnetStrength = 15f;

    private LineRenderer _aimingLine;
    private Rigidbody2D _rigid;

    // Initialize에서 한 번만 계산해 캐싱하는 값들 (상태마다 변하지 않음)
    private Vector2 _flightGravity; // 비행 시 받는 중력 (Physics2D.gravity * gravityScale)
    private float _sqrMinAimingDist; // _minAimingDist 제곱 (IsValidAiming 비교용)
    private float _aimingLineStepLength; // 조준선 점 사이 호 길이

    private bool _isActiveSling;
    public bool IsActiveSling => _isActiveSling;

    public void Initialize(Rigidbody2D rigid)
    {
        _rigid = rigid;

        // 상태마다 변하지 않는 값은 여기서 한 번만 계산해 둔다.
        _flightGravity = Physics2D.gravity * _rigid.gravityScale;
        _sqrMinAimingDist = _minAimingDist * _minAimingDist;
        _aimingLineStepLength = _aimingLineLength / (_aimingLinePointCount - 1);

        _aimingLine = GetComponentInChildren<LineRenderer>();
        _aimingLine.useWorldSpace = true;
        _aimingLine.positionCount = _aimingLinePointCount; // 점 개수 고정 → 매 프레임 설정 불필요
        _aimingLine.enabled = false;
    }

    // ============ ... ============

    public void SetActiveSling(bool active)
    {
        _isActiveSling = active;
    }

    // ============ 물리 모드 전환 ============

    // 벽/바닥에 붙어 정지한 상태의 물리 (PlayerAttachedState.Enter)
    // Kinematic: 중력·외력 무시. 추후 움직이는 플랫폼에 부모로 붙어 따라갈 수 있게 함.
    public void SetAttachedPhysics()
    {
        _rigid.linearVelocity = Vector2.zero;
        _rigid.bodyType = RigidbodyType2D.Kinematic;
    }

    // 중력을 받아 날아가는 상태의 물리 (PlayerFlyingState.Enter)
    // Kinematic→Dynamic 복귀가 먼저여야 이후 Shoot()의 velocity가 적용된다.
    public void SetFlyingPhysics()
    {
        _rigid.bodyType = RigidbodyType2D.Dynamic;
    }

    // ============ ... ============

    // 드래그 세기 0~1: _minAimingDist ~ _maxAimingDist 범위를 정규화.
    // ComputeShotVelocity의 t와 동일한 기준이므로 카메라 zoom이 발사 세기와 일치한다.
    public float ComupteDragStrength(Vector2 aimPos)
    {
        var dist = Vector2.Distance(transform.position, aimPos);
        return Mathf.Clamp01(dist / _maxAimingDist);
    }

    // 드래그 → 발사 초기 속도. 예측선과 실제 발사가 동일한 값을 쓰도록 한곳에서 계산한다.
    private Vector2 ComputeShotVelocity(Vector2 pullBackPos)
    {
        var origin = (Vector2)transform.position;
        var dir = (origin - pullBackPos).normalized;
        var dist = Vector2.Distance(pullBackPos, origin);
        var t = Mathf.Clamp01(dist / _maxAimingDist); // 당김 정도를 0 ~ 1로 정규화
        var speed = Mathf.Lerp(_minShotSpeed, _maxShotSpeed, t);
        var velocity = dir * speed;

        return velocity;
    }

    public Vector2 ComputePullBackPos(Vector2 targetPos)
    {
        var origin = (Vector2)transform.position;
        return origin - (targetPos - origin).normalized * _maxAimingDist;
    }

    // ============ 조준 ============

    // 발사로 인정할 만큼 충분히 당겼는지 (플레이어로부터의 월드 거리 기준).
    // PointerInput.DragThreshold(화면 px, 탭/드래그 구분)와는 별개의 게임플레이 게이트.
    // 비교만 하므로 sqrt(Vector2.Distance) 대신 sqrMagnitude로 양변 제곱 비교(_sqrMinAimingDist 캐싱).
    public bool IsValidAiming(Vector2 pullBackPos)
    {
        var sqrDist = ((Vector2)transform.position - pullBackPos).sqrMagnitude;
        return sqrDist >= _sqrMinAimingDist;
    }

    // 실제 날아갈 포물선을, "총 길이 고정(_aimingLineLength)"으로 잘라 그린다.
    // 같은 길이 안에서 — 세게 당기면(빠름) 거의 직선, 약하게 당기면(느림) 금방 떨어지는 포물선.
    public void ShowAiming(Vector2 pullBackPos)
    {
        var origin = (Vector2)transform.position;
        var velocity = ComputeShotVelocity(pullBackPos);

        _aimingLine.SetPosition(0, origin);

        // 점들을 시간이 아니라 "호 길이(arc length)" 간격(_aimingStep)으로 배치 → 총 길이가 항상 일정.
        const float STEP_DELTA_TIME = 0.005f; // 곡선을 따라가는 미세 시간 간격

        var prevPos = origin;
        var sumStep = 0f; // _aimingStep 경계까지 쌓이는 호 길이
        var stepTime = 0f;
        var placedPointCount = 1;

        while (placedPointCount < _aimingLinePointCount)
        {
            stepTime += STEP_DELTA_TIME;

            // 포물선 공식: p(t) = p0 + v0·t + ½·g·t²
            var currPos = origin + velocity * stepTime + 0.5f * _flightGravity * (stepTime * stepTime);
            sumStep += Vector2.Distance(prevPos, currPos); // 직전 점과의 거리를 누적해 호 길이를 측정
            prevPos = currPos;

            // 호 길이가 _aimingStep을 넘을 때마다 점을 찍는다 (한 번에 여러 개 넘을 수도 있음)
            while (sumStep >= _aimingLineStepLength && placedPointCount < _aimingLinePointCount)
            {
                _aimingLine.SetPosition(placedPointCount, currPos);
                sumStep -= _aimingLineStepLength; // 점을 찍은 만큼 소진, 나머지는 다음 점 간격으로 이월
                placedPointCount++;
            }

            if (stepTime > 100f) break; // 안전장치 (정상적으론 그 전에 길이 도달)
        }

        _aimingLine.enabled = true;
    }

    public void HideAiming()
    {
        _aimingLine.enabled = false;
    }

    // ============ 위치 이동 ============

    // 목표 위치 방향으로 일정 속도로 이동. 부착 후 중심점으로 자연스럽게 끌려가는 후처리.
    public void Magnet(Vector2 target)
    {
        _rigid.position = Vector2.MoveTowards(_rigid.position, target, _magnetStrength * Time.fixedDeltaTime);
    }

    // ============ 발사 ============

    // 비행 물리(SetFlyingPhysics)로 Dynamic 전환된 뒤에 호출해야 velocity가 적용된다.
    public void Shoot(Vector2 pullBackPos)
    {
        _rigid.linearVelocity = ComputeShotVelocity(pullBackPos);
    }

    // AI용: 목표 위치로 최대 세기 발사. 드래그 역산 없이 방향만으로 aimPoint를 구성한다.
    public void ShootAt(Vector2 targetPos)
    {
        var pullBackPos = ComputePullBackPos(targetPos);
        
        Shoot(pullBackPos);
    }
}
