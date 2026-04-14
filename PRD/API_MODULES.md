🎯 Mục tiêu

Trả dữ liệu POI

Quản lý POI

Auth

🧩 Module \& nhiệm vụ

1\. POIModule



File gợi ý:



POIController.cs

POIService.cs

POIRepository.cs



Nhiệm vụ:



GET list POI

GET detail POI theo lang

Validate tồn tại

2\. POIContentModule



File gợi ý:

POIContentService.cs



Nhiệm vụ:



Load content theo ngôn ngữ

Fallback về vi

3\. QRModule



File gợi ý:

QRService.cs



Nhiệm vụ:



Generate QR từ poiId

Trả về base64

4\. AuthModule



File gợi ý:



AuthController.cs

AuthService.cs



Nhiệm vụ:



Login

Generate JWT

Trả về role + language

5\. UserPreferenceModule



File gợi ý:

UserPreferenceService.cs



Nhiệm vụ:



Update language

Lưu DB

