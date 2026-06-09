# Refactor Conveyor System to Linked List Style

## Project Overview
- Game Title: People Flow
- High-Level Concept: Refactor conveyor system to support multiple linked paths (non-circular) connected by gates.
- Players: Single player
- Target Platform: Android
- Render Pipeline: URP

## Game Mechanics
### Linked Conveyors
- Băng truyền không còn bắt buộc phải là hình tròn (Closed Spline).
- Hỗ trợ nhiều băng truyền trong một màn chơi.
- Các băng truyền có thể liên kết với nhau (băng truyền A kết thúc -> chuyển sang băng truyền B).
- Các cổng (Travel Gates) đóng vai trò là điểm chuyển tiếp.

## Key Asset & Context
- `Assets/Game/Scripts/Data/LevelConfig.cs`: Cập nhật cấu trúc dữ liệu để hỗ trợ danh sách băng truyền và chỉ số liên kết.
- `Assets/Game/Scripts/Core/LevelLoader.cs`: Cập nhật logic sinh băng truyền và gán đúng tham chiếu cho hàng chờ/nhà máy.
- `Assets/Game/Scripts/Gameplay/Conveyor.cs` & `ConveyorPath.cs`: Hỗ trợ đường Spline không khép kín và logic chuyển tiếp minion.
- `Assets/Editor/SplineDataHelper.cs`: Công cụ hỗ trợ copy Knot data từ Spline trong Scene vào ScriptableObject.

## Implementation Steps
1. **Refactor Data Models**: Cập nhật `LevelConfig.cs` để hỗ trợ `List<ConveyorData>` và các biến `conveyorIndex`, `nextConveyorIndex`.
   - Assigned role: developer
   - Dependencies: None
2. **Update Level Loading**: Sửa `LevelLoader.cs` để khởi tạo toàn bộ danh sách băng truyền và liên kết chúng.
   - Assigned role: developer
   - Dependencies: Step 1
3. **Conveyor Logic Refactor**: Cập nhật `Conveyor.cs` và `ConveyorPath.cs` để xử lý đường đi không lặp lại (non-looping) và sự kiện kết thúc đường đi.
   - Assigned role: developer
   - Dependencies: Step 2
4. **Minion Transition Logic**: Cập nhật `MinionAgent.cs` để khi chạm cuối băng truyền sẽ tự động "nhảy" sang băng truyền tiếp theo nếu có liên kết.
   - Assigned role: developer
   - Dependencies: Step 3
5. **Spline Knot Utility**: Tạo script Editor để copy thông số Knot giúp nhập liệu nhanh hơn.
   - Assigned role: developer
   - Dependencies: None

## Verification & Testing
- Tạo một màn chơi thử nghiệm có 2 băng truyền nối tiếp nhau.
- Kiểm tra minion từ hàng chờ đẩy vào băng 1, chạy hết băng 1 và tự động chuyển sang băng 2.
- Kiểm tra nhà máy ở băng 2 vẫn bắt được minion bình thường.
