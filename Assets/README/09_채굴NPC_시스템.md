# 채굴 NPC 시스템

플레이어가 비용을 지불하면 자동으로 광물을 채굴하는 NPC를 소환하는 시스템입니다.
소환된 NPC는 영구적으로 채굴을 반복하며, 채굴된 아이템은 MineralDrop(GridStacker)으로 이동합니다.

```
MiningNPCZone   ← 비용 지불 + NPC 소환 관리
NPCMiner        ← NPC 자동 채굴 로직 (BaseMiner 상속)
```

---

## MiningNPCZone.cs (PurchaseZone 상속)

`PurchaseZone`을 상속받아 **비용을 지불하고 MiningNPC를 소환**하는 존입니다.
화폐 지불 로직과 트리거 처리는 PurchaseZone이 담당하며, 이 클래스는 NPC 소환만 구현합니다.

> 상속 구조: `TransferZone → PurchaseZone → MiningNPCZone`

### Inspector 설정 필드

| 필드 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `npcPrefab` | GameObject | - | 소환할 MiningNPC 프리팹 (`Assets/Prefabs/NPC/MiningNPC`) |
| `npcCount` | int | 2 | 한 번에 소환할 NPC 수 |
| `cost` | int | 5 | NPC 소환에 필요한 총 비용 (PurchaseZone 필드) |
| `delay` | float | 1 | 진입 후 소환 시도까지 대기 시간 (TransferZone 필드) |
| `interval` | float | 0.3 | 돈 아이템 제거 간격 (TransferZone 필드) |
| `miningDamage` | int | 1 | NPC의 1회 채굴 데미지 |
| `miningRadius` | float | 5 | NPC의 광물 탐색 반경 |
| `autoMiningInterval` | float | 1 | NPC의 채굴 간격 (초) |
| `mineralDrop` | GridStacker | - | 채굴 아이템이 쌓일 GridStacker |
| `mineralItemPrefab` | GameObject | - | 채굴 시 생성할 아이템 프리팹 |

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
PurchaseZone.OnPurchaseComplete() → MiningNPCZone.OnPurchaseComplete()
    │
    ▼
SpawnNPCs() — BoxCollider 범위 내 랜덤 위치에 npcCount만큼 NPC 소환
```

### 핵심 포인트

- **PurchaseZone 상속**: 트리거 처리, 타이머, 화폐 지불 로직을 직접 구현하지 않습니다.
- **한 번 소환된 NPC는 영구적으로 유지됩니다.** `CanPurchase()`가 `spawnedNPCs == null`을 체크하여 중복 소환을 방지합니다.
- NPC 소환 위치는 BoxCollider의 XZ 범위 내 랜덤이며, **Y값은 GameObject의 transform.position.y**를 사용합니다.

---

## NPCMiner.cs (BaseMiner 상속)

자동으로 광물을 찾아 이동하고, 채굴하여 MineralDrop에 아이템을 보내는 NPC입니다.

### Inspector 설정 필드

| 필드 | 타입 | 기본값 | 설명 |
|---|---|---|---|
| `moveSpeed` | float | 3 | 이동 속도 |
| `mineDistance` | float | 1.5 | 광물에 이 거리 이내로 도착하면 채굴 시작 |

> 나머지 채굴 수치(데미지, 반경, 간격 등)는 `MiningNPCZone`이 `Setup()`을 통해 주입합니다.

### 주요 메서드

| 메서드 | 설명 |
|---|---|
| `Setup(GridStacker, GameObject, int, float, float)` | MiningNPCZone에서 호출. 드롭 위치, 프리팹, 데미지, 반경, 간격을 설정합니다. |
| `StartAutoMining()` | 자동 채굴 코루틴을 시작합니다. |
| `StopAutoMining()` | 자동 채굴을 중지하고 예약된 광물을 해제합니다. |
| `OnMiningHit()` | **Mining 애니메이션 이벤트**에서 호출. 실제 채굴 데미지를 적용합니다. |

### 자동 채굴 루프 (AutoMineRoutine)

```
[1] FindUnreservedMineral() — 다른 NPC가 예약하지 않은 가장 가까운 광물 탐색
         │
         │  광물 없음 → 0.5초 대기 후 [1]로
         ▼
[2] 광물 예약 (reservedMinerals에 등록)
         │
         ▼
[3] 광물까지 이동
         │  Animator: isWalking = true
         │  moveSpeed로 직접 Transform 이동
         │  도착(mineDistance 이내) 시 다음 단계
         ▼
[4] 채광 시작
         │  Animator: isMining = true
         │  Mining 애니메이션 재생
         │  애니메이션 이벤트 → OnMiningHit() → mineral.Mine(damage)
         │  광물 파괴 시 SpawnMineralItem() 호출
         ▼
[5] 광물 예약 해제
         │  Animator: isMining = false
         │  0.2초 대기 후 [1]로
```

### 광물 예약 시스템 (NPC 겹침 방지)

```csharp
private static readonly HashSet<Mineral> reservedMinerals = new HashSet<Mineral>();
```

- **`static HashSet`** 으로 모든 NPCMiner 인스턴스가 예약 목록을 공유합니다.
- `FindUnreservedMineral()` — `reservedMinerals`에 없는 광물만 탐색 대상으로 합니다.
- 광물 선택 시 `reservedMinerals.Add()` → 채굴 완료/이탈/파괴 시 `reservedMinerals.Remove()`

```
NPC-A: 광물1 예약 → 이동 → 채광
NPC-B: 광물1 스킵(예약됨) → 광물2 예약 → 이동 → 채광
NPC-C: 광물1,2 스킵 → 광물3 예약 → 이동 → 채광
```

### 아이템 생성 (SpawnMineralItem)

```
mineralItemPrefab 생성 (NPC 머리 위)
    → ItemTweenHelper.MoveArc()로 MineralDrop까지 포물선 이동
    → 트윈 완료 후에만 GridStacker.AddItem() 호출
```

- **트윈 완료 전에는 GridStacker에 등록되지 않습니다.** ConveyorLine이 아직 날아가는 중인 아이템을 가져가는 문제를 방지합니다.
- `pendingTweenCount`로 동시에 날아가는 아이템 수를 추적하여 목표 위치가 겹치지 않습니다.

### 애니메이터 설정

MiningNPC 프리팹의 Animator Controller에 아래 파라미터와 상태가 필요합니다.

**파라미터:**

| 파라미터 | 타입 |
|---|---|
| `isWalking` | Bool |
| `isMining` | Bool |

**상태 전이:**

```
Idle ←→ Walk     (isWalking 기준)
Idle  → Mining   (isMining == true)
Mining → Idle    (isMining == false)
```

**애니메이션 이벤트:**
- Mining 애니메이션 클립에 Animation Event 추가 → Function: `OnMiningHit`

---

## MiningManager와의 관계

NPCMiner는 MiningManager의 레벨 시스템에 **구독하지 않습니다.**
채굴 수치(데미지, 반경, 간격)는 MiningNPCZone의 Inspector에서 독립적으로 설정됩니다.

| 항목 | PlayerMiner | NPCMiner |
|---|---|---|
| 레벨 관리 | MiningManager 이벤트 구독 | MiningNPCZone에서 직접 설정 |
| 채굴 수치 | MiningLevelData에 따라 변동 | Inspector 고정값 |
| 인벤토리 | ItemStacker (머리 위 스태킹) | 없음 (바로 GridStacker로 전송) |
| 채굴 방식 | 애니메이션 이벤트 (OnMiningHit) | 애니메이션 이벤트 (OnMiningHit) |

---

## Unity 씬 설정 방법

### 1. MiningNPCZone GameObject

1. 빈 GameObject 생성, 이름: `MiningNPCZone`
2. `BoxCollider` 추가 → **Is Trigger = true**
3. `MiningNPCZone` 스크립트 추가
4. Inspector에서 설정:
   - `Npc Prefab` → `Assets/Prefabs/NPC/MiningNPC`
   - `Npc Count` → 원하는 NPC 수
   - `Cost` → 소환 비용
   - `Mining Damage` / `Mining Radius` / `Auto Mining Interval` → 채굴 수치
   - `Mineral Drop` → MineralDrop GridStacker 오브젝트
   - `Mineral Item Prefab` → 광물 아이템 프리팹

### 2. MiningNPC 프리팹

1. `NPCMiner` 스크립트 추가
2. `Animator` 컴포넌트 + Animator Controller 설정 (isWalking, isMining 파라미터)
3. Mining 애니메이션 클립에 `OnMiningHit` 이벤트 추가
4. `Mining Pos` → 채굴 감지 중심점 (보통 캐릭터 앞)
5. `Mineral Layer` → 광물 레이어 마스크 설정
