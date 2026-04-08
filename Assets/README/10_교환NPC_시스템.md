# 교환 NPC 시스템

플레이어가 비용을 지불하면 MineralRedItem을 자동으로 운반·교환하는 NPC를 소환하는 시스템입니다.
NPC는 MineralRedPickupZone과 ExchangeZone을 왕복하며, 플레이어처럼 아이템을 등에 쌓아 운반합니다.

```
ExchangeNPCZone    ← 비용 지불 + NPC 소환 관리
ExchangeNPCWorker  ← NPC 왕복 운반 로직
```

---

## ExchangeNPCZone.cs (PurchaseZone 상속)

`PurchaseZone`을 상속받아 **비용을 지불하고 ExchangeNPC를 소환**하는 존입니다.
화폐 지불 로직과 트리거 처리는 PurchaseZone이 담당하며, 이 클래스는 NPC 소환만 구현합니다.

> 상속 구조: `TransferZone → PurchaseZone → ExchangeNPCZone`

### Inspector 설정 필드

| 필드 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `npcPrefab` | GameObject | - | 소환할 ExchangeNPC 프리팹 |
| `cost` | int | 5 | NPC 소환에 필요한 총 비용 (PurchaseZone 필드) |
| `delay` | float | 1 | 진입 후 소환 시도까지 대기 시간 (TransferZone 필드) |
| `interval` | float | 0.3 | 돈 아이템 제거 간격 (TransferZone 필드) |
| `maxCarry` | int | 5 | NPC가 한 번에 운반하는 최대 수량 |
| `pickupInterval` | float | 0.3 | 픽업 시 아이템 간 간격 (초) |
| `dropInterval` | float | 0.3 | 드롭 시 아이템 간 간격 (초) |
| `pickupPoint` | Transform | - | MineralRedPickupZone 위치 |
| `exchangePoint` | Transform | - | ExchangeZone 위치 (NPC 생성 위치이자 대기 위치) |
| `sourceStacker` | GridStacker | - | MineralRedItem이 쌓여있는 GridStacker |
| `targetStacker` | GridStacker | - | MineralRedItem을 내려놓을 GridStacker (MineralRedDrop) |

### 동작 흐름

```
플레이어 진입 (TransferZone.OnTriggerEnter)
    │
    ▼
PurchaseZone.CanTransfer() — 돈이 충분한지 체크
    │
    ▼
TransferRoutine — 돈 아이템을 interval 간격으로 하나씩 제거
    │
    ▼
PurchaseZone.OnPurchaseComplete() → ExchangeNPCZone.OnPurchaseComplete()
    │
    ▼
SpawnNPC() — exchangePoint 위치에 NPC 소환
    → ExchangeNPCWorker.Setup() + StartWorking()
```

### 핵심 포인트

- **PurchaseZone 상속**: 트리거 처리, 타이머, 화폐 지불 로직을 직접 구현하지 않습니다.
- **한 번 소환된 NPC는 영구적으로 유지됩니다.** `CanPurchase()`가 `spawnedNPC == null`을 체크하여 중복 소환을 방지합니다.
- NPC는 `exchangePoint` 위치에 생성됩니다.

---

## ExchangeNPCWorker.cs

MineralRedPickupZone과 ExchangeZone을 **왕복하며 MineralRedItem을 운반**하는 NPC입니다.

### Inspector 설정 필드

| 필드 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `moveSpeed` | float | 3 | 이동 속도 |
| `itemStacker` | ItemStacker | - | NPC의 아이템 운반용 스택커 (MineralRedItemSpawnPos) |

> 나머지 수치(maxCarry, 간격, 스택커 참조 등)는 `ExchangeNPCZone`이 `Setup()`을 통해 주입합니다.

### 주요 메서드

| 메서드 | 설명 |
|---|---|
| `Setup(...)` | ExchangeNPCZone에서 호출. 소스/타겟 스택커, 경유 지점, 운반 수치를 설정합니다. |
| `StartWorking()` | 왕복 운반 코루틴을 시작합니다. |

### 왕복 루프 (WorkRoutine)

```
[1] sourceStacker(MineralRedPickupZone)에 아이템이 있는가?
         │
         │  없음 → ExchangeZone으로 이동 → 아이템이 생길 때까지 대기
         │  있음 ↓
         ▼
[2] MineralRedPickupZone으로 이동
         │  Animator: isWalking = true
         ▼
[3] 아이템 픽업 (PickupItems)
         │  sourceStacker에서 PopItem()
         │  → MoveArc로 NPC의 ItemStacker(MineralRedItemSpawnPos)에 쌓기
         │  → maxCarry개까지 또는 소스 소진까지 반복
         │  → pickupInterval 간격으로 하나씩 픽업
         ▼
[4] ExchangeZone으로 이동
         │  Animator: isWalking = true
         ▼
[5] 아이템 드롭 (DropItems)
         │  ItemStacker에서 PopItem()
         │  → MoveArc로 targetStacker(MineralRedDrop)에 쌓기
         │  → 전부 내려놓을 때까지 반복
         │  → dropInterval 간격으로 하나씩 드롭
         │
         └→ [1]로 돌아가 반복
```

### 픽업 동작 상세 (PickupItems)

플레이어가 PickupZone에서 아이템을 줍는 것과 동일한 방식입니다.

```csharp
// sourceStacker에서 아이템을 꺼냄
GameObject item = sourceStacker.PopItem();

// NPC의 ItemStacker로 포물선 이동
ItemTweenHelper.MoveArc(item.transform, targetPos, 0.3f, 1.5f);

// ItemStacker에 추가 (NPC 등에 쌓임)
itemStacker.AddItem(item);
```

- 아이템은 NPC의 `ItemStacker` (MineralRedItemSpawnPos)에 수직으로 쌓입니다.
- `stackInterval` 간격으로 플레이어와 동일하게 머리/등 위에 쌓이는 시각 효과를 냅니다.

### 드롭 동작 상세 (DropItems)

```csharp
// NPC의 ItemStacker에서 아이템을 꺼냄
GameObject item = itemStacker.PopItem();

// targetStacker(MineralRedDrop)로 포물선 이동
ItemTweenHelper.MoveArc(item.transform, targetPos, 0.3f, 1.5f);

// GridStacker에 추가 (격자에 쌓임)
targetStacker.AddItem(item);
```

### 대기 조건

sourceStacker에 MineralRedItem이 **하나도 없으면**:
1. ExchangeZone으로 이동합니다.
2. Idle 상태로 대기합니다.
3. 0.5초 간격으로 sourceStacker를 확인합니다.
4. 아이템이 생기면 다시 왕복을 시작합니다.

### 애니메이터 설정

**파라미터:**

| 파라미터 | 타입 |
|---|---|
| `isWalking` | Bool |

**상태 전이:**

```
Idle ←→ Walk   (isWalking 기준)
```

---

## 플레이어 교환 vs NPC 교환 비교

| 항목 | 플레이어 | ExchangeNPC |
|---|---|---|
| 픽업 방식 | PickupZone 트리거 진입 | 코루틴으로 직접 PopItem → AddItem |
| 운반 | PlayerMiner의 ItemStacker | ExchangeNPCWorker의 ItemStacker |
| 드롭 방식 | DropZone 트리거 진입 | 코루틴으로 직접 PopItem → AddItem |
| 대기 조건 | 플레이어가 직접 이동 | 소스 아이템 없으면 자동 대기 |
| 최대 운반량 | MiningLevelData.maxCarryCount | ExchangeNPCZone.maxCarry |

---

## Unity 씬 설정 방법

### 1. ExchangeNPC 프리팹

1. 캐릭터 모델에 `ExchangeNPCWorker` 스크립트 추가
2. `Animator` 컴포넌트 + Animator Controller (`isWalking` Bool 파라미터)
3. 자식 오브젝트로 `MineralRedItemSpawnPos` 생성:
   - `ItemStacker` 컴포넌트 추가
   - `itemPrefab`은 비워둠 (외부에서 아이템을 받으므로)
   - `stackInterval` 설정 (아이템 사이 수직 간격)
4. ExchangeNPCWorker의 `Item Stacker` 필드에 위 오브젝트를 연결

### 2. ExchangeNPCZone GameObject

1. 빈 GameObject 생성
2. `BoxCollider` 추가 → **Is Trigger = true**
3. `ExchangeNPCZone` 스크립트 추가
4. Inspector에서 설정:
   - `Npc Prefab` → ExchangeNPC 프리팹
   - `Cost` → 소환 비용
   - `Max Carry` → NPC 최대 운반량
   - `Pickup Point` → MineralRedPickupZone 위치 (Transform)
   - `Exchange Point` → ExchangeZone 위치 (Transform)
   - `Source Stacker` → MineralRedItem이 쌓여있는 GridStacker
   - `Target Stacker` → MineralRedDrop GridStacker

### 3. 주의사항

- ExchangeNPC 프리팹의 Animator에서 **Apply Root Motion을 반드시 꺼야** 합니다. Root Motion이 켜져 있으면 코드의 `transform.position` 변경을 Animator가 덮어써서 NPC가 이동하지 못합니다.
- 모든 관련 Collider는 **Is Trigger = true** 상태여야 합니다.
