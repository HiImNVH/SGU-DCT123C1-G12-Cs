# Làm việc nhóm và phạm vi đóng góp

Project được thực hiện bởi 2 thành viên:

| Thành viên | MSSV | Vai trò | Phạm vi phụ trách |
| --- | --- | --- | --- |
| Nguyễn Vũ Huy | 3123411123 | Thiết kế và Frontend | Thiết kế giao diện, xây dựng các sơ đồ trong PRD và phát triển giao diện Mobile/Admin Web |
| Phạm Nguyên Phát | 3123411221 | Backend và nội dung PRD | Phát triển API, xử lý dữ liệu phía Backend và xây dựng phần nội dung, thông tin trong PRD |

Không suy luận quyền sở hữu module chỉ từ số lượng commit. Hai thành viên cần thống nhất bảng trên dựa trên công việc thực tế và cập nhật khi phạm vi phụ trách thay đổi.

## Luồng kỹ thuật cần nắm

```text
Mobile quét QR/deep link
  → lấy POI id
  → gọi GET /api/poi/{id}?lang={language}
  → POIController
  → POIService (chọn ngôn ngữ, fallback vi)
  → POIRepository / Entity Framework Core
  → SQL Server
  → POIDetailDto
  → mobile hiển thị chi tiết và phát thuyết minh
```

Khi thay đổi luồng này, thành viên phụ trách cần cập nhật tài liệu liên quan, mô tả dữ liệu vào/ra và ghi rõ cách xử lý lỗi.

## Quy ước commit mới

Dùng commit ngắn, có mục đích rõ ràng:

```text
feat(api): add role-based authorization for POI management
fix(mobile): handle QR scan navigation error
docs: add local setup instructions
test(auth): add login service tests
```

Không cần viết lại toàn bộ lịch sử cũ chỉ để đổi message. Ưu tiên lịch sử rõ ràng từ thời điểm này.
