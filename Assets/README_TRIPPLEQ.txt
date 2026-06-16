Casual puzzle clone từ People Flow:
Rule: người chơi tap/giữ các cột minion để đẩy từng hàng lên băng truyền; minion trôi vòng và biến mất khi gặp gate cùng màu, dọn sạch mọi gate trước khi hết giờ mà không làm tràn băng → Lose.


> Engine: **Unity 6000.3.13f1** · URP (Mobile) · **New Input System** · package **com.unity.splines**

## Cách chơi


- Tap 1 cột (Factory) → đẩy 1 hàng minion lên băng truyền.
- Ân giữ cột → đẩy liên tục từng hàng.
- Minion chỉ biến mất khi gặp gate cùng màu (nhảy vào hố). Sai màu → tiếp tục chạy vòng.
- Gate về 0 → đóng, gate kế trong line được đẩy lên (số ẩn cho tới khi được kích hoạt).
- Băng truyền có giới hạn số hàng (Capacity). Đẩy thêm khi đã đầy → THUA (tràn băng).
- Hết giờ → THUA.


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
   └── UI/           # HudController, UI
```


Tầng tách bạch, nối lỏng qua `EventBus`:
- Data (SO): toàn bộ level là data — không hard-code.
- Core: vòng đời game (state machine), timer, win-check, nạp level, input.
- Gameplay: từng component đơn nhiệm (conveyor, gate, minion...).
- UI: chỉ là view, bind sự kiện.


Đường băng tùy hình dạng: mỗi minion chỉ giữ một biến `Distance` và lấy vị trí/hướng từ `ConveyorPath` (bọc một closed Spline). `Distance % Length` lo phần loop; chênh lệch `Distance`cố định giữa các hàng lo việc xếp hàng đều và tính Capacity (`Length / rowSpacing`). Nhờ vậy **một script `MinionAgent` chạy đúng trên mọi hình dạng băng chỉ bằng cách đổi spline trong từng level. Capture của gate dùng swept point-segment (chống tunneling khi băng chạy nhanh).


---


## Trả lời Phần II


### 1. Bạn tổ chức code theo cách nào? Vì sao?
=> Chia 4 tầng (Data / Core / Gameplay / UI) và nối lỏng bằng một EventBus tĩnh. Mỗi MonoBehaviour đơn nhiệm (Conveyor lo băng, Gate lo cổng, MinionAgent lo di chuyển...). Toàn bộ nội dung level nằm trong ScriptableObject (`LevelConfig`, `ColorPalette`). Lý do: thể loại này cốt lõi là content nhiều, logic ổn định — tách data khỏi logic cho phép nhân level và cân bằng độ khó mà không sửa code; EventBus giúp UI/hệ thống phụ không phụ thuộc cứng vào nhau, dễ thêm/bớt 


### 2. Nếu cần làm 100 level, bạn mở rộng thế nào?
=> Mỗi level = một `LevelConfig` asset, nên 100 level = 100 asset, không đụng code.
Mở rộng tiếp: import level từ CSV/JSON cho người thiết kế, gom `LevelConfig` vào một `LevelDatabase`. Các prefab map có thể dùng Adressable để giảm dung lượng game





### 3. Nếu người chơi fail quá nhiều, bạn chỉnh ở đâu trước?
=> Chỉnh data. theo thứ tự: 
(1) tăng `capacityRows` (giảm áp lực tràn băng — nguyên nhân thua phổ biến nhất)
(2) nới `timeLimit`
(3) giảm độ lệch giữa thứ tự màu ở `startColumns` và thứ tự gate
mở (để minion đúng màu gặp gate sớm hơn).
Nếu vẫn cao, mới xét bổ sung cơ chế trợ giúp (item dọn sẵn 1 goal gate).


### 4. Mechanic nào ảnh hưởng retention nhiều nhất? Vì sao?
=> Capacity của băng truyền (rủi ro tràn → thua). Vì mỗi lần tap là user phải suy nghĩ cần chọn hàng nào trước để đẩy


