-- ============================================================================
-- Học phần: Lập trình Back End (IN4533) - Bài tập 02: EduHub mini
-- Sinh viên: Bùi Trung Tín - MSSV: 0024416184
-- File: sql/03-queries.sql
-- Mục đích: Thực thi 3 truy vấn theo yêu cầu bài tập
-- ============================================================================

-- ----------------------------------------------------------------------------
-- Truy vấn (a): Họ tên thành viên và tên khoá học của các phiếu confirmed, sắp xếp theo họ tên;
-- ----------------------------------------------------------------------------
SELECT 
    m.full_name AS ho_ten_thanh_vien, 
    c.title AS ten_khoa_hoc,
    r.status AS trang_thai
FROM registrations r
INNER JOIN members m ON r.member_id = m.id
INNER JOIN courses c ON r.course_id = c.id
WHERE r.status = 'confirmed'
ORDER BY m.full_name ASC;

-- ----------------------------------------------------------------------------
-- Truy vấn (b): Số phiếu đăng ký của từng khoá học, kể cả khoá chưa có phiếu nào;
-- Giải thích: Vì sao dùng LEFT JOIN mà không dùng JOIN?
-- Dùng LEFT JOIN để bảo toàn tất cả các dòng từ bảng bên trái (courses), kể cả khi
-- khoá học đó chưa có bản ghi nào khớp trong bảng bên phải (registrations).
-- Khi đó r.id mang giá trị NULL và COUNT(r.id) sẽ trả về 0.
-- Nếu dùng INNER JOIN (hoặc JOIN), những khoá học chưa có lượt đăng ký nào sẽ bị
-- loại bỏ khỏi kết quả truy vấn, không đáp ứng yêu cầu "kể cả khoá chưa có phiếu nào".
-- ----------------------------------------------------------------------------
SELECT 
    c.id AS ma_khoa_hoc,
    c.title AS ten_khoa_hoc, 
    COUNT(r.id) AS so_dang_ky
FROM courses c
LEFT JOIN registrations r ON r.course_id = c.id
GROUP BY c.id, c.title
ORDER BY c.id ASC;

-- ----------------------------------------------------------------------------
-- Truy vấn (c): Các thành viên chưa đăng ký khoá nào;
-- ----------------------------------------------------------------------------
SELECT 
    m.id AS ma_thanh_vien, 
    m.full_name AS ho_ten_thanh_vien, 
    m.email
FROM members m
LEFT JOIN registrations r ON m.id = r.member_id
WHERE r.id IS NULL
ORDER BY m.id ASC;
