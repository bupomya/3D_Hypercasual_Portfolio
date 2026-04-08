# ArriveZone & WalkNPC 시스템

ExchangeManager의 WalkNPC가 MineralRedItem을 가지고 웨이포인트를 경유하여 ArriveZone에 도착하고,
사라지지 않고 서있는 시스템입니다. ArriveZone에는 최대 수용인원이 있으며 MoneyItem을 지불하여 확장할 수 있습니다.

```
ExchangeManager (수정) ← NPC 생성, 먹이기, 웨이포인트 이동, ArriveZone 등록
ArriveZone      (신규) ← NPC 수용 관리, MoneyItem으로 수용인원 업그레이드
```

---

## ExchangeManager.cs (수정사항)

기존 ExchangeManager에 **웨이포인트 경유**와 **ArriveZone 연동** 기능이 추가되었습니다.

### 추가된 Inspector 필드

| 필드 | 타입 | 설명 |
|---|---|---|
| `travelWaypoints` | Transform[] | NPC가 경유할 웨이포인트 배열 |
| `arriveZone` | ArriveZone | NPC가 최종 도착할 ArriveZone |

### 삭제된 필드

| 필드 | 이유 |
|---|---|
| `arrivePoint` | `travelWaypoints` + `arriveZone`으로 대체 |

### 변경된 NPC 라이프사이클

```
[1] ArriveZone에 공간이 있는지 확인
         │
         │  공간 없음 → 0.5초 간격으로 대기
         │  공간 있음 ↓
         ▼
[2] NPC 생성 → spawnPoint에서 waitingPoint로 이동
         │
         ▼
[3] MineralRedItem 먹이기 (FeedUntilFull)
         │  ※ 변경: 아이템을 파괴하지 않고 NPC의 ItemStacker에 쌓음
         │  → NPC가 MineralRedItem을 등에 지고 이동
         ▼
[4] 돈 아이템 생성 (SpawnMoney) — 기존과 동일
         │
         ▼
[5] 웨이포인트 경유 이동
         │  travelWaypoints[0] → [1] → [2] → ... 순서대로
         │  Animator: isWalking = true/false
         ▼
[6] ArriveZone에 등록
         │  → NPC를 ArriveZone 내 격자 위치에 배치
         │  → Animator: isWalking = false (Idle)
         │  → NPC는 파괴되지 않고 영구적으로 서있음
         │
         └→ [1]로 돌아가 반복
```

### FeedUntilFull 변경사항

NPC 프리팹에 `ItemStacker`가 있으면 아이템을 쌓고, 없으면 기존처럼 파괴합니다.

```
NPC 프리팹에 ItemStacker가 있는 경우:
    mineralRedDrop에서 PopItem()
    → MoveArc로 NPC의 ItemStacker에 쌓기
    → NPC 등에 MineralRedItem이 시각적으로 쌓임

NPC 프리팹에 ItemStacker가 없는 경우:
    mineralRedDrop에서 PopItem()
    → MoveArc로 NPC 위치로 날린 뒤 파괴 (기존 동작)
```

### MoveNPC 변경사항

```
변경 전: 위치만 이동, Animator 미사용
변경 후: Animator의 isWalking을 설정, 회전 Slerp 적용
```

### 에디터 시각화

Scene 뷰에서 **빨간색 선**으로 waitingPoint → travelWaypoints → ArriveZone 경로를 표시합니다.

---

## ArriveZone.cs

WalkNPC가 도착하여 서있는 구역입니다.
최대 수용인원을 관리하며, NPC 등록 시 이벤트를 발행합니다.

> 수용인원 확장은 `ArriveZoneExpansionZone`(PurchaseZone 상속)이 담당합니다.
> 상세 내용은 [12_구매존_시스템.md](12_구매존_시스템.md)를 참고하세요.

### Inspector 설정 필드

| 필드 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `maxCapacity` | int | 3 | 현재 최대 수용인원 |
| `npcsPerRow` | int | 3 | 한 줄에 서는 NPC 수 |
| `npcSpacing` | float | 1.5 | NPC 사이 좌우 간격 |
| `rowSpacing` | float | 1.5 | 줄 사이 앞뒤 간격 |

### 주요 프로퍼티

| 프로퍼티 | 타입 | 설명 |
|---|---|---|
| `CurrentCount` | int | 현재 도착한 NPC 수 |
| `HasSpace` | bool | 수용 여유가 있는지 (`CurrentCount < maxCapacity`) |

### 이벤트

| 이벤트 | 설명 |
|---|---|
| `OnNPCRegistered` | NPC가 등록될 때 발행. GuideArrowController가 구독하여 확장존 활성화를 판단합니다. |

### 주요 메서드

| 메서드 | 설명 |
|---|---|
| `RegisterNPC(GameObject)` | NPC를 격자 위치에 배치하고 목록에 등록합니다. Idle 상태로 설정 후 `OnNPCRegistered` 이벤트를 발행합니다. |

### NPC 배치 방식

ArriveZone의 Transform을 기준으로 격자 형태로 배치됩니다.

```
(ArriveZone 기준 로컬 방향)

       transform.right →
        ┌─────────────────┐
row 0:  [NPC1] [NPC2] [NPC3]
row 1:  [NPC4] [NPC5] [NPC6]   ← transform.forward 반대 방향
row 2:  [NPC7] [NPC8] [NPC9]
        └─────────────────┘
```

- `npcsPerRow` 개씩 한 줄, 좌우 `npcSpacing` 간격
- 줄 간 `rowSpacing` 간격, transform.forward 반대 방향으로 증가
- NPC는 ArriveZone 중심을 바라보도록 회전됩니다.

### 확장 시스템

ArriveZone의 수용인원 확장은 별도의 `ArriveZoneExpansionZone` 컴포넌트가 담당합니다.

```
ArriveZone.CurrentCount >= maxCapacity
    │
    ▼
GuideArrowController가 OnNPCRegistered 이벤트로 감지
    → ArriveZoneExpansionZone 활성화 + 카메라 연출
    │
    ▼
플레이어가 ArriveZoneExpansionZone에 진입하여 비용 지불
    → ArriveZone.maxCapacity 증가
    → 벽 파괴/확장 애니메이션 재생
```

### 에디터 시각화

- **보라색 반투명 박스** — ArriveZone 영역
- **보라색 와이어 구체** — 각 NPC 배치 위치 (maxCapacity 개수만큼 미리보기)

---

## 전체 흐름도

```
ExchangeManager
    │
    ├─ [대기] ArriveZone.HasSpace == false → 대기
    │
    ├─ [생성] NPC를 spawnPoint에 생성
    │
    ├─ [이동] waitingPoint로 이동
    │
    ├─ [먹이기] MineralRedItem을 NPC의 ItemStacker에 쌓기
    │           (requiredCount개 채워질 때까지)
    │
    ├─ [돈 생성] moneyDrop에 MoneyItem 생성
    │
    ├─ [웨이포인트 경유] travelWaypoints[0] → [1] → ... → [N]
    │                   (NPC가 MineralRedItem을 등에 지고 이동)
    │
    └─ [도착] ArriveZone.RegisterNPC(npc)
              → NPC 격자 위치에 배치
              → Idle 상태로 영구 서있음

ArriveZone
    │
    ├─ NPC 수용인원 관리 (maxCapacity)
    │
    └─ 플레이어 MoneyItem 지불 → maxCapacity 증가
```

---

## Unity 씬 설정 방법

### 1. WalkNPC 프리팹 수정

1. 자식 오브젝트로 `ItemSpawnPos` 생성 → `ItemStacker` 컴포넌트 추가
   - MineralRedItem이 쌓이는 위치 (NPC 등/머리)
   - `stackInterval` 설정
2. `Animator`에 `isWalking` Bool 파라미터 확인
3. **Apply Root Motion = false** 확인

### 2. ArriveZone GameObject

1. 빈 GameObject 생성
2. `BoxCollider` 추가 → **Is Trigger = true**
3. `ArriveZone` 스크립트 추가
4. Inspector 설정:
   - `Max Capacity` → 초기 수용인원
   - `Npcs Per Row` / `Npc Spacing` / `Row Spacing` → 배치 레이아웃
   - `Upgrade Cost` / `Upgrade Amount` → 업그레이드 설정

### 3. ExchangeManager 설정 변경

1. 기존 `Arrive Point` 필드 제거됨
2. 새 필드 설정:
   - `Travel Waypoints` → 빈 Transform들을 배열에 순서대로 추가
   - `Arrive Zone` → ArriveZone 오브젝트 연결

### 4. 웨이포인트 배치

1. 빈 GameObject들을 원하는 경로에 배치
2. ExchangeManager의 `Travel Waypoints` 배열에 순서대로 추가
3. Scene 뷰에서 빨간색 선으로 경로를 확인
