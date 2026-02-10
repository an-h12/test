Module này cung cấp giao diện dòng lệnh để tự động hóa các thao tác trên PC Windows:
- Bật/Tắt Windows Narrator (trình đọc màn hình hỗ trợ khả năng tiếp cận)
- Gửi phím Tab để điều hướng giao diện
- Lấy thông tin phần tử UI đang được focus (mới)
MỤC LỤC:
- pc_cli.py: CLI entry point, orchestrates workflows
- pc_uia.py: UI Automation element inspection
- pc_output_narrator.py: Clipboard and Narrator speech capture
- pc_keys.py: Keyboard simulation via SendInput
- pc_element_info.py: JSON serialization of element properties

Cách dùng CLI: python pc_cli.py <action>.

YÊU CẦU HỆ THỐNG
----------------
- Python 3.12 đã cài đặt và thêm vào PATH
- Thư viện uiautomation (cho chức năng lấy thông tin element):
    pip install uiautomation
- Thư viện psutil (để kiểm tra Narrator.exe):
    pip install psutil
- Thư viện winrt (để clear clipboard history Win+V):
    pip install winrt-runtime winrt-Windows.ApplicationModel.DataTransfer


CÁCH SỬ DỤNG DÒNG LỆNH
-----------------------

1. LẤY THÔNG TIN PHẦN TỬ ĐANG FOCUS
   Lệnh:
     python pc_cli.py get_focused
   
   Mô tả:
     Lấy thông tin về phần tử UI đang được focus hiện tại sử dụng UI Automation.
   
   Đầu ra (stdout):
     JSON object với các thuộc tính:
     - Name: Tên/label của element
     - LocalizedControlType: Loại control
     - Position: { Index, Total } - vị trí trong set (optional)
     - Value: Nội dung text (Edit, Document, ComboBox)
     - ToggleState: 'On'/'Off' (CheckBox, toggle Button)
     - ExpandCollapseState: 'Collapsed'/'Expanded' (ComboBox, TreeItem)
     - IsSelected: 'selected'/'non-selected' (RadioButton, ListItem)
   
   Ví dụ output:
       {"Name": "OK", "LocalizedControlType": "button"}
       {"Name": "Search", "LocalizedControlType": "edit", "Value": "test"}
       {"Name": "Item 3", "LocalizedControlType": "list item", 
        "Position": {"Index": 3, "Total": 10}, "IsSelected": "selected"}
   
   Mã thoát:
     0 = Thành công
     1 = Lỗi (không có element hoặc lỗi kỹ thuật)


2. BẬT/TẮT NARRATOR
   Lệnh:
     python pc_cli.py narrator
   
   Mô tả:
     Gửi phím tắt Ctrl + Win + Enter để bật/tắt Narrator.


3. NHẤN PHÍM TAB VÀ QUÉT UI ELEMENTS
   Lệnh:
     python pc_cli.py tab
   
   Mô tả:
     Tự động nhấn phím Tab liên tục để duyệt qua tất cả các phần tử UI.
     Sau mỗi lần Tab, lấy thông tin element và xuất dưới dạng JSON.
     Mỗi dòng là một wrapper object: { "NarratorText": ..., "Element": {...} }
     
     Tính năng:
     - Cycle Detection: Tự động dừng khi phát hiện element lặp lại (dùng RuntimeId)
     - Auto-capture Narrator text cho mỗi element
     - Nhấn ESC để dừng thủ công
   
   Cách lấy Narrator Output:
     1. Auto-toggle: Tự động bật Narrator nếu đang tắt (log ra stderr)
     2. Capture mechanism: 
        - Gửi phím tắt: Narrator Key (CapsLock/Insert) + Ctrl + X
        - Narrator copy câu đọc cuối cùng vào clipboard
        - Script đọc text từ clipboard và khôi phục nội dung cũ
     3. Clipboard history: Tự động xóa lịch sử Win+V sau mỗi lần capture
     4. Validation: So sánh NarratorText với Name/ControlType, log mismatch ra stderr
     5. Restore: Tắt Narrator lại nếu ban đầu script đã tự bật


ĐỊNH DẠNG OUTPUT
-----------------
STDOUT: JSON data (mỗi dòng một object)
STDERR: Log messages và thống kê

Lưu ý: Chỉ parse STDOUT trong C#, STDERR dùng cho debug.


HƯỚNG DẪN TÍCH HỢP C#
----------------------
1. BẬT/TẮT NARRATOR:
    string command = "python \"module\\pc_cli.py\" narrator";
    RunCommand(command, 2000);

2. LẤY THÔNG TIN ELEMENT HIỆN TẠI:
    string command = "python \"module\\pc_cli.py\" get_focused";
    string json = RunCommand(command, 2000);
    var info = JsonConvert.DeserializeObject<ElementInfo>(json);

3. QUÉT TẤT CẢ ELEMENTS (Tab + Cycle Detection):
    var psi = new ProcessStartInfo {
        FileName = "python.exe",
        Arguments = "module\\pc_cli.py tab",
        RedirectStandardOutput = true,
        UseShellExecute = false
    };
    var process = Process.Start(psi);
    string line;
    while ((line = process.StandardOutput.ReadLine()) != null)
    {
        var wrapper = JsonConvert.DeserializeObject<TabResult>(line);
        // wrapper.NarratorText, wrapper.Element
    }



DANH SÁCH KIỂM TRA TRIỂN KHAI
------------------------------
1. Copy tất cả file .py vào thư mục module/
2. Cấu hình .csproj:
   <ItemGroup>
     <Content Include="module\*.py">
       <CopyToOutputDirectory>Always</CopyToOutputDirectory>
     </Content>
   </ItemGroup>
3. Cài đặt dependencies: pip install uiautomation psutil
4. Kiểm tra Python đã có trong PATH


KHẮC PHỤC SỰ CỐ
----------------
- "Python is not recognized": Thêm Python vào PATH
- Narrator không bật/tắt: Kiểm tra Settings > Accessibility > Narrator
- "psutil unavailable": Cài pip install psutil (hoặc bỏ qua, fallback vẫn hoạt động)
- Tab không hoạt động: Một số app bảo mật cao có thể block SendInput

