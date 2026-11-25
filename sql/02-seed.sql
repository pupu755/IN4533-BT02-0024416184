-- ============================================================================
-- Học phần: Lập trình Back End (IN4533) - Bài tập 02: EduHub mini
-- Sinh viên: Bùi Trung Tín - MSSV: 0024416184
-- File: sql/02-seed.sql
-- Mục đích: Khởi tạo dữ liệu mẫu tối thiểu (>= 4 thành viên, >= 3 khoá học, >= 6 phiếu đăng ký)
-- Đảm bảo: có đủ 3 trạng thái (pending, confirmed, cancelled) và có ít nhất 1 thành viên chưa đăng ký
-- ============================================================================

-- Xoá dữ liệu cũ và reset sequence
TRUNCATE TABLE registrations, courses, members RESTART IDENTITY CASCADE;

-- 1. Thêm thành viên (5 thành viên, trong đó thành viên id 5 chưa đăng ký khoá học nào)
INSERT INTO members (full_name, email) VALUES
('Bùi Trung Tín', 'trungtin@eduhub.vn'),
('Nguyễn Văn An', 'an.nguyen@eduhub.vn'),
('Trần Thị Bích', 'bich.tran@eduhub.vn'),
('Lê Hoàng Nam', 'nam.le@eduhub.vn'),
('Phạm Thuỳ Dung', 'dung.pham@eduhub.vn'); -- Thành viên chưa có phiếu đăng ký nào

-- 2. Thêm khoá học (4 khoá học, trong đó khoá id 4 chưa có đăng ký nào để kiểm thử LEFT JOIN)
INSERT INTO courses (title, capacity) VALUES
('Lập trình Back End với .NET & PostgreSQL', 30),
('Lập trình Front End với React & TypeScript', 25),
('Nhập môn Trí tuệ Nhân tạo & Machine Learning', 20),
('Thiết kế Giao diện Trải nghiệm Người dùng UI/UX', 15); -- Khoá học chưa có lượt đăng ký

-- 3. Thêm 7 phiếu đăng ký (có đủ ba trạng thái: pending, confirmed, cancelled)
INSERT INTO registrations (member_id, course_id, status) VALUES
(1, 1, 'confirmed'),  -- Bùi Trung Tín đăng ký Back End (confirmed)
(1, 2, 'pending'),    -- Bùi Trung Tín đăng ký Front End (pending)
(2, 1, 'confirmed'),  -- Nguyễn Văn An đăng ký Back End (confirmed)
(2, 3, 'cancelled'),  -- Nguyễn Văn An đăng ký AI (cancelled)
(3, 1, 'pending'),    -- Trần Thị Bích đăng ký Back End (pending)
(3, 2, 'confirmed'),  -- Trần Thị Bích đăng ký Front End (confirmed)
(4, 3, 'confirmed');  -- Lê Hoàng Nam đăng ký AI (confirmed)
