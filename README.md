# NHÓM 10
# 3123411123 - NGUYỄN VŨ HUY
# 3123411221 - PHẠM NGUYÊN PHÁT

Note: ứng dụng nằm ở branch Master
Mô tả dự án: Xây dựng hệ thống tự động thuyết minh thông tin du lịch (đa ngôn ngữ) khi người dùng di chuyển đến gần địa điểm (POI) hoặc quét mã QR, hoạt động tốt ngay cả khi offline (không có mạng).

PROJECT DOCUMENTATION: TRAVEL GUIDE APPLICATION

1. Tổng quan dự án (Project Overview)
Travel Guide là một ứng dụng nền tảng di động (Mobile Application) hỗ trợ người dùng cuối trong việc tra cứu, khám phá các địa điểm du lịch và di tích lịch sử. Ứng dụng tập trung vào việc tối ưu hóa trải nghiệm người dùng dựa trên dữ liệu vị trí địa lý thực tế (Geospatial data) để cung cấp các gợi ý hành trình chính xác tại Việt Nam.
- Tên dự án: Travel Guide
- Phạm vi triển khai: Thành phố Hồ Chí Minh.
- Đối tượng mục tiêu: Người dùng nội địa và khách du lịch quốc tế.

2. Kiến trúc hệ thống và Công nghệ (Tech Stack)
Để đảm bảo tính mở rộng và hiệu suất, dự án được đề xuất xây dựng trên các công nghệ sau:
- Frontend: C#/Xaml + html
- Backend: html (chưa phát triển)
- Database: MySql/MsSql (chưa phát triển)
- Maps Integration: Goong.io api

3. Luồng nghiệp vụ (Business Logic & User Flow) (đang phát triển)
3.1. Quy trình Xác thực (Authentication Flow)
a. Người dùng khởi tạo tài khoản qua Email và số điện thoại.
b. Hệ thống xác thực thông tin và cấp quyền truy cập.
c. Duy trì trạng thái đăng nhập qua Access Token.

3.2. Quy trình Khám phá (Discovery Flow)
a. Ứng dụng yêu cầu quyền truy cập vị trí (GPS).
b. Hệ thống tính toán khoảng cách từ vị trí hiện tại đến các POI (Point of Interest) trong database.
c. Hiển thị danh sách địa điểm theo thứ tự ưu tiên khoảng cách hoặc mức độ phổ biến.

4. Đặc tả tính năng (Functional Requirements)
4.1. Module Người dùng
- Đăng ký/Đăng nhập: Hỗ trợ tạo tài khoản, xác thực thông tin cơ bản.
- Quản lý Profile: Cập nhật thông tin cá nhân (Họ tên, Email, Ảnh đại diện).
- Cài đặt hệ thống: Tùy chọn ngôn ngữ (Tiếng Anh/Tiếng Việt) và cấu hình thông báo.

4.2. Module Địa điểm và Bản đồ
- Trang chủ (Dashboard): Hiển thị danh sách địa điểm dưới dạng Card View với hình ảnh độ phân giải cao và mô tả tóm tắt.
- Tìm kiếm & Bộ lọc: Cho phép truy vấn địa điểm theo từ khóa hoặc danh mục (Di tích, Giải trí, Kiến trúc).
- Bản đồ tương tác (Interactive Map): - Hiển thị các Marker địa lý.
- Tích hợp Info Window hiển thị thông tin nhanh khi người dùng tương tác với Marker.
- Chi tiết địa điểm: Cung cấp thông tin chuyên sâu gồm lịch sử, hình ảnh toàn cảnh và tọa độ chính xác.

5. Yêu cầu phi chức năng (Non-Functional Requirements)
5.1. Hiệu năng (Performance)
- Thời gian phản hồi API không quá 200ms cho các truy vấn cơ bản.
- Tối ưu hóa bộ nhớ đệm (Caching) cho hình ảnh để giảm thiểu dung lượng dữ liệu di động.

5.2. Giao diện (UI/UX)
- Thiết kế theo ngôn ngữ Material Design hoặc Human Interface Guidelines.
- Đảm bảo tính nhất quán về thuật ngữ ngôn ngữ trên toàn bộ các màn hình.

5.3. Độ tin cậy (Reliability)
- Độ chính xác của vị trí địa lý phải được đảm bảo trong sai số cho phép của GPS (10m - 20m).

6. Kế hoạch hoàn thiện và Khắc phục lỗi (Development Roadmap)
6.1. Ưu tiên cấp thiết (High Priority)
- Data Integrity: Khắc phục lỗi logic truyền dữ liệu gây hiển thị giá trị undefined trên các Pop-up bản đồ.
- Localization: Chuẩn hóa toàn bộ chuỗi ký tự (Strings) sang hệ thống đa ngôn ngữ hoàn chỉnh, tránh tình trạng trộn lẫn ngôn ngữ giữa các màn hình.

6.2. Tính năng mở rộng (Future Features)
- Navigation: Tích hợp tính năng chỉ đường thời gian thực (Turn-by-turn navigation).
- Bookmark System: Phát triển module lưu trữ các địa điểm yêu thích vào kho lưu trữ cá nhân (Favorites).
- Offline Mode: Hỗ trợ lưu trữ dữ liệu bản đồ ngoại tuyến cho các khu vực sóng yếu.
