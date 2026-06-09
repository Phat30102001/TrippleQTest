# People Flow — Refactored Prototype (TrippleQ Dev Test)

A casual puzzle game where players push minions from a queue into a conveyor belt to reach color-coded goal holes. 

> **Engine:** Unity 6000.3.13f1 · URP (Mobile) · **New Input System** · Package: **com.unity.splines**

---

## Gameplay Mechanics

- **Minion Queues:** Tap/Hold columns (using `WaitingArea.fbx`) to push minion rows onto the conveyor.
- **Conveyor System:** Minions travel along a closed spline path (`Road.fbx`).
- **Goal Gates:** Minions jump into matching colored holes (`Hole.fbx`) within containers (`Factory.fbx`).
- **Barrier System:** A `Clear_Barrier` (`Barrier.fbx`) appears automatically when all holes in a goal line are filled.
- **Capacity:** Conveyor has a row limit. Overflowing the belt results in a **FAIL**.
- **Timer:** Complete all goals before the clock runs out to **WIN**.

---

## Technical Architecture

The project follows Clean Code principles and 100% English naming conventions.

### 4-Tier Architecture
1. **Data Layer (ScriptableObjects):** `LevelConfig`, `ColorPalette`. Definable levels with no code changes.
2. **Core Layer:** `GameManager` (State Machine), `EventBus` (Decoupling), `LevelLoader`, `TimerSystem`, `InputReader`.
3. **Gameplay Layer:** `Conveyor`, `MinionAgent` (Path movement), `GoalGate` (Matching logic), `GoalLine` (Sequence management).
4. **UI Layer:** `HudController` (HUD and Overlays).

---

## Level Creation (Scalability)

1. Use the **PeopleFlow > Level Creator** menu to generate a base level asset.
2. Configure the **Spline Path** and add **Minion Queues** and **Goal Lines** via the Inspector.
3. The system supports 100+ levels by simply swapping the `LevelConfig` asset in the `LevelLoader`.

---

## Part II: General Questions (Answers)

### 1. Code Organization
The code is organized into a modular, event-driven architecture using an **EventBus** for loose coupling. Tiers (Data, Core, Gameplay, UI) are separated to ensure that logic is independent of presentation and data. This allows for easier debugging and extension.

### 2. Scaling to 100 Levels
The system is entirely **Data-Driven**. Each level is a `ScriptableObject` containing spline data, queue sequences, and goal configurations. Creating new levels requires zero code changes. A **Level Creator** tool is included to automate the generation of elliptical conveyor loops.

### 3. Balancing Fail Rates
Difficulty can be tuned purely via data:
- Adjust `capacityRows` to give players more/less breathing room.
- Extend `timeLimit` for easier sessions.
- Reduce goal `count` or simplify minion color sequences in the queues.

### 4. Retention Mechanic
The **Conveyor Capacity** is the primary retention driver. It creates a "Risk vs. Reward" tension where players must decide between pushing quickly to beat the clock or waiting for the right holes to open to avoid overflow. This "flow management" loop is inherently satisfying and perfect for short mobile sessions.

Casual puzzle clone của **People Flow**: người chơi tap/giữ các cột minion để đẩy từng hàng lên
**băng truyền loop**; minion trôi vòng và biến mất khi gặp **gate cùng màu**. Dọn sạch mọi gate
trước khi hết giờ mà không làm tràn băng → **Win**.

> Engine: **Unity 6000.3.13f1** · URP (Mobile) · **New Input System** · package **com.unity.splines**

---

## Cách chơi

- **Tap 1 cột** (Factory) → đẩy **1 hàng** minion lên băng truyền.
- **Giữ** trên cột → đẩy liên tục từng hàng.
- Minion **chỉ biến mất khi gặp gate cùng màu** (nhảy vào hố). Sai màu → tiếp tục chạy vòng.
- Gate về **0** → đóng, gate kế trong line được đẩy lên (số ẩn cho tới khi được kích hoạt).
- **Gate băng đá**: có bộ đếm riêng, giảm mỗi khi **bất kỳ gate nào** trong màn đóng; về 0 → mở khóa.
- **Băng truyền có giới hạn số hàng (Capacity)**. Đẩy thêm khi đã đầy → **THUA** (tràn băng).
- **Hết giờ** → THUA.

---

## Kiến trúc & cấu trúc thư mục

```
Assets/Game/
├── 3DModel/          # FBX có sẵn (Minion, TravelDoor, Hole, Factory, Road, WaitingArea, Barrier...)
├── Data/             # ColorPalette.asset + Levels/Level_001..003.asset (ScriptableObject)
├── Materials/        # Mat_Minion_* (URP Lit) cho từng màu
├── Prefabs/          # Minion, Gate, MinionColumn + Minion.controller (Animator)
└── Scripts/
    ├── Data/         # MinionColor, ColorPalette, LevelConfig (+ struct con)
    ├── Core/         # EventBus, GameManager, TimerSystem, WinChecker, LevelLoader, InputReader
    ├── Gameplay/     # Conveyor, ConveyorPath, MinionAgent, MinionView, Gate, GateLine,
    │                 #   GateFactory, GateVisual, IceGateController, MinionColumn, Billboard
    ├── UI/           # HudController
    └── Editor/       # LevelConfigEditor, LevelCreatorWindow (PeopleFlow > Level Creator)
```

**Tầng tách bạch, nối lỏng qua `EventBus`:**
- **Data (SO):** toàn bộ level là data — không hard-code.
- **Core:** vòng đời game (state machine), timer, win-check, nạp level, input.
- **Gameplay:** từng component đơn nhiệm (conveyor, gate, minion...).
- **UI:** chỉ là view, bind sự kiện.

**Đường băng tùy hình dạng:** mỗi minion chỉ giữ một biến `Distance` và lấy vị trí/hướng từ
`ConveyorPath` (bọc một **closed Spline**). `Distance % Length` lo phần loop; chênh lệch `Distance`
cố định giữa các hàng lo việc xếp hàng đều và tính Capacity (`Length / rowSpacing`). Nhờ vậy **một
script `MinionAgent` chạy đúng trên mọi hình dạng băng** chỉ bằng cách đổi spline trong từng level.
Capture của gate dùng **swept point-segment** (chống tunneling khi băng chạy nhanh).

## Cách thêm level mới (không đụng code)

1. Menu **PeopleFlow → Level Creator** → đặt tên, chỉnh capacity/time/loop → **Create Level Asset**.
2. Chọn asset vừa tạo → inspector có **summary, nút generate path, validation**; thêm
   `startColumns` (tự sắp thứ tự minion), `gateLines`, `iceGates`.
3. Gán asset vào `LevelLoader.level` trong scene và chơi.

---

## Trả lời Phần II

### 1. Bạn tổ chức code theo cách nào? Vì sao?
Chia **4 tầng** (Data / Core / Gameplay / UI) và nối lỏng bằng một **EventBus** tĩnh. Mỗi
MonoBehaviour đơn nhiệm (Conveyor lo băng, Gate lo cổng, MinionAgent lo di chuyển...). Toàn bộ
nội dung level nằm trong **ScriptableObject** (`LevelConfig`, `ColorPalette`). Lý do: thể loại này
cốt lõi là **content nhiều, logic ổn định** — tách data khỏi logic cho phép nhân level và cân bằng
độ khó mà không sửa code; EventBus giúp UI/hệ thống phụ không phụ thuộc cứng vào nhau, dễ thêm/bớt
(vd gate băng đá chỉ cần lắng nghe `OnGateClosed`).

### 2. Nếu cần làm 100 level, bạn mở rộng thế nào?
Đã sẵn sàng: mỗi level = một `LevelConfig` asset, nên 100 level = 100 asset, **không đụng code**.
Có sẵn **Level Creator window** + custom inspector (generate path, validation cảnh báo màu không có
gate khớp). Mở rộng tiếp: import level từ CSV/JSON cho người thiết kế, gom `LevelConfig` vào một
`LevelDatabase` + màn chọn level, và thêm progression (mở khóa tuần tự). Kiến trúc spline-based cho
phép mỗi level có layout băng hoàn toàn khác nhau mà vẫn dùng chung runtime.

### 3. Nếu người chơi fail quá nhiều, bạn chỉnh ở đâu trước?
**Chỉnh data trước, không chỉnh code.** Theo thứ tự đòn bẩy: (1) tăng `capacityRows` (giảm áp lực
tràn băng — nguyên nhân thua phổ biến nhất), (2) nới `timeLimit`, (3) giảm `count` trên gate hoặc
`unlockCounter` của gate băng đá, (4) giảm độ lệch giữa thứ tự màu ở `startColumns` và thứ tự gate
mở (để minion đúng màu gặp gate sớm hơn). Tất cả đều là trường trong `LevelConfig` → A/B test nhanh.
Nếu vẫn cao, mới xét bổ sung cơ chế trợ giúp (gợi ý cột nên đẩy).

### 4. Mechanic nào ảnh hưởng retention nhiều nhất? Vì sao?
**Capacity của băng truyền** (rủi ro tràn → thua). Nó biến mỗi cú tap thành quyết định "đẩy thêm
hay chờ gate đúng mở", tạo căng thẳng và cảm giác làm chủ — đúng mạch gây nghiện của puzzle casual.
Cộng với **restart tức thì** (lượt 20–40s), người chơi thua là muốn thử lại ngay ("just one more").
Color-matching tạo độ khó nền, nhưng chính cơ chế Capacity mới tạo nhịp hồi hộp giữ chân người chơi.

---

## Trạng thái kiểm thử (Play Mode)
Đã verify trong Play Mode: spawn minion ✓, di chuyển dọc spline ✓ (vị trí vẽ đúng đường loop),
capture đúng màu ✓ (gates 24→6), gate đóng → promote gate kế ✓, **gate băng đá mở khóa sau N lần
đóng gate ✓**, không exception ✓. (Lưu ý: `Run In Background` được bật để mô phỏng chạy ổn định.)
