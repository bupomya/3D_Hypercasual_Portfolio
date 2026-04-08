# GridStacker 최대 수량 제한

> GridStacker에 `maxCount`와 `IsFull` 기능을 추가하여 보관함의 최대 수량을 제한할 수 있게 한 업데이트입니다.
> DropZone에서 보관함이 가득 찼을 때 WarningUI로 경고 메시지를 표시합니다.

---

## 변경된 파일

```
GridStacker.cs  ← maxCount, IsFull 추가, AddItem 반환값 변경
DropZone.cs     ← CanTransfer()에 IsFull 체크 + 경고 UI 추가
```

---

## GridStacker 변경 사항

> `Scripts/GridStacker.cs`

### 추가된 필드/프로퍼티

| 이름 | 타입 | 설명 |
|---|---|---|
| `maxCount` | `int` | 최대 아이템 수 (기본 0 = **무제한**) |
| `IsFull` | `bool` | `maxCount > 0 && Count >= maxCount` |

### AddItem 반환값 변경

```
변경 전: public void AddItem(GameObject item)
변경 후: public bool AddItem(GameObject item)
```

- `IsFull`이면 `false` 반환, 아이템 추가하지 않음
- 정상 추가 시 `true` 반환
- **기존 코드 호환**: 반환값을 사용하지 않는 기존 호출부는 수정 없이 동작

### maxCount 설정

| maxCount 값 | 동작 |
|---|---|
| `0` (기본값) | 무제한, `IsFull`은 항상 `false` |
| `1 이상` | 해당 수량까지만 아이템 추가 가능 |

---

## DropZone 변경 사항

> `Scripts/DropZone.cs`

### CanTransfer() 변경

```csharp
// 변경 전
return source != null && source.Count > 0 && targetStacker != null;

// 변경 후
if (source == null || source.Count == 0 || targetStacker == null) return false;

if (targetStacker.IsFull)
{
    WarningUI.Instance.Show("보관함이 가득 찼습니다! (maxCount/maxCount)");
    return false;
}

return true;
```

### 동작 흐름

```
플레이어가 DropZone에 진입
    │
    ▼
CanTransfer() 체크
    │
    ├─ source 없음 / 비어있음 → 전송 안 함
    ├─ targetStacker 가득 참 → WarningUI 표시 + 전송 안 함
    └─ 조건 충족 → 아이템 전송 시작
```

---

## 설정 가이드

1. mineralDrop으로 사용하는 GridStacker 오브젝트 선택
2. Inspector에서 `Max Count` 값 설정
   - 예: `6` → 최대 6개까지 광물 보관 가능
   - `0`으로 두면 무제한
