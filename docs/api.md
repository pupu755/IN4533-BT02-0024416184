# BÁO CÁO THIẾT KẾ RESTFUL API - EDUHUB MINI

- **Học phần**: Lập trình Back End (IN4533)
- **Học kỳ**: 1, Năm học: 2026 – 2027
- **Họ và tên**: Bùi Trung Tín
- **Mã số sinh viên (MSSV)**: 0024416184
- **Dự án**: EduHub mini — Cơ sở dữ liệu PostgreSQL và REST API

---

## 1. Bảng thiết kế REST API (Phần 2.1)

Thiết kế tài nguyên tuân thủ nghiêm ngặt nguyên tắc RESTful:
- Sử dụng danh từ số nhiều cho URI (`courses`, `registrations`).
- Không chứa động từ trong đường dẫn URI.
- Hành động được xác định thông qua HTTP Method (`GET`, `POST`, `PUT`/`PATCH`, `DELETE`).
- Quan hệ phân cấp cha-con (sub-resource): Các phiếu đăng ký thuộc một khoá học được định tuyến qua `/courses/{courseId}/registrations`.

| STT | Chức năng | HTTP Method | URI | Mã thành công | Các mã lỗi có thể gặp | Mô tả chi tiết |
|:---:|:---|:---:|:---|:---:|:---|:---|
| 1 | Xem danh sách đăng ký của một khoá học | `GET` | `/api/v1/courses/{courseId}/registrations` | `200 OK` | `404 Not Found`<br>`500 Internal Server Error` | Lấy danh sách tất cả các phiếu đăng ký của khoá học có mã `courseId`. |
| 2 | Tạo phiếu đăng ký khoá học | `POST` | `/api/v1/courses/{courseId}/registrations` | `201 Created` | `400 Bad Request`<br>`404 Not Found`<br>`409 Conflict`<br>`500 Internal Server Error` | Tạo mới một phiếu đăng ký cho thành viên vào khoá học `courseId`. Trạng thái mặc định là `pending`. |
| 3 | Xác nhận đăng ký khoá học | `PUT` | `/api/v1/registrations/{id}/status` | `200 OK` | `400 Bad Request`<br>`404 Not Found`<br>`409 Conflict`<br>`500 Internal Server Error` | Cập nhật trạng thái phiếu đăng ký sang `confirmed` (xác nhận đăng ký). |
| 4 | Huỷ đăng ký khoá học | `DELETE` | `/api/v1/registrations/{id}` | `200 OK` / `204 No Content` | `404 Not Found`<br>`409 Conflict`<br>`500 Internal Server Error` | Huỷ phiếu đăng ký (có thể cập nhật status = `cancelled` hoặc xoá bản ghi). |

*Ghi chú thêm về chức năng Xác nhận và Huỷ theo chuẩn RESTful:*
- Với thao tác xác nhận đăng ký: Cập nhật tài nguyên trạng thái thông qua endpoint `PUT /api/v1/registrations/{id}/status` với body `{"status": "confirmed"}` (hoặc `PATCH /api/v1/registrations/{id}`).
- Với thao tác huỷ đăng ký: Sử dụng phương thức chuẩn `DELETE /api/v1/registrations/{id}` (hoặc `PUT /api/v1/registrations/{id}/status` với body `{"status": "cancelled"}`).

---

## 2. JSON Request và Response Mẫu (Phần 2.2)

### 2.1. Chức năng: Tạo đăng ký (POST)

#### Endpoint:
```http
POST /api/v1/courses/2/registrations
Content-Type: application/json
```

#### Request Body mẫu:
```json
{
  "memberId": 3
}
```

---

#### Response thành công (201 Created):
- **Status Code**: `201 Created`
- **Headers**:
  - `Content-Type: application/json; charset=utf-8`
  - `Location: /api/v1/courses/2/registrations/7`
- **Body**:
```json
{
  "id": 7,
  "memberId": 3,
  "courseId": 2,
  "status": "pending",
  "registeredAt": "2026-11-25T18:00:00Z"
}
```

---

#### Response lỗi: Trùng đăng ký (409 Conflict)
Theo tiêu chuẩn **RFC 9457 (Problem Details for HTTP APIs)** (thay thế RFC 7807):

- **Status Code**: `409 Conflict`
- **Headers**:
  - `Content-Type: application/problem+json; charset=utf-8`
- **Body**:
```json
{
  "type": "https://eduhub.vn/probs/duplicate-registration",
  "title": "Registration Conflict",
  "status": 409,
  "detail": "Thành viên ID 3 đã đăng ký khoá học ID 2 trước đó. Mỗi thành viên chỉ được đăng ký một khoá học một lần.",
  "instance": "/api/v1/courses/2/registrations"
}
```

---

### 2.2. Các trường hợp lỗi mẫu khác theo RFC 9457

#### Trường hợp 1: Không tìm thấy khoá học (404 Not Found)
- **Status Code**: `404 Not Found`
- **Headers**: `Content-Type: application/problem+json`
- **Body**:
```json
{
  "type": "https://eduhub.vn/probs/course-not-found",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Không tìm thấy khoá học với ID = 999.",
  "instance": "/api/v1/courses/999/registrations"
}
```

#### Trường hợp 2: Dữ liệu gửi lên không hợp lệ (400 Bad Request)
- **Status Code**: `400 Bad Request`
- **Headers**: `Content-Type: application/problem+json`
- **Body**:
```json
{
  "type": "https://eduhub.vn/probs/validation-error",
  "title": "Invalid Request Body",
  "status": 400,
  "detail": "memberId là bắt buộc và phải là số nguyên dương lớn hơn 0.",
  "instance": "/api/v1/courses/2/registrations"
}
```

---

## 3. Request và Response mẫu cho các chức năng còn lại

### 3.1. Xem danh sách đăng ký của một khoá học
```http
GET /api/v1/courses/1/registrations
Accept: application/json
```
**Response (200 OK):**
```json
[
  {
    "id": 1,
    "memberId": 1,
    "memberName": "Bùi Trung Tín",
    "memberEmail": "trungtin@eduhub.vn",
    "courseId": 1,
    "status": "confirmed",
    "registeredAt": "2026-11-25T10:00:00Z"
  },
  {
    "id": 3,
    "memberId": 2,
    "memberName": "Nguyễn Văn An",
    "memberEmail": "an.nguyen@eduhub.vn",
    "courseId": 1,
    "status": "confirmed",
    "registeredAt": "2026-11-25T10:15:00Z"
  },
  {
    "id": 5,
    "memberId": 3,
    "memberName": "Trần Thị Bích",
    "memberEmail": "bich.tran@eduhub.vn",
    "courseId": 1,
    "status": "pending",
    "registeredAt": "2026-11-25T10:30:00Z"
  }
]
```

### 3.2. Xác nhận đăng ký khoá học
```http
PUT /api/v1/registrations/5/status
Content-Type: application/json

{
  "status": "confirmed"
}
```
**Response (200 OK):**
```json
{
  "id": 5,
  "memberId": 3,
  "courseId": 1,
  "status": "confirmed",
  "message": "Xác nhận đăng ký thành công"
}
```

### 3.3. Huỷ đăng ký khoá học
```http
DELETE /api/v1/registrations/5
```
**Response (200 OK):**
```json
{
  "id": 5,
  "status": "cancelled",
  "message": "Huỷ phiếu đăng ký thành công"
}
```
