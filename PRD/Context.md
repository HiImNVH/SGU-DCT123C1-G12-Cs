\# 📘 PROJECT CONVENTION \& RULES



\## 1. Kiến trúc tổng thể



Project được chia thành 4 phần chính:



\* \*\*User App\*\* (Mobile - .NET MAUI)

\* \*\*Admin Web\*\* (Blazor Server)

\* \*\*API\*\* (ASP.NET Core)

\* \*\*Core\*\* (Shared: Models, DTOs, Enums, Constants,...)



> ⚠️ Core chỉ chứa phần dùng chung, KHÔNG chứa logic business riêng của từng module



\---



\## 2. Nguyên tắc module



\* Mỗi module phải \*\*độc lập\*\*

\* \*\*KHÔNG overlap logic\*\*

\* Không phụ thuộc chéo không cần thiết

\* Có thể:



&#x20; \* sửa

&#x20; \* nâng cấp

&#x20; \* thay thế

&#x20;   👉 mà \*\*không ảnh hưởng module khác\*\*



> Nếu một thay đổi làm ảnh hưởng module khác → thiết kế sai



\---



\## 3. Quy định đặt tên



\### 3.1 Nguyên tắc chung



\* Đặt tên phải:



&#x20; \* rõ nghĩa

&#x20; \* thống nhất

&#x20; \* không mơ hồ



\* Tránh:



&#x20; \* `data1`, `test`, `temp`, `handleStuff`

&#x20; \* viết tắt khó hiểu



\---



\### 3.2 Quy tắc file



\* Mỗi file:



&#x20; \* chỉ xử lý \*\*1 chức năng chính\*\*

&#x20; \* hoặc \*\*1 nhóm function liên quan\*\*



❌ Sai:



\* nhồi nhiều logic vào 1 file



✅ Đúng:



\* tách nhỏ, rõ trách nhiệm



\---



\### 3.3 Quy tắc folder



\* Tổ chức theo \*\*feature / domain\*\*, không phải theo kiểu lộn xộn



Ví dụ:



```

/Services

&#x20;   /POI

&#x20;       POIService.cs

&#x20;       POIValidator.cs

&#x20;   /Auth

&#x20;       AuthService.cs

```



\---



\## 4. Logging (Terminal)



\### 4.1 Nguyên tắc



\* MỌI hành động đều phải có log

\* Log dùng để:



&#x20; \* debug

&#x20; \* tracking flow

&#x20; \* detect lỗi



\---



\### 4.2 Format bắt buộc



```

\[log]   - Thong tin chung

\[info]  - Thong tin quan trong

\[warn]  - Canh bao

\[error] - Loi

```



\### 4.3 Quy định nội dung



\* Viết \*\*TIẾNG VIỆT KHÔNG DẤU\*\*

\* Format:



```

\[type] - Noi dung

```



Ví dụ:



```

\[log] - Bat dau scan QR

\[info] - Da load du lieu tu cache

\[warn] - Khong tim thay audio local

\[error] - API loi 500

```



\---



\## 5. UI Text (Giao diện)



\* Tất cả text hiển thị cho user:



&#x20; \* phải dùng \*\*TIẾNG VIỆT CÓ DẤU\*\*



Ví dụ:



```

"Không tìm thấy dữ liệu"

"Kết nối thất bại"

```



\---



\## 6. Error Handling



\* MỌI lỗi hiển thị:



&#x20; \* phải có:



&#x20;   \* `Exception`

&#x20;   \* hoặc kiểm tra logic tương đương



\* Không được:



&#x20; \* bỏ qua lỗi

&#x20; \* silent fail



\---



\## 7. Quy tắc thay đổi code



\* KHÔNG tự ý sửa module khác nếu:



&#x20; \* không liên quan trực tiếp



\* Nếu bắt buộc phải sửa:



&#x20; \* phải đảm bảo:



&#x20;   \* không phá flow cũ

&#x20;   \* không gây side effect



\---



\## 8. Nguyên tắc bắt buộc



\* Mọi hành động → có log

\* Mọi lỗi → phải được xử lý

\* Code → rõ ràng, tách biệt

\* Module → độc lập tuyệt đối



\---



\## 9. Tư duy phát triển



\* Ưu tiên:



&#x20; \* dễ đọc

&#x20; \* dễ maintain

&#x20; \* dễ mở rộng



\* Không ưu tiên:



&#x20; \* code nhanh

&#x20; \* hack tạm



> Code viết hôm nay phải đủ rõ để 1 tháng sau đọc lại vẫn hiểu ngay



\---

\---



\# 📦 10. Thư viện sử dụng cho từng project



\## 10.1 User App (.NET MAUI)



\### Bắt buộc



\* `ZXing.Net.MAUI` → Scan QR

\* `Microsoft.Maui.Media` → TTS

\* `sqlite-net-pcl` → Local database (offline)

\* `CommunityToolkit.Maui` → UI + helpers

\* `Refit` hoặc `HttpClient` → Gọi API



\---



\### Khuyến nghị



\* `Newtonsoft.Json` → xử lý JSON linh hoạt

\* `Polly` → retry khi gọi API (network yếu)



\---



\### Mục tiêu



\* Scan QR nhanh

\* Load data offline-first

\* TTS hoạt động ổn định



\---



\## 10.2 Admin Web (Blazor Server)



\### Bắt buộc



\* `MudBlazor` → UI component (rất mạnh cho admin)

\* `FluentValidation` → validate input

\* `Refit` hoặc `HttpClient` → gọi API



\---



\### Khuyến nghị



\* `Serilog` → logging

\* `AutoMapper` → mapping DTO



\---



\### Mục tiêu



\* CRUD nhanh

\* UI đơn giản, dễ dùng

\* Không cần đẹp, chỉ cần rõ



\---



\## 10.3 API (ASP.NET Core)



\### Bắt buộc



\* `Microsoft.EntityFrameworkCore`

\* `Microsoft.EntityFrameworkCore.SqlServer`

\* `Microsoft.EntityFrameworkCore.Tools`

\* `Swashbuckle.AspNetCore` → Swagger

\* `Microsoft.AspNetCore.Authentication.JwtBearer` → Auth



\---



\### Khuyến nghị



\* `Serilog.AspNetCore` → logging

\* `FluentValidation.AspNetCore` → validate request

\* `AutoMapper` → mapping

\* `MediatR` (optional) → nếu muốn clean architecture hơn



\---



\### QR Code



\* `QRCoder` → generate QR



\---



\### Mục tiêu



\* API rõ ràng

\* Response nhanh (<5s theo PRD )

\* Không over-engineer



\---



\## 10.4 Core (Shared)



\### Chỉ chứa



\* Models (Entity)

\* DTOs

\* Enums

\* Constants



\---



\### KHÔNG được chứa



\* Business logic

\* Service

\* Database access



\---



\# 🧭 11. Hướng đi dự án (POC cho sinh viên)



\## 11.1 Mục tiêu thực tế



Đây là \*\*POC\*\*, không phải production system → cần:



\* Làm \*\*đúng flow\*\*

\* Chạy \*\*ổn định\*\*

\* Demo được



KHÔNG cần:



\* scale lớn

\* tối ưu cực hạn

\* microservices



\---



\## 11.2 Thứ tự triển khai (RẤT QUAN TRỌNG)



\### Phase 1 – Core Flow (Quan trọng nhất)



1\. Tạo API:



&#x20;  \* GET POI

&#x20;  \* GET POI Detail



2\. User App:



&#x20;  \* Hardcode QR → gọi API

&#x20;  \* Hiển thị data



👉 Nếu chưa xong bước này → KHÔNG làm tiếp



\---



\### Phase 2 – QR + TTS



\* Scan QR thật

\* TTS:



&#x20; \* ưu tiên text → audio sau



\---



\### Phase 3 – Offline Cache



\* Lưu SQLite

\* Load từ local khi offline



\---



\### Phase 4 – Admin



\* CRUD POI

\* Upload content

\* Generate QR



\---



\### Phase 5 – Polish



\* UI

\* Error handling

\* Logging đầy đủ



\---



\## 11.3 Nguyên tắc sống còn



\* Làm từng bước, chạy được rồi mới mở rộng

\* Không code trước khi có flow rõ ràng

\* Không tối ưu sớm



\---



\## 11.4 Những thứ KHÔNG nên làm (trong POC)



❌ Microservices

❌ Clean Architecture quá phức tạp

❌ CQRS full

❌ Event-driven system

❌ Docker + Kubernetes (trừ khi bắt buộc)



\---



\## 11.5 Những thứ NÊN làm



✅ Code rõ ràng

✅ Log đầy đủ

✅ Demo mượt

✅ Offline chạy được

✅ Flow đúng theo PRD



\---



\## 11.6 Definition of Done (POC đạt yêu cầu)



\* Scan QR → ra đúng POI

\* Có thể:



&#x20; \* xem thông tin

&#x20; \* nghe TTS

\* Offline vẫn dùng được (có cache)

\* Admin tạo được QR



\---



\## 11.7 Tư duy quan trọng



> POC tốt = đơn giản + chạy ổn định + đúng vấn đề



KHÔNG phải:



> nhiều công nghệ + code phức tạp



\---



\# 🚀 Kết luận



\* Kiến trúc: rõ ràng, tách module

\* Code: clean, dễ đọc

\* Flow: ưu tiên hoàn thành trước

\* Mục tiêu: demo được, không phải production



\---





