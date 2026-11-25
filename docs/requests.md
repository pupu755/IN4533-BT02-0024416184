# DANH SÁCH LỆNH CURL KIỂM THỬ RESTFUL API (Phần 3.1)

- **Học phần**: Lập trình Back End (IN4533)
- **Sinh viên**: Bùi Trung Tín - MSSV: 0024416184
- **Dự án**: EduHub mini API
- **Địa chỉ API Server**: `http://localhost:5000`

---

## 1. Lệnh curl cho 4 chức năng chính

### Lệnh 1: Xem danh sách đăng ký của một khoá học
- **Chức năng**: Lấy danh sách thành viên đã đăng ký khoá học ID = 1
- **Phương thức**: `GET`
- **Mã trạng thái HTTP dự kiến**: `200 OK`

```bash
curl -i -X GET http://localhost:5000/api/v1/courses/1/registrations \
  -H "Accept: application/json"
```

---

### Lệnh 2: Tạo phiếu đăng ký mới cho thành viên vào khoá học
- **Chức năng**: Đăng ký thành viên ID = 5 vào khoá học ID = 4
- **Phương thức**: `POST`
- **Mã trạng thái HTTP dự kiến**: `201 Created`

```bash
curl -i -X POST http://localhost:5000/api/v1/courses/4/registrations \
  -H "Content-Type: application/json" \
  -d '{"memberId": 5}'
```

*(Trường hợp thử nghiệm trùng lặp - kiểm tra mã lỗi 409 Conflict):*
- **Mã trạng thái HTTP dự kiến**: `409 Conflict` (theo chuẩn RFC 9457)
```bash
curl -i -X POST http://localhost:5000/api/v1/courses/1/registrations \
  -H "Content-Type: application/json" \
  -d '{"memberId": 1}'
```

---

### Lệnh 3: Xác nhận đăng ký khoá học (chuyển trạng thái sang confirmed)
- **Chức năng**: Cập nhật trạng thái phiếu đăng ký ID = 2 sang `confirmed`
- **Phương thức**: `PUT`
- **Mã trạng thái HTTP dự kiến**: `200 OK`

```bash
curl -i -X PUT http://localhost:5000/api/v1/registrations/2/status \
  -H "Content-Type: application/json" \
  -d '{"status": "confirmed"}'
```

---

### Lệnh 4: Huỷ đăng ký khoá học
- **Chức năng**: Huỷ phiếu đăng ký ID = 2 (chuyển trạng thái sang cancelled)
- **Phương thức**: `DELETE`
- **Mã trạng thái HTTP dự kiến**: `200 OK`

```bash
curl -i -X DELETE http://localhost:5000/api/v1/registrations/2 \
  -H "Accept: application/json"
```

---

## 2. Bảng tổng hợp các lệnh kiểm thử và mã phản hồi dự kiến

| STT | Endpoint | Method | Payload | Mã HTTP dự kiến | Mô tả |
|:---:|---|:---:|---|:---:|---|
| 1 | `/api/v1/courses/1/registrations` | `GET` | *(None)* | `200 OK` | Lấy danh sách thành công |
| 2a | `/api/v1/courses/4/registrations` | `POST` | `{"memberId": 5}` | `201 Created` | Tạo mới đăng ký thành công |
| 2b | `/api/v1/courses/1/registrations` | `POST` | `{"memberId": 1}` | `409 Conflict` | Trùng đăng ký (Problem Details RFC 9457) |
| 3 | `/api/v1/registrations/2/status` | `PUT` | `{"status": "confirmed"}` | `200 OK` | Xác nhận đăng ký thành công |
| 4 | `/api/v1/registrations/2` | `DELETE` | *(None)* | `200 OK` | Huỷ đăng ký thành công |
