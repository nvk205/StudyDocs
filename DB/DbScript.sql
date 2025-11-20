USE master;
GO

/* ====================================================
   1. TẠO DATABASE (NẾU CHƯA CÓ)
   ==================================================== */
IF DB_ID('StudyDocs') IS NULL
BEGIN
    CREATE DATABASE StudyDocs;
END
GO

USE StudyDocs;
GO

/* ====================================================
   2. XÓA BẢNG CŨ (ĐỂ LÀM SẠCH DỮ LIỆU TRÙNG LẶP)
   ==================================================== */
-- Xóa theo thứ tự: Bảng con trước -> Bảng cha sau
IF OBJECT_ID('dbo.Document', 'U') IS NOT NULL DROP TABLE dbo.Document;
IF OBJECT_ID('dbo.Subject', 'U') IS NOT NULL DROP TABLE dbo.Subject;
IF OBJECT_ID('dbo.[User]', 'U') IS NOT NULL DROP TABLE dbo.[User];
GO

/* ====================================================
   3. TẠO BẢNG USER (NGƯỜI DÙNG)
   ==================================================== */
CREATE TABLE dbo.[User] (
    UserId        INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Username      NVARCHAR(50)  NOT NULL,
    PasswordHash  VARBINARY(64) NOT NULL,
    PasswordSalt  VARBINARY(16) NOT NULL,
    DisplayName   NVARCHAR(100) NULL,
    Role          NVARCHAR(20)  NOT NULL DEFAULT ('User'), -- 'Admin' hoặc 'User'
    IsActive      BIT           NOT NULL DEFAULT (1),
    CreatedAt     DATETIME      NOT NULL DEFAULT (GETDATE())
);
GO
-- Tạo ràng buộc: Tên đăng nhập là duy nhất (Không cho trùng)
CREATE UNIQUE INDEX UX_User_Username ON dbo.[User](Username);
GO

/* ====================================================
   4. TẠO BẢNG SUBJECT (MÔN HỌC)
   ==================================================== */
CREATE TABLE dbo.[Subject] (
    SubjectId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Name]    NVARCHAR(200) NOT NULL
);
GO
-- Tạo ràng buộc: Tên môn học là duy nhất (Sửa lỗi hiện trùng lặp trên App)
CREATE UNIQUE INDEX UX_Subject_Name ON dbo.[Subject]([Name]);
GO

/* ====================================================
   5. TẠO BẢNG DOCUMENT (TÀI LIỆU)
   ==================================================== */
CREATE TABLE dbo.[Document] (
    DocumentId  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Title       NVARCHAR(200) NOT NULL,
    SubjectId   INT NULL,
    [Type]      NVARCHAR(20) NOT NULL, -- pdf, docx...
    
    -- Cột lưu file (QUAN TRỌNG)
    FileData    VARBINARY(MAX) NULL,
    FileName    NVARCHAR(200) NULL,
    
    Notes       NVARCHAR(MAX) NULL,
    [Status]    BIT NOT NULL DEFAULT 0,
    LastOpened  DATETIME NULL,
    CreatedAt   DATETIME NOT NULL DEFAULT (GETDATE()),
    UserId      INT NULL -- Người tạo
);
GO

-- Tạo khóa ngoại (Liên kết các bảng)
ALTER TABLE dbo.[Document] ADD CONSTRAINT FK_Document_Subject
    FOREIGN KEY (SubjectId) REFERENCES dbo.[Subject](SubjectId)
    ON DELETE SET NULL; 

ALTER TABLE dbo.[Document] ADD CONSTRAINT FK_Document_User
    FOREIGN KEY (UserId) REFERENCES dbo.[User](UserId)
    ON DELETE SET NULL;
GO

/* ====================================================
   6. DỮ LIỆU MẪU MÔN HỌC
   ==================================================== */
INSERT INTO dbo.[Subject]([Name]) VALUES 
(N'Lập trình C#'), 
(N'Cơ sở dữ liệu'), 
(N'Toán cao cấp'),
(N'Triết học Mác - Lênin');
GO

SELECT * FROM dbo.[Subject];
PRINT '--- KHOI TAO DATABASE THANH CONG ---';

USE StudyDocs;
GO

-- Tìm ông có tên là 'admin' và sửa Role thành 'Admin'
UPDATE dbo.[User]
SET Role = 'Admin'
WHERE Username = 'admin';

-- Kiểm tra lại xem đã lên Admin chưa
SELECT * FROM dbo.[User] WHERE Username = 'admin';
