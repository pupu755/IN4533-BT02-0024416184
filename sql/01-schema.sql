-- ============================================================================
-- Học phần: Lập trình Back End (IN4533) - Bài tập 02: EduHub mini
-- Sinh viên: Bùi Trung Tín - MSSV: 0024416184
-- File: sql/01-schema.sql
-- Mục đích: Tạo cấu trúc các bảng cho hệ thống quản lý EduHub
-- ============================================================================

-- Xoá bảng cũ nếu tồn tại (theo thứ tự quan hệ khoá ngoại)
DROP TABLE IF EXISTS registrations CASCADE;
DROP TABLE IF EXISTS courses CASCADE;
DROP TABLE IF EXISTS members CASCADE;

-- 1. Bảng thành viên (members)
CREATE TABLE members (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE
);

-- 2. Bảng khoá học (courses)
CREATE TABLE courses (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    title VARCHAR(150) NOT NULL,
    capacity INT NOT NULL CHECK (capacity > 0)
);

-- 3. Bảng phiếu đăng ký (registrations)
-- Ràng buộc: Mỗi thành viên chỉ đăng ký một khoá học một lần (UNIQUE(member_id, course_id))
-- Ràng buộc CHECK: status chỉ nhận ba giá trị 'pending', 'confirmed', 'cancelled'
CREATE TABLE registrations (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    member_id INT NOT NULL REFERENCES members(id) ON DELETE CASCADE,
    course_id INT NOT NULL REFERENCES courses(id) ON DELETE CASCADE,
    status VARCHAR(20) NOT NULL DEFAULT 'pending' 
        CHECK (status IN ('pending', 'confirmed', 'cancelled')),
    registered_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_member_course UNIQUE (member_id, course_id)
);
