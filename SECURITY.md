# Security policy

## Báo cáo lỗ hổng

Không đăng credential hoặc chi tiết khai thác nhạy cảm trong issue công khai. Liên hệ riêng với maintainer của repository và cung cấp phạm vi ảnh hưởng, cách tái hiện và đề xuất khắc phục.

## Quản lý secret

- Development: dùng .NET User Secrets hoặc biến môi trường.
- CI/CD và production: dùng secret store của nền tảng triển khai.
- Mobile: chỉ dùng Goong key đã giới hạn ứng dụng/domain/quota. Không xem key trong client là bí mật tuyệt đối.
- Không log password, JWT, API key, authorization header hoặc connection string.

Các file `appsettings.Example.json` và `.env.example` chỉ chứa placeholder, không chứa credential sử dụng thật.

## Xử lý secret đã lộ

1. Thu hồi/rotate JWT signing key và các Goong key trên hệ thống phát hành.
2. Cập nhật secret store của môi trường chạy.
3. Vô hiệu hóa session/token cũ nếu có thể.
4. Kiểm tra log và lịch sử sử dụng bất thường.
5. Nếu repository cần công khai, cân nhắc làm sạch lịch sử bằng `git filter-repo` hoặc BFG, sau đó force-push có phối hợp với toàn bộ thành viên.

Việc làm sạch lịch sử Git có tính phá vỡ clone/fork cũ nên không được tự động thực hiện trong thay đổi này. Rotate key vẫn là bước bắt buộc vì lịch sử có thể đã được sao chép.
