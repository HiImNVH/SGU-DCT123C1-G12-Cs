\# 📁 PROJECT FOLDER STRUCTURE



\## 🧭 Tổng quan



Project gồm 4 phần chính:



```

/TravelGuideApp

&#x20;   /UserApp

&#x20;   /AdminWeb

&#x20;   /API

&#x20;   /Core

```



\---



\# 📱 1. USER APP (MAUI)



```

/UserApp

&#x20;   /Views

&#x20;   /ViewModels

&#x20;   /Services

&#x20;   /Repositories

&#x20;   /Models

&#x20;   /Helpers

&#x20;   /Constants

&#x20;   App.xaml

&#x20;   MauiProgram.cs

```



\## 📂 Views



\* Chứa UI (XAML)

\* Ví dụ:



&#x20; \* `HomePage.xaml`

&#x20; \* `ScanPage.xaml`

&#x20; \* `POIDetailPage.xaml`



👉 Chỉ hiển thị, không chứa logic



\---



\## 📂 ViewModels



\* Xử lý logic cho UI (MVVM)

\* Binding dữ liệu



👉 Ví dụ:



\* `ScanViewModel.cs`

\* `POIDetailViewModel.cs`



\---



\## 📂 Services



\* Logic chính của app



👉 Bao gồm:



\* `QRScannerService.cs` → scan QR

\* `POIDataService.cs` → gọi API + xử lý data

\* `TTSPlayerService.cs` → phát audio

\* `CacheService.cs` → SQLite



\---



\## 📂 Repositories



\* Làm việc trực tiếp với SQLite



👉 Ví dụ:



\* `LocalCacheRepository.cs`



\---



\## 📂 Models



\* Model dùng riêng cho app (nếu cần)



\---



\## 📂 Helpers



\* Utility functions



👉 Ví dụ:



\* `NetworkHelper.cs`

\* `LogHelper.cs`



\---



\## 📂 Constants



\* Biến cố định



👉 Ví dụ:



\* API URL

\* Key config



\---



\# 🌐 2. ADMIN WEB (BLAZOR)



```

/AdminWeb

&#x20;   /Pages

&#x20;   /Services

&#x20;   /Models

&#x20;   /Components

&#x20;   /Helpers

&#x20;   Program.cs

```



\## 📂 Pages



\* UI chính



👉 Ví dụ:



\* `POIList.razor`

\* `POIEdit.razor`



\---



\## 📂 Services



\* Gọi API



👉 Ví dụ:



\* `POIManagementService.cs`

\* `QRManagementService.cs`



\---



\## 📂 Models



\* Model hiển thị UI



\---



\## 📂 Components



\* UI component tái sử dụng



\---



\## 📂 Helpers



\* Utility (format, validate nhẹ)



\---



\# 🔌 3. API (ASP.NET CORE)



```

/API

&#x20;   /Controllers

&#x20;   /Services

&#x20;   /Repositories

&#x20;   /Entities

&#x20;   /DTOs

&#x20;   /Middleware

&#x20;   /Configurations

&#x20;   Program.cs

```



\## 📂 Controllers



\* Entry point của API



👉 Ví dụ:



\* `POIController.cs`

\* `AuthController.cs`



\---



\## 📂 Services



\* Business logic



👉 Ví dụ:



\* `POIService.cs`

\* `QRService.cs`

\* `AuthService.cs`



\---



\## 📂 Repositories



\* Làm việc với database



👉 Ví dụ:



\* `POIRepository.cs`

\* `UserRepository.cs`



\---



\## 📂 Entities



\* Model database (EF Core)



👉 Ví dụ:



\* `POI.cs`

\* `POIContent.cs`



\---



\## 📂 DTOs



\* Data transfer object



👉 Ví dụ:



\* `POIDetailDto.cs`

\* `CreatePOIDto.cs`



\---



\## 📂 Middleware



\* Xử lý global



👉 Ví dụ:



\* Error handling

\* Logging



\---



\## 📂 Configurations



\* Config EF, JWT, Swagger



\---



\# 🧩 4. CORE (SHARED)



```

/Core

&#x20;   /Models

&#x20;   /DTOs

&#x20;   /Enums

&#x20;   /Constants

```



\---



\## 📂 Models



\* Shared entities



\---



\## 📂 DTOs



\* Dùng chung giữa API + Client



\---



\## 📂 Enums



\* Ví dụ:



&#x20; \* `UserRole`

&#x20; \* `PlayerState`



\---



\## 📂 Constants



\* Giá trị dùng chung toàn hệ thống



\---



\# 🔗 QUAN HỆ GIỮA CÁC PHẦN



```

UserApp  --> API <--- AdminWeb

&#x20;       \\          /

&#x20;        \\        /

&#x20;           Core

```



\---



\# 🚨 NGUYÊN TẮC QUAN TRỌNG



\* UserApp KHÔNG truy cập DB trực tiếp

\* AdminWeb KHÔNG xử lý business logic

\* API là nơi xử lý logic chính

\* Core chỉ chứa shared (KHÔNG logic)



\---



\# 🚀 TÓM TẮT



| Layer    | Vai trò chính               |

| -------- | --------------------------- |

| UserApp  | UI + trải nghiệm người dùng |

| AdminWeb | Quản lý dữ liệu             |

| API      | Xử lý logic + dữ liệu       |

| Core     | Dùng chung                  |



\---



