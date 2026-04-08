# Idle 힌트 UI 시스템 (IdleHintUI)

> 플레이어가 일정 시간 이동하지 않으면 조이스틱을 숨기고, 손가락 드래그 애니메이션으로 "움직이세요"라는 힌트를 표시하는 시스템입니다.

---

## 동작 흐름

```
[플레이어 정지]
    │
    ▼
idleTimer 증가 (매 프레임)
    │
    ├─ 이동 중 / 채굴 중 / MiningZone 안 → 타이머 리셋, 힌트 숨김
    └─ idleTimer >= idleThreshold (기본 3초)
        │
        ▼
    IdleHintUI.Show()
        ├─ 조이스틱 페이드 아웃 (입력은 유지)
        └─ 손가락 아이콘 드래그 애니메이션 반복
            │
            ▼
    [플레이어가 조이스틱 터치]
        │ (조이스틱의 blocksRaycasts 유지, fingerIcon.raycastTarget = false)
        ▼
    이동 감지 → IdleHintUI.Hide()
        ├─ 손가락 아이콘 페이드 아웃
        └─ 조이스틱 페이드 인
```

---

## IdleHintUI.cs (싱글턴)

> `Scripts/IdleHintUI.cs`

### 주요 필드

| 필드 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `joystickCanvasGroup` | CanvasGroup | — | 조이스틱의 CanvasGroup (페이드용) |
| `fingerIcon` | Image | — | 손가락 아이콘 Image |
| `idleThreshold` | float | 3 | 힌트 표시까지 대기 시간 (초) |
| `fadeDuration` | float | 0.3 | 페이드 인/아웃 시간 (초) |
| `dragRadius` | float | 80 | 손가락 드래그 반경 (px) |
| `cycleDuration` | float | 1.5 | 드래그 한 사이클 시간 (초) |
| `cycleDelay` | float | 0.3 | 사이클 사이 대기 시간 (초) |

### 주요 메서드

| 메서드 | 설명 |
|---|---|
| `Show()` | 조이스틱 페이드아웃 + 손가락 드래그 애니메이션 시작 |
| `Hide()` | 손가락 페이드아웃 + 조이스틱 페이드인 |
| `PlayDragAnimation()` | DOTween Sequence로 위/오른쪽/왼쪽아래 드래그 반복 |

---

## 손가락 드래그 애니메이션

```
[중앙] → [위] → [중앙] → [오른쪽] → [중앙] → [왼쪽 아래] → [중앙]
                                                                │
                                                         cycleDelay 대기
                                                                │
                                                         처음부터 반복 (무한)
```

- 각 방향 이동: `OutSine` 이징 (자연스러운 감속)
- 중앙 복귀: `InSine` 이징 (자연스러운 가속)
- `DOTween.Sequence`의 `SetLoops(-1, Restart)`로 무한 반복

---

## 입력 투과 원리

힌트가 표시된 상태에서도 플레이어가 바로 조이스틱을 조작할 수 있습니다.

| 설정 | 값 | 이유 |
|---|---|---|
| `joystickCanvasGroup.blocksRaycasts` | **유지 (true)** | 투명해져도 터치 입력을 계속 받음 |
| `fingerIcon.raycastTarget` | **false** | 손가락 아이콘이 터치를 가로채지 않음 |

---

## Idle 판정 조건 (PlayerMovement.cs)

힌트를 표시하지 않는 조건 (하나라도 해당 시 타이머 리셋):

| 조건 | 체크 방식 |
|---|---|
| 이동 중 | `moveDir.sqrMagnitude >= 0.01f` |
| 채굴 중 | `BaseMiner.IsMining == true` |
| MiningZone 안 | `BaseMiner.IsInMiningZone == true` |

`IsInMiningZone`은 `MiningZone`의 `OnTriggerEnter`/`OnTriggerExit`에서 `BaseMiner.SetInMiningZone()`으로 설정됩니다.
레벨에 관계없이 MiningZone 내부에서는 힌트가 표시되지 않습니다.

---

## 카메라 연출 시 조이스틱 숨김

`GuideArrowController`에서 카메라가 특정 존을 비출 때 조이스틱을 숨깁니다.

```
카메라 연출 시작 → HideJoystick()
    │ (joystickCanvasGroup.alpha → 0, blocksRaycasts = false)
    ▼
카메라가 플레이어로 복귀 → ShowJoystick()
    │ (joystickCanvasGroup.alpha → 1, blocksRaycasts = true)
```

적용 대상:
- `ShowLevelUpZoneRoutine()` — 첫 MoneyItem 획득 시 LevelUpZone 연출
- `ShowExpansionZoneRoutine()` — ArriveZone 가득 참 시 ExpansionZone 연출

> **참고:** 카메라 연출 시에는 `blocksRaycasts = false`로 입력도 차단합니다 (IdleHintUI와 달리 조작 불가).

---

## Unity 에디터 설정 가이드

### 1. 손가락 스프라이트 준비
- 손가락(터치) 아이콘 이미지를 Import
- Texture Type → **Sprite (2D and UI)**

### 2. 오브젝트 구조

```
Canvas
├── Native Joystick (Black)     ← CanvasGroup 컴포넌트 추가
└── IdleHintUI                  ← 빈 GameObject + IdleHintUI 스크립트
    └── FingerIcon              ← Image (손가락 스프라이트)
```

### 3. Inspector 연결

| 필드 | 연결 대상 |
|---|---|
| Joystick Canvas Group | Native Joystick (Black)의 CanvasGroup |
| Finger Icon | FingerIcon Image |

### 4. GuideArrowController Inspector 추가 연결

| 필드 | 연결 대상 |
|---|---|
| Joystick Canvas Group | Native Joystick (Black)의 CanvasGroup |
