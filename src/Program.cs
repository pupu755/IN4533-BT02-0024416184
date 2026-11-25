using System.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình chuỗi kết nối PostgreSQL (ưu tiên biến môi trường hoặc fallback)
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connString) || connString.Contains("your_password_here"))
{
    var pgHost = Environment.GetEnvironmentVariable("PGHOST") ?? "localhost";
    var pgPort = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
    var pgDb = Environment.GetEnvironmentVariable("PGDATABASE") ?? "eduhub";
    var pgUser = Environment.GetEnvironmentVariable("PGUSER") ?? "postgres";
    var pgPass = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "123456";
    connString = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass}";
}

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Trang chủ giới thiệu API
app.MapGet("/", () => Results.Ok(new
{
    Project = "EduHub Mini - RESTful API & PostgreSQL",
    Student = "Bùi Trung Tín",
    MSSV = "0024416184",
    Course = "IN4533 - Lập trình Back End",
    Endpoints = new[]
    {
        "GET /api/v1/courses/{courseId}/registrations",
        "POST /api/v1/courses/{courseId}/registrations",
        "PUT /api/v1/registrations/{id}/status",
        "DELETE /api/v1/registrations/{id}"
    }
}));

// Helper mở kết nối cơ sở dữ liệu
NpgsqlConnection CreateConnection() => new NpgsqlConnection(connString);

// ----------------------------------------------------------------------------
// 1. GET /api/v1/courses/{courseId:int}/registrations
// Xem danh sách đăng ký của một khoá học
// ----------------------------------------------------------------------------
app.MapGet("/api/v1/courses/{courseId:int}/registrations", async (int courseId) =>
{
    await using var conn = CreateConnection();
    await conn.OpenAsync();

    // 1. Kiểm tra khoá học có tồn tại hay không
    await using (var checkCmd = new NpgsqlCommand("SELECT COUNT(1) FROM courses WHERE id = @courseId", conn))
    {
        checkCmd.Parameters.AddWithValue("@courseId", courseId);
        var exists = Convert.ToInt64(await checkCmd.ExecuteScalarAsync()) > 0;
        if (!exists)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Course Not Found",
                type: "https://eduhub.vn/probs/course-not-found",
                detail: $"Không tìm thấy khoá học có ID = {courseId}.",
                instance: $"/api/v1/courses/{courseId}/registrations"
            );
        }
    }

    // 2. Lấy danh sách phiếu đăng ký (sử dụng tham số an toàn @courseId)
    const string sql = @"
        SELECT 
            r.id, 
            r.member_id, 
            m.full_name, 
            m.email, 
            r.course_id, 
            r.status, 
            r.registered_at
        FROM registrations r
        INNER JOIN members m ON r.member_id = m.id
        WHERE r.course_id = @courseId
        ORDER BY r.id ASC;";

    await using var cmd = new NpgsqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@courseId", courseId);

    var list = new List<object>();
    await using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        list.Add(new
        {
            id = reader.GetInt32(0),
            memberId = reader.GetInt32(1),
            memberName = reader.GetString(2),
            memberEmail = reader.GetString(3),
            courseId = reader.GetInt32(4),
            status = reader.GetString(5),
            registeredAt = reader.GetDateTime(6)
        });
    }

    return Results.Ok(list);
});

// ----------------------------------------------------------------------------
// 2. POST /api/v1/courses/{courseId:int}/registrations
// Tạo phiếu đăng ký mới (trả về 201 Created hoặc 409 Conflict Problem Details)
// ----------------------------------------------------------------------------
app.MapPost("/api/v1/courses/{courseId:int}/registrations", async (int courseId, CreateRegistrationRequest req) =>
{
    if (req == null || req.MemberId <= 0)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid Request Body",
            type: "https://eduhub.vn/probs/validation-error",
            detail: "memberId là bắt buộc và phải là số nguyên dương lớn hơn 0.",
            instance: $"/api/v1/courses/{courseId}/registrations"
        );
    }

    await using var conn = CreateConnection();
    await conn.OpenAsync();

    // 1. Kiểm tra khoá học tồn tại
    await using (var cmdCourse = new NpgsqlCommand("SELECT COUNT(1) FROM courses WHERE id = @courseId", conn))
    {
        cmdCourse.Parameters.AddWithValue("@courseId", courseId);
        if (Convert.ToInt64(await cmdCourse.ExecuteScalarAsync()) == 0)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Course Not Found",
                type: "https://eduhub.vn/probs/course-not-found",
                detail: $"Khoá học có ID = {courseId} không tồn tại.",
                instance: $"/api/v1/courses/{courseId}/registrations"
            );
        }
    }

    // 2. Kiểm tra thành viên tồn tại
    await using (var cmdMember = new NpgsqlCommand("SELECT COUNT(1) FROM members WHERE id = @memberId", conn))
    {
        cmdMember.Parameters.AddWithValue("@memberId", req.MemberId);
        if (Convert.ToInt64(await cmdMember.ExecuteScalarAsync()) == 0)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Member Not Found",
                type: "https://eduhub.vn/probs/member-not-found",
                detail: $"Thành viên có ID = {req.MemberId} không tồn tại.",
                instance: $"/api/v1/courses/{courseId}/registrations"
            );
        }
    }

    // 3. Thực hiện thêm phiếu đăng ký (bắt lỗi vi phạm ràng buộc UNIQUE)
    const string insertSql = @"
        INSERT INTO registrations (member_id, course_id, status)
        VALUES (@memberId, @courseId, 'pending')
        RETURNING id, member_id, course_id, status, registered_at;";

    try
    {
        await using var cmdInsert = new NpgsqlCommand(insertSql, conn);
        cmdInsert.Parameters.AddWithValue("@memberId", req.MemberId);
        cmdInsert.Parameters.AddWithValue("@courseId", courseId);

        await using var reader = await cmdInsert.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var result = new
            {
                id = reader.GetInt32(0),
                memberId = reader.GetInt32(1),
                courseId = reader.GetInt32(2),
                status = reader.GetString(3),
                registeredAt = reader.GetDateTime(4)
            };

            return Results.Created($"/api/v1/courses/{courseId}/registrations/{result.id}", result);
        }

        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
    catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
    {
        // Trả về mã lỗi 409 Conflict định dạng Problem Details (RFC 9457)
        return Results.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Registration Conflict",
            type: "https://eduhub.vn/probs/duplicate-registration",
            detail: $"Thành viên với ID = {req.MemberId} đã đăng ký khoá học ID = {courseId} trước đó. Mỗi thành viên chỉ được đăng ký một khoá học một lần.",
            instance: $"/api/v1/courses/{courseId}/registrations"
        );
    }
});

// ----------------------------------------------------------------------------
// 3. PUT /api/v1/registrations/{id:int}/status
// Xác nhận phiếu đăng ký (chuyển trạng thái sang confirmed)
// ----------------------------------------------------------------------------
app.MapPut("/api/v1/registrations/{id:int}/status", async (int id, UpdateStatusRequest req) =>
{
    var status = req?.Status?.Trim().ToLower();
    if (string.IsNullOrEmpty(status) || !(status == "pending" || status == "confirmed" || status == "cancelled"))
    {
        return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid Status Value",
            type: "https://eduhub.vn/probs/validation-error",
            detail: "Giá trị status chỉ được phép là 'pending', 'confirmed' hoặc 'cancelled'.",
            instance: $"/api/v1/registrations/{id}/status"
        );
    }

    await using var conn = CreateConnection();
    await conn.OpenAsync();

    const string updateSql = "UPDATE registrations SET status = @status WHERE id = @id RETURNING id, member_id, course_id, status;";
    await using var cmd = new NpgsqlCommand(updateSql, conn);
    cmd.Parameters.AddWithValue("@status", status);
    cmd.Parameters.AddWithValue("@id", id);

    await using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return Results.Ok(new
        {
            id = reader.GetInt32(0),
            memberId = reader.GetInt32(1),
            courseId = reader.GetInt32(2),
            status = reader.GetString(3),
            message = "Cập nhật trạng thái phiếu đăng ký thành công"
        });
    }

    return Results.Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Registration Not Found",
        type: "https://eduhub.vn/probs/registration-not-found",
        detail: $"Không tìm thấy phiếu đăng ký có ID = {id}.",
        instance: $"/api/v1/registrations/{id}/status"
    );
});

// ----------------------------------------------------------------------------
// 4. DELETE /api/v1/registrations/{id:int}
// Huỷ đăng ký khoá học (chuyển sang cancelled)
// ----------------------------------------------------------------------------
app.MapDelete("/api/v1/registrations/{id:int}", async (int id) =>
{
    await using var conn = CreateConnection();
    await conn.OpenAsync();

    const string deleteSql = "UPDATE registrations SET status = 'cancelled' WHERE id = @id RETURNING id, member_id, course_id, status;";
    await using var cmd = new NpgsqlCommand(deleteSql, conn);
    cmd.Parameters.AddWithValue("@id", id);

    await using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return Results.Ok(new
        {
            id = reader.GetInt32(0),
            memberId = reader.GetInt32(1),
            courseId = reader.GetInt32(2),
            status = reader.GetString(3),
            message = "Huỷ phiếu đăng ký thành công"
        });
    }

    return Results.Problem(
        statusCode: StatusCodes.Status404NotFound,
        title: "Registration Not Found",
        type: "https://eduhub.vn/probs/registration-not-found",
        detail: $"Không tìm thấy phiếu đăng ký có ID = {id}.",
        instance: $"/api/v1/registrations/{id}"
    );
});

app.Run("http://localhost:5000");

// Request Models
public record CreateRegistrationRequest(int MemberId);
public record UpdateStatusRequest(string? Status);
