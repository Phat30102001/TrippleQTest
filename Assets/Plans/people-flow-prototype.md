# Project Overview

- **Game Title:** People Flow (prototype clone — TrippleQ Dev Test)
- **High-Level Concept:** Puzzle casual quản lý dòng chảy: người chơi tap/giữ các cột minion để đẩy từng hàng minion lên băng truyền loop; minion trôi vòng và biến mất khi gặp gate cùng màu. Dọn sạch mọi gate trước khi hết giờ mà không làm tràn băng → Win.
- **Players:** Single player
- **Inspiration / Reference Games:** People Flow / People Loop (Antlerex) — thể loại crowd/flow color-matching puzzle.
- **Tone / Art Direction:** Casual, hình khối low-poly, dùng nguyên bộ model trong `Assets/Game/3DModel`. Không cần art polish.
- **Target Platform:** Mobile (Android / iOS)
- **Screen Orientation / Resolution:** Portrait (1080x1920) — bố cục theo screenshot Level 180.
- **Render Pipeline:** URP (Mobile_RPAsset / PC_RPAsset đã có sẵn).

## Engine & Constraints
- Unity 6000.3.13f1, New Input System (1.19.0), URP 17.3.0.
- **Cần cài thêm:** `com.unity.splines` (Package Manager → Unity Registry → "Splines").
- Không cần: art hoàn chỉnh, sound/VFX, ads/IAP.

---

# Game Mechanics

## Core Gameplay Loop
1. Khu **START** chứa nhiều **cột minion** nhiều màu (mỗi cột = stack các hàng minion theo thứ tự định trước).
2. Người chơi **tap 1 cột** → đẩy **1 hàng** minion từ cột đó lên **băng truyền**. **Ấn giữ** → đẩy liên tục từng hàng theo interval.
3. Băng truyền **chạy loop**, nhưng có **Capacity = số hàng tối đa**. Nếu băng đã đầy mà người chơi cố đẩy thêm → **THUA**.
4. Minion trôi trên băng theo path. Khi gặp **gate cùng màu** → phát animation **Jump**, nhảy vào (Hole) → biến mất. Minion **sai màu** → tiếp tục **chạy vòng** chờ gate đúng mở.
5. Mỗi minion đúng màu vào gate → **số trên gate giảm**. Gate về **0** → **đóng**; gate kế tiếp trong cùng **line** được **đẩy lên** (số trước đó **ẩn**, chỉ lộ khi được promote).
6. Mỗi **line** có badge số (vd "7") = **tổng số gate** còn lại của line đó (dùng `Barrier.fbx`).
7. **Gate băng đá ❄️:** có một **unlockCounter** riêng; counter này **giảm mỗi khi BẤT KỲ gate nào trong màn đóng** (không phân biệt line). Về **0** → line băng đá mở khóa, hoạt động bình thường.
8. **Timer đếm ngược:** hết giờ = **THUA NGAY** (game không tính sao/điểm).
9. **WIN:** dọn sạch toàn bộ gate (kể cả line băng đá) trước khi hết giờ và không làm tràn băng.
10. **Restart** tức thì. Thời lượng mục tiêu mỗi lượt: 20–40 giây.

## Controls and Input Methods
- New Input System, tái dùng `Assets/InputSystem_Actions.inputactions`.
- Một action `Tap/Press` (pointer/touch) → raycast xuống cột minion (`MinionColumn`).
  - **Tap (nhả nhanh):** đẩy 1 hàng.
  - **Hold (giữ):** lặp đẩy hàng theo `holdRepeatInterval` cho tới khi nhả hoặc cột rỗng.

---

# UI

Layout Portrait, mô phỏng screenshot Level 180:

```
┌─────────────────────────────┐
│ [Pause][Restart]  ⏱ 04:26   │  ← Top bar
│                    Level 180 │
├─────────────────────────────┤
│      [Gate lines + ❄️]       │  ← khu gate (TravelDoor + Hole + Barrier badge số)
│   ║      ║       ║       ║    │
│   ║  băng truyền (Spline)║    │  ← minion loop trên path
│   ║      ║       ║       ║    │
├─────────────────────────────┤
│  Capacity: ▓▓▓▓▓░░░ (5/8)    │  ← thanh hiển thị số hàng / Capacity
├─────────────────────────────┤
│   [Col][Col][Col][Col]       │  ← START: các cột minion (tap/hold)
└─────────────────────────────┘
   [ WIN panel ] / [ FAIL panel ]  ← overlay + nút Restart / Next
```

- **HUD:** Timer đếm ngược, Capacity bar (current/max rows), nút Pause + Restart, label Level.
- **Win panel:** thông báo thắng + nút Next/Restart.
- **Fail panel:** lý do thua (Hết giờ / Tràn băng) + nút Restart.
- Công nghệ UI: uGUI (đã có `com.unity.ugui`) — đơn giản, đủ cho prototype mobile. Quyết định cuối theo skill `ui` khi thực thi.

---

# Key Asset & Context

## Model mapping (Assets/Game/3DModel — đã kiểm tra)
| Model | Vai trò | Ghi chú |
|-------|---------|---------|
| `Minion.fbx` (7 SkinnedMesh) | Minion | Đổi màu qua MaterialPropertyBlock theo `ColorId` |
| `Anim_Minions_Jump.fbx` | Clip Idle / Run / Jump_01 | Nguồn animation |
| `Anim_Minions_Die.fbx` | Clip Die | Nguồn animation |
| `WaitingArea.fbx` (dài 28.9u) | Băng truyền visual | Đặt dọc theo Spline path |
| `TravelDoor.fbx` + `Hole.fbx` | Gate màu có số | TravelDoor = cổng; Hole = vùng Jump vào |
| `Factory.fbx` | Cột minion ở START | Nguồn đẩy hàng |
| `Road.fbx` (4.24u/tile) | Tile nối / nền băng | Trang trí tuyến |
| `Barrier.fbx` | Badge số line ("7") / vách ngăn | Hiển thị số gate còn lại |

## Cấu trúc data (ScriptableObject — data-driven)
- **`ColorPalette`** (1 asset chung): `List<ColorEntry{ ColorId id; Color color; Material material; }>`.
- **`LevelConfig`** (1 asset / level):
  - `float timeLimit`
  - `ConveyorData conveyor { SplineContainer pathRef / spline data; int capacityRows; float minionSpeed; float rowSpacing; }`
  - `List<StartColumn> startColumns` — mỗi column: `List<ColorId> rows` (thứ tự đẩy; **người dùng tự sắp**)
  - `List<GateLineData> gateLines` — mỗi line: `List<GateData> gates { ColorId color; int count; }`
  - `List<IceGateData> iceGates` — `{ int unlockCounter; List<GateData> gates; }`

## Script API (signatures dự kiến)
```csharp
// Data
public enum/struct ColorId { ... }                 // tập màu mở rộng tự do
[CreateAssetMenu] class ColorPalette : SO { Material GetMaterial(ColorId); Color GetColor(ColorId); }
[CreateAssetMenu] class LevelConfig : SO { float timeLimit; ConveyorData conveyor; List<StartColumn>; List<GateLineData>; List<IceGateData>; }

// Path (Splines)
class ConveyorPath { float Length; Vector3 EvaluatePosition(float distance); Vector3 EvaluateTangent(float distance); } // wrap SplineContainer, loop bằng distance % Length

// Gameplay
class MinionAgent : MonoBehaviour { ColorId Color; float Distance; void Tick(float dt); void EnterGate(Gate g); } // Run -> Jump
class Conveyor : MonoBehaviour { int CapacityRows; bool TryAddRow(IList<MinionAgent>); event Action OnOverflow; }
class MinionColumn : MonoBehaviour { void PushRow(); bool IsEmpty; }      // gắn lên Factory
class Gate : MonoBehaviour { ColorId Color; int Count; void Hit(); event Action<Gate> OnClosed; } // TravelDoor+Hole
class GateLine : MonoBehaviour { void PromoteNext(); int Remaining; }     // cập nhật Barrier badge
class IceGateController : MonoBehaviour { int UnlockCounter; void OnAnyGateClosed(); } // nghe toàn cục
class WinChecker / GameManager { enum State{Loading,Playing,Win,Fail}; void Win(); void Fail(FailReason); void Restart(); }
class TimerSystem { event Action OnTimeOut; }
static class EventBus { event ... OnGateClosed, OnOverflow, OnTimeOut, OnAllGatesCleared; }

// Input
class InputReader { event Action<Vector2> OnTapStart; event Action OnTapEnd; } // dùng InputSystem_Actions
```

## Kỹ thuật path/loop (cốt lõi)
- Mỗi `MinionAgent` giữ `Distance` (mét). Mỗi frame: `Distance += speed*dt; float u = (Distance % Length)/Length; pos = spline.EvaluatePosition(u); rot = LookRotation(spline.EvaluateTangent(u));`
- **Capacity** = `floor(Length / rowSpacing)`; khoảng cách giữa hàng = `rowSpacing` cố định → xếp hàng đều.
- `Closed Spline` = loop khép kín.

---

# Implementation Steps

### Step 1 — Context & Setup
- **Description:** Cài package `com.unity.splines`. Xác minh import settings của `Minion.fbx` (rig Generic/Humanoid), kiểm tra material/skinned mesh. Tạo cây thư mục: `Assets/Game/Scripts`, `Assets/Game/Prefabs`, `Assets/Game/Data/Levels`, `Assets/Game/Materials`.
- **Assigned role:** explorer → developer
- **Dependencies:** None
- **Parallelizable:** No (nền tảng)

### Step 2 — Data Layer (ScriptableObject)
- **Description:** Tạo `ColorId`, `ColorPalette`, `LevelConfig` + struct con (`ConveyorData`, `StartColumn`, `GateLineData`, `GateData`, `IceGateData`). Tạo asset `ColorPalette` mẫu + materials màu cho Minion.
- **Assigned role:** developer
- **Dependencies:** Step 1
- **Parallelizable:** Yes (song song Step 3)

### Step 3 — Animator cho Minion
- **Description:** Tạo `AnimatorController` (Idle/Run/Jump/Die) dùng clip từ `Anim_Minions_Jump.fbx` & `Anim_Minions_Die.fbx`, gắn vào prefab Minion. Set up đổi màu qua MaterialPropertyBlock.
- **Assigned role:** developer (skill: 2d-character/animation workflow tham khảo)
- **Dependencies:** Step 1
- **Parallelizable:** Yes (song song Step 2)

### Step 4 — Conveyor & Path (Splines)
- **Description:** `ConveyorPath` wrap SplineContainer (loop). `Conveyor` quản lý danh sách hàng, Capacity, `TryAddRow`, `OnOverflow`. `MinionAgent` di chuyển theo Distance, Run/Jump.
- **Assigned role:** developer
- **Dependencies:** Step 2, Step 3
- **Parallelizable:** No

### Step 5 — Gate System
- **Description:** `Gate` (TravelDoor+Hole, color+count, Hit/Close), `GateLine` (promote gate kế, ẩn/lộ số, cập nhật Barrier badge), `IceGateController` (nghe mọi OnGateClosed → giảm unlockCounter → unlock). Va chạm minion↔gate đúng màu.
- **Assigned role:** developer
- **Dependencies:** Step 4
- **Parallelizable:** No

### Step 6 — Input & Columns
- **Description:** `InputReader` (New Input System) tap/hold → raycast → `MinionColumn.PushRow()` (gắn Factory) → `Conveyor.TryAddRow`. Hold lặp theo interval.
- **Assigned role:** developer (skill: setup-game-inputs)
- **Dependencies:** Step 4
- **Parallelizable:** Yes (song song Step 5)

### Step 7 — Game Flow & Timer
- **Description:** `GameManager` state machine + `TimerSystem` (timeout→Fail), `WinChecker` (hết gate→Win), `EventBus`. Restart reload level.
- **Assigned role:** developer
- **Dependencies:** Step 5, Step 6
- **Parallelizable:** No

### Step 8 — UI / HUD
- **Description:** Timer, Capacity bar, Pause/Restart, label Level, Win/Fail panel (lý do thua). Bind vào EventBus.
- **Assigned role:** developer (skill: ui)
- **Dependencies:** Step 7
- **Parallelizable:** Yes (song song Step 9 phần dựng scene)

### Step 9 — LevelLoader & Scene
- **Description:** `LevelLoader` đọc `LevelConfig` → spawn START columns, conveyor visual (WaitingArea theo spline), gate lines, ice gates. Dựng scene chơi được. Tạo 1 `LevelConfig` mẫu.
- **Assigned role:** developer (skill: unity-scene-creator)
- **Dependencies:** Step 7
- **Parallelizable:** No

### Step 10 — Level Editor Tool
- **Description:** Custom Editor / EditorWindow cho `LevelConfig`: chỉnh path (spline), sắp xếp startColumns rows trực quan, thêm gate lines/ice gates, nút "Create New Level". Giúp build 100 level nhanh.
- **Assigned role:** developer
- **Dependencies:** Step 9
- **Parallelizable:** No

### Step 11 — Polish & Multi-level
- **Description:** Tạo 1–3 level mẫu qua editor tool để chứng minh mở rộng. Tinh chỉnh speed/capacity/timeLimit cho 20–40s/lượt. Cleanup.
- **Assigned role:** developer
- **Dependencies:** Step 10
- **Parallelizable:** No

---

# Verification & Testing

## Manual checks (play mode)
- Tap cột → đúng 1 hàng minion lên băng; Hold → đẩy liên tục.
- Minion trôi mượt theo mọi hình dạng spline (cong/loop), tự xoay theo hướng đường.
- Minion sai màu chạy vòng tiếp; đúng màu → Jump vào gate → biến mất, số gate giảm.
- Gate về 0 → đóng, gate kế lộ số; badge line giảm đúng.
- Ice gate: counter giảm mỗi lần BẤT KỲ gate đóng; về 0 → unlock.
- Băng đầy + cố đẩy thêm → Fail "Tràn băng".
- Hết giờ còn gate → Fail "Hết giờ".
- Dọn hết gate trước giờ → Win.
- Restart reset sạch trạng thái.

## Edge cases
- Cột rỗng khi tap → không lỗi.
- Capacity = số hàng chính xác (floor(Length/rowSpacing)); đẩy đúng ngưỡng tràn.
- Minion đang Jump không bị tính sai vào Capacity.
- Ice gate unlock đúng kể cả khi nhiều gate đóng cùng frame.
- Đổi số màu trong ColorPalette không vỡ logic.

## Data-driven verification
- Tạo level mới bằng editor tool, không sửa code → chơi được → chứng minh khả năng mở rộng 100 level.

## Phần II (trả lời trong README — dựa trên kiến trúc này)
1. Tổ chức code: tách Data (SO) / Core systems / Gameplay components / UI, nối lỏng qua EventBus.
2. 100 level: nhân `LevelConfig` asset + Level Editor tool; không đụng code.
3. Fail nhiều: chỉnh data trước (timeLimit, capacityRows, độ lệch màu, count gate).
4. Retention: cơ chế Capacity băng truyền (rủi ro tràn → căng thẳng "đẩy hay chờ") + restart nhanh.
