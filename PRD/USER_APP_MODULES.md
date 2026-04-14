🎯 Mục tiêu

Scan QR

Hiển thị POI

TTS

Offline

🧩 Module \& nhiệm vụ

1\. QRScannerModule



File gợi ý:

QRScannerService.cs



Nhiệm vụ:



Scan QR

Trả về poiId

Handle lỗi:

QR invalid



Log:



\[log] - Bat dau scan QR

\[error] - QR khong hop le

2\. POIDataModule



File gợi ý:

POIDataService.cs



Nhiệm vụ:



Gọi API lấy POI

Mapping DTO → Model

Check cache trước khi gọi API

Fallback khi offline

3\. CacheModule



File gợi ý:

CacheService.cs



Nhiệm vụ:



Lưu SQLite:

POI

Content

Audio

Get data từ local

Clear/update cache

4\. TTSModule



File gợi ý:

TTSPlayerService.cs



Nhiệm vụ:



Play audio theo thứ tự:

Local audio

Audio URL

TTS text

Control:

Play / Pause / Stop

State management

5\. UI Flow (không cần file riêng, nhưng phải rõ)



Flow:



Scan QR → lấy poiId

→ Check cache

→ Nếu có: load ngay

→ Nếu không: gọi API

→ Hiển thị UI

→ Auto play TTS

