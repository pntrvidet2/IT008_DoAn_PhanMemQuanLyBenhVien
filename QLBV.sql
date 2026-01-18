/* QLBV - Unicode (NVARCHAR) script
   Fix Vietnamese display by using NVARCHAR for text fields + N'...' literals.
*/

USE master;
GO

IF DB_ID(N'QLBV') IS NOT NULL
BEGIN
    ALTER DATABASE QLBV SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QLBV;
END
GO

CREATE DATABASE QLBV;
GO

USE QLBV;
GO

---------------------------------------------------
-- DROP TABLES IF EXIST (FK order)
---------------------------------------------------
DROP TABLE IF EXISTS PRESCRIPTION_DETAILS;
DROP TABLE IF EXISTS BILL;
DROP TABLE IF EXISTS MONITORING_SHEET;
DROP TABLE IF EXISTS MEDICAL_FORM;
DROP TABLE IF EXISTS MEDICAL_RECORD;
DROP TABLE IF EXISTS PATIENT;
DROP TABLE IF EXISTS EMPLOYEE;
DROP TABLE IF EXISTS DEPARTMENT;
DROP TABLE IF EXISTS EMPLOYEE_TYPE;
DROP TABLE IF EXISTS DRUG;
DROP TABLE IF EXISTS ACCOUNT;
DROP TABLE IF EXISTS ACCOUNT_TYPE;
GO

---------------------------------------------------
-- CREATE TABLES (Unicode: NVARCHAR)
---------------------------------------------------

CREATE TABLE ACCOUNT_TYPE (
    Acc_Type_ID varchar(6) primary key,
    Acc_Type_Name nvarchar(40) not null
);

CREATE TABLE ACCOUNT (
    Acc_ID varchar(6) primary key,
    User_Name varchar(50) not null,
    Password varchar(20) not null,
    Acc_Type varchar(6) not null,
    Date_Create_Acc smalldatetime not null,
    Acc_Status nvarchar(20) not null
);

CREATE TABLE EMPLOYEE_TYPE (
    Emp_Type varchar(6) primary key,
    Employ_Name nvarchar(20) not null
);

CREATE TABLE DEPARTMENT (
    Depart_ID varchar(6) primary key,
    Depart_Name nvarchar(50) not null,
    Head_Of_Depart varchar(6)
);

CREATE TABLE EMPLOYEE (
    Emp_ID varchar(6) primary key,
    Emp_Name nvarchar(50) not null,
    Emp_Type varchar(6) not null,
    Gender varchar(6) not null,
    Day_Of_Birth smalldatetime not null,
    Start_Date smalldatetime not null,
    Phone varchar(10) not null,
    CID varchar(12) not null,
    Address nvarchar(50) not null,
    Email varchar(50) not null,
    Salary money not null,
    Depart_ID varchar(6) not null
);

CREATE TABLE PATIENT (
    Patient_ID varchar(6) primary key,
    Patient_Name nvarchar(50) not null,
    Gender varchar(6) not null,
    Day_Of_Birth smalldatetime not null,
    Phone varchar(10) not null,
    CID varchar(12) not null,
    Address nvarchar(50) not null,
    Curr_Condition nvarchar(100) not null,
    Emp_ID varchar(6)
);

CREATE TABLE MEDICAL_RECORD (
    Med_Record_ID varchar(6) primary key,
    Patient_ID varchar(6) not null,
    Employ_ID varchar(6) not null,
    Med_Date smalldatetime not null,
    Diagnosis nvarchar(100) not null,
    Theory_Plan nvarchar(100) not null,
    Health_Note nvarchar(100)
);

CREATE TABLE MEDICAL_FORM (
    Med_Form_ID varchar(6) primary key,
    Patient_ID varchar(6) not null,
    Emp_ID varchar(6) not null,
    Date smalldatetime not null,
    Symptom nvarchar(200) not null,
    Conclusion nvarchar(100) not null
);

CREATE TABLE MONITORING_SHEET (
    Moni_Sheet_ID varchar(6) primary key,
    Patient_ID varchar(6) not null,
    Emp_ID varchar(6) not null,
    Start_Date smalldatetime not null,
    End_Date smalldatetime,
    Curr_Condition nvarchar(100) not null
);

CREATE TABLE BILL (
    Bill_ID varchar(6) primary key,
    Patient_ID varchar(6) not null,
    Emp_ID varchar(6) not null,
    Date smalldatetime not null,
    Total money not null,
    Payment_Method nvarchar(20) not null,
    Payment_Status nvarchar(20) not null
);

CREATE TABLE DRUG (
    Drug_ID varchar(6) primary key,
    Drug_Name nvarchar(50) not null,
    Drug_Unit nvarchar(10) not null,
    Drug_Price money not null,
    Stock_Quantity int not null
);

CREATE TABLE PRESCRIPTION_DETAILS (
    Bill_ID varchar(6),
    Drug_ID varchar(6),
    Drug_Quantity tinyint not null,
    Amount money not null,
    PRIMARY KEY (Bill_ID, Drug_ID)
);
GO

---------------------------------------------------
-- FOREIGN KEYS + CHECKS
---------------------------------------------------
ALTER TABLE ACCOUNT
ADD CONSTRAINT FK_ACCOUNT_1 FOREIGN KEY (Acc_Type) REFERENCES ACCOUNT_TYPE(Acc_Type_ID);
GO

ALTER TABLE EMPLOYEE
ADD CONSTRAINT FK_EMPLOYEE_1 FOREIGN KEY (Emp_Type) REFERENCES EMPLOYEE_TYPE(Emp_Type),
    CONSTRAINT fk_emp_start CHECK (Start_Date > Day_Of_Birth),
    CONSTRAINT FK_EMPLOYEE_2 FOREIGN KEY (Depart_ID) REFERENCES DEPARTMENT(Depart_ID);
GO

ALTER TABLE PATIENT
ADD CONSTRAINT FK_PATIENT_1 FOREIGN KEY (Emp_ID) REFERENCES EMPLOYEE(Emp_ID);
GO

ALTER TABLE MEDICAL_RECORD
ADD CONSTRAINT FK_MR_1 FOREIGN KEY (Patient_ID) REFERENCES PATIENT(Patient_ID),
    CONSTRAINT FK_MR_2 FOREIGN KEY (Employ_ID) REFERENCES EMPLOYEE(Emp_ID);
GO

ALTER TABLE MEDICAL_FORM
ADD CONSTRAINT FK_MF_1 FOREIGN KEY (Patient_ID) REFERENCES PATIENT(Patient_ID),
    CONSTRAINT FK_MF_2 FOREIGN KEY (Emp_ID) REFERENCES EMPLOYEE(Emp_ID);
GO

ALTER TABLE MONITORING_SHEET
ADD CONSTRAINT FK_MS_1 FOREIGN KEY (Patient_ID) REFERENCES PATIENT(Patient_ID),
    CONSTRAINT FK_MS_2 FOREIGN KEY (Emp_ID) REFERENCES EMPLOYEE(Emp_ID);
GO

ALTER TABLE BILL
ADD CONSTRAINT FK_BILL_1 FOREIGN KEY (Patient_ID) REFERENCES PATIENT(Patient_ID),
    CONSTRAINT FK_BILL_2 FOREIGN KEY (Emp_ID) REFERENCES EMPLOYEE(Emp_ID);
GO

ALTER TABLE PRESCRIPTION_DETAILS
ADD CONSTRAINT FK_PD_1 FOREIGN KEY (Bill_ID) REFERENCES BILL(Bill_ID),
    CONSTRAINT FK_PD_2 FOREIGN KEY (Drug_ID) REFERENCES DRUG(Drug_ID);
GO

-- Add department head FK after EMPLOYEE exists (will work now that table exists)
ALTER TABLE DEPARTMENT
ADD CONSTRAINT FK_DEPART_1 FOREIGN KEY (Head_Of_Depart) REFERENCES EMPLOYEE(Emp_ID);
GO
ALTER TABLE ACCOUNT
ADD CONSTRAINT DF_ACCOUNT_DATE DEFAULT GETDATE() FOR Date_Create_Acc;
GO

ALTER TABLE ACCOUNT
ADD CONSTRAINT DF_ACCOUNT_STATUS DEFAULT N'Active' FOR Acc_Status;
GO


---------------------------------------------------
-- SEED DATA (use N'...' for Vietnamese)
---------------------------------------------------
INSERT INTO ACCOUNT_TYPE VALUES ( 'EM001', N'Nhân viên nhập liệu');
INSERT INTO ACCOUNT_TYPE VALUES ( 'DC002', N'Bác sĩ');
INSERT INTO ACCOUNT_TYPE VALUES ( 'PT003', N'Bệnh nhân');
INSERT INTO ACCOUNT_TYPE VALUES ( 'EC003', N'Quản lý nhân sự');
INSERT INTO ACCOUNT_TYPE VALUES ( 'AD004', N'ADMIN');
GO

INSERT INTO EMPLOYEE_TYPE VALUES ('EM001', N'Nhân viên nhập liệu');
INSERT INTO EMPLOYEE_TYPE VALUES ('DC002', N'Bác sĩ');
INSERT INTO EMPLOYEE_TYPE VALUES ('EC003', N'Quản lý nhân sự');
INSERT INTO EMPLOYEE_TYPE VALUES ('AD004', N'ADMIN');
GO

INSERT INTO DEPARTMENT VALUES ('DP001', N'Nội tổng quát', NULL);
INSERT INTO DEPARTMENT VALUES ('DP002', N'Chẩn đoán hình ảnh', NULL);
INSERT INTO DEPARTMENT VALUES ('DP003', N'Tim mạch', NULL);
INSERT INTO DEPARTMENT VALUES ('DP004', N'Tiêu hóa', NULL);
INSERT INTO DEPARTMENT VALUES ('DP005', N'Nhi', NULL);
INSERT INTO DEPARTMENT VALUES ('DP006', N'Vật lý trị liệu', NULL);
INSERT INTO DEPARTMENT VALUES ('DP007', N'ICU', NULL);
INSERT INTO DEPARTMENT VALUES ('DP008', N'Ngoại', NULL);
INSERT INTO DEPARTMENT VALUES ('DP009', N'Da liễu', NULL);
INSERT INTO DEPARTMENT VALUES ('DP010', N'Tai Mũi Họng', NULL);
GO

-- 1. Xóa dữ liệu cũ theo thứ tự (Account xóa trước vì nó tham chiếu Employee)
DELETE FROM ACCOUNT;
DELETE FROM EMPLOYEE;
GO

-- 2. Chèn dữ liệu vào bảng EMPLOYEE (Đã sửa lỗi email trùng EMP006-EMP008 và lệch EMP024-EMP025)
INSERT INTO EMPLOYEE VALUES ('EMP001', N'Hồ Thiên Ân', 'EM001', 'Male','1985-2-1','2025-9-2','0825571899','172637481920',N'HCM','anho@gmail.com','25000000','DP001');
INSERT INTO EMPLOYEE VALUES ('EMP002', N'Đoàn Gia Bảo', 'EM001', 'Male','1992-7-18','2010-3-15','0782922178','193748592032',N'HCM','giabao123@gmail.com','18000000','DP002');
INSERT INTO EMPLOYEE VALUES ('EMP003', N'Huỳnh Quốc Thiên', 'EM001', 'Male','1990-2-11','2012-9-10','0118826739','102738491020',N'HCM','quocthien123@gmail.com','18000000','DP003');
INSERT INTO EMPLOYEE VALUES ('EMP006', N'Lê Hoàng Thiên', 'EM001', 'Male','2000-11-25','2015-6-1','0082736273','283647584001',N'HCM','hoangthien123@gmail.com','26000000','DP006');
INSERT INTO EMPLOYEE VALUES ('EMP008', N'Hồ Hoàng Châu', 'EM001', 'Male','1988-11-9','2018-1-2','0263749921','839467883910',N'HCM','hoangchau123@gmail.com','24000000','DP008');
INSERT INTO EMPLOYEE VALUES ('EMP009', N'Trần Bảo Ngọc', 'EM001', 'Female','1998-9-1','2025-12-12','0263749372','927384001729',N'HCM','baongoc123@gmail.com','26000000','DP009');
INSERT INTO EMPLOYEE VALUES ('EMP010', N'Lý Tâm Minh', 'EM001', 'Male','2001-10-3','2019-12-1','0887347483','203836466718',N'HCM','tamminh123@gmail.com','19500000','DP010');
INSERT INTO EMPLOYEE VALUES ('EMP011', N'Phạm Nhật Huy', 'DC002', 'Male','1993-6-12','2016-3-10','0823456721','123456789111',N'HCM','nhathuy123@gmail.com','26000000','DP010');
INSERT INTO EMPLOYEE VALUES ('EMP012', N'Nguyễn Thị Hà', 'DC002', 'Female','1996-1-25','2020-7-2','0834567812','223456789112',N'HCM','thiha123@gmail.com','19500000','DP010');
INSERT INTO EMPLOYEE VALUES ('EMP013', N'Trương Minh Hải', 'DC002', 'Male','1987-4-18','2014-11-20','0701239845','323456789113',N'HCM','minhhai123@gmail.com','18000000','DP010');
INSERT INTO EMPLOYEE VALUES ('EMP014', N'Võ Mỹ Quyên', 'DC002', 'Female','1994-9-3','2017-2-12','0912384756','423456789114',N'HCM','myquyen123@gmail.com','24000000','DP010');
INSERT INTO EMPLOYEE VALUES ('EMP015', N'Đào Xuân Phát', 'DC002', 'Male','1991-2-27','2013-5-9','0923478561','523456789115',N'HCM','xuanphat123@gmail.com','26000000','DP001');
INSERT INTO EMPLOYEE VALUES ('EMP016', N'Nguyễn Hữu Nhân', 'EC003', 'Male','1998-11-15','2021-10-3','0937845123','623456789116',N'HCM','huunhan123@gmail.com','24000000','DP002');
INSERT INTO EMPLOYEE VALUES ('EMP017', N'Lê Trúc Mai', 'EC003', 'Female','2000-5-30','2022-1-25','0945678123','723456789117',N'HCM','trucmai123@gmail.com','19500000','DP009');
INSERT INTO EMPLOYEE VALUES ('EMP018', N'Tạ Hồng Khang', 'EC003', 'Male','1997-7-22','2019-6-14','0956781234','823456789118',N'HCM','hongkhang123@gmail.com','24000000','DP008');
INSERT INTO EMPLOYEE VALUES ('EMP019', N'Bùi Gia Khanh', 'EC003', 'Male','1989-12-29','2012-3-18','0967812345','923456789119',N'HCM','giakhanh123@gmail.com','24000000','DP009');
INSERT INTO EMPLOYEE VALUES ('EMP020', N'Võ Kim Ngân', 'EC003', 'Female','1995-3-11','2018-8-21','0978123456','103456789110',N'HCM','kimngan123@gmail.com','25000000','DP001');
INSERT INTO EMPLOYEE VALUES ('EMP021', N'Phan Khánh Ngọc', 'EC003', 'Female','1993-10-2','2016-9-15','0812349756','113456789111',N'HCM','khanhngoc123@gmail.com','17000000','DP001');
INSERT INTO EMPLOYEE VALUES ('EMP022', N'Nguyễn Tấn Lộc', 'EC003', 'Male','1988-1-18','2011-4-12','0823498756','123456789222',N'HCM','tanloc123@gmail.com','25000000','DP002');
INSERT INTO EMPLOYEE VALUES ('EMP023', N'Trần Hữu Đạt', 'EC003', 'Male','1999-9-14','2023-2-7','0834578961','133456789333',N'HCM','huudat123@gmail.com','16000000','DP003');
INSERT INTO EMPLOYEE VALUES ('EMP024', N'Hồ Bảo Vy', 'EC003', 'Female','2001-6-20','2020-12-5','0845679231','143456789444',N'HCM','baovy123@gmail.com','25000000','DP004');
INSERT INTO EMPLOYEE VALUES ('EMP025', N'Đặng Minh Tú', 'AD004', 'Male','1997-8-12','2019-5-22','0856792341','153456789555',N'HCM','minhtu@gmail.com','21000000','DP005');
INSERT INTO EMPLOYEE VALUES ('EMP026', N'Nguyễn Trần Phương Vy', 'AD004', 'Female','1997-8-12','2019-5-22','0562318690','153756789555',N'HCM','phuongvie1110@gmail.com','21000000','DP005');
GO

-- 3. Chèn dữ liệu vào bảng ACCOUNT (Đã khớp hoàn toàn 1:1 với Email bên Employee)
INSERT INTO ACCOUNT VALUES
('Acc001', 'anho@gmail.com', '0825571899', 'EM001', '2024-01-12', N'Active'),
('Acc002', 'giabao123@gmail.com', '0782922178', 'EM001', '2024-04-12', N'Active'),
('Acc003', 'quocthien123@gmail.com', '0118826739', 'EM001', '2024-02-05', N'Active'),
('Acc004', 'hoangthien123@gmail.com', '0082736273', 'EM001', '2025-06-05', N'Active'),
('Acc005', 'hoangchau123@gmail.com', '0263749921', 'EM001', '2024-04-15', N'Active'),
('Acc006', 'baongoc123@gmail.com', '0263749372', 'EM001', '2025-12-12', N'Active'),
('Acc007', 'tamminh123@gmail.com', '0887347483', 'EM001', '2019-12-01', N'Active'),
('Acc008', 'nhathuy123@gmail.com', '0823456721', 'DC002', '2016-03-10', N'Active'),
('Acc009', 'thiha123@gmail.com', '0834567812', 'DC002', '2020-07-02', N'Active'),
('Acc010', 'minhhai123@gmail.com', '0701239845', 'DC002', '2014-11-20', N'Active'),
('Acc011', 'myquyen123@gmail.com', '0912384756', 'DC002', '2017-02-12', N'Active'),
('Acc012', 'xuanphat123@gmail.com', '0923478561', 'DC002', '2013-05-09', N'Active'),
('Acc013', 'huunhan123@gmail.com', '0937845123', 'EC003', '2021-10-03', N'Active'),
('Acc014', 'trucmai123@gmail.com', '0945678123', 'EC003', '2022-01-25', N'Active'),
('Acc015', 'hongkhang123@gmail.com', '0956781234', 'EC003', '2019-06-14', N'Active'),
('Acc016', 'giakhanh123@gmail.com', '0967812345', 'EC003', '2012-03-18', N'Active'),
('Acc017', 'kimngan123@gmail.com', '0978123456', 'EC003', '2018-08-21', N'Active'),
('Acc018', 'khanhngoc123@gmail.com', '0812349756', 'EC003', '2016-09-15', N'Active'),
('Acc019', 'tanloc123@gmail.com', '0823498756', 'EC003', '2011-04-12', N'Active'),
('Acc020', 'huudat123@gmail.com', '0834578961', 'EC003', '2023-02-07', N'Active'),
('Acc021', 'baovy123@gmail.com', '0845679231', 'EC003', '2020-12-05', N'Active'),
('Acc022', 'minhtu@gmail.com', '0856792341', 'AD004', '2019-05-22', N'Active'),
('Acc026', 'phuongvie1110@gmail.com', '0562318690', 'AD004', '2024-01-02', N'Active');
GO
GO

-- Assign department heads
UPDATE DEPARTMENT SET Head_Of_Depart = 'EMP001' WHERE Depart_ID = 'DP001';
UPDATE DEPARTMENT SET Head_Of_Depart = 'EMP002' WHERE Depart_ID = 'DP002';
UPDATE DEPARTMENT SET Head_Of_Depart = 'EMP003' WHERE Depart_ID = 'DP003';
UPDATE DEPARTMENT SET Head_Of_Depart = 'EMP015' WHERE Depart_ID = 'DP005';
UPDATE DEPARTMENT SET Head_Of_Depart = 'EMP006' WHERE Depart_ID = 'DP006';
UPDATE DEPARTMENT SET Head_Of_Depart = 'EMP008' WHERE Depart_ID = 'DP008';
UPDATE DEPARTMENT SET Head_Of_Depart = 'EMP009' WHERE Depart_ID = 'DP009';
UPDATE DEPARTMENT SET Head_Of_Depart = 'EMP010' WHERE Depart_ID = 'DP010';
GO

INSERT INTO PATIENT VALUES ('PAT001', N'Nguyễn Văn Nam', 'Male','1999-3-11','0911002200','987654321',N'HCM',N'Sốt cao','EMP001');
INSERT INTO PATIENT VALUES ('PAT002', N'Trần Thị Lan', 'Female','2003-6-22','0911222333','876543219',N'HCM',N'Đau bụng','EMP002');
INSERT INTO PATIENT VALUES ('PAT003', N'Lê Quốc Huy', 'Male','1980-10-14','0911444555','765432198',N'HCM',N'Huyết áp cao','EMP003');
INSERT INTO PATIENT VALUES ('PAT004', N'Phạm Minh Hoàng', 'Male','1995-2-5','0912334455','982345671',N'HCM',N'Viêm họng','EMP006');
INSERT INTO PATIENT VALUES ('PAT005', N'Võ Thị Mỹ Duyên', 'Female','1998-8-17','0913456789','983456782',N'HCM',N'Đau đầu','EMP008');
INSERT INTO PATIENT VALUES ('PAT006', N'Ngô Văn Lợi', 'Male','1979-12-1','0914567890','984567893',N'HCM',N'Suy nhược cơ thể','EMP006');
INSERT INTO PATIENT VALUES ('PAT007', N'Nguyễn Thị Thu Hà', 'Female','1987-4-23','0915678901','985678904',N'HCM',N'Đau vai gáy','EMP009');
INSERT INTO PATIENT VALUES ('PAT008', N'Đặng Quốc Bảo', 'Male','1992-7-15','0916789012','986789015',N'HCM',N'Gãy tay','EMP008');
INSERT INTO PATIENT VALUES ('PAT009', N'Trương Ngọc Mai', 'Female','2000-9-29','0917890123','987890126',N'HCM',N'Viêm da cơ địa','EMP009');
INSERT INTO PATIENT VALUES ('PAT010', N'Lý Minh Nhựt', 'Male','1983-11-11','0918901234','988901237',N'HCM',N'Đau dạ dày','EMP010');
INSERT INTO PATIENT VALUES ('PAT011', N'Đoàn Thị Quỳnh', 'Female','1997-1-19','0919012345','989012348',N'HCM',N'Mệt mỏi','EMP011');
INSERT INTO PATIENT VALUES ('PAT012', N'Lê Hồng Phúc', 'Male','1981-3-30','0920123456','990123459',N'HCM',N'Khó thở','EMP012');
INSERT INTO PATIENT VALUES ('PAT013', N'Nguyễn Nhật Hào', 'Male','2004-6-26','0921234567','991234560',N'HCM',N'Viêm kết mạc','EMP013');
INSERT INTO PATIENT VALUES ('PAT014', N'Trần Mỹ Linh', 'Female','1990-10-8','0922345678','992345671',N'HCM',N'Viêm phổi','EMP014');
INSERT INTO PATIENT VALUES ('PAT015', N'Phạm Hoàng Sơn', 'Male','1986-5-21','0923456789','993456782',N'HCM',N'Đau ngực','EMP015');
INSERT INTO PATIENT VALUES ('PAT016', N'Bùi Thị Hạnh', 'Female','2002-12-14','0924567890','994567893',N'HCM',N'Viêm xoang','EMP016');
INSERT INTO PATIENT VALUES ('PAT017', N'Dương Quốc Thái', 'Male','1978-8-3','0925678901','995678904',N'HCM',N'Đau cột sống','EMP017');
INSERT INTO PATIENT VALUES ('PAT018', N'Nguyễn Khánh Vy', 'Female','1999-4-10','0926789012','996789015',N'HCM',N'Sốt virus','EMP018');
INSERT INTO PATIENT VALUES ('PAT019', N'Lâm Chí Kiệt', 'Male','1985-9-5','0927890123','997890126',N'HCM',N'Viêm gan','EMP019');
INSERT INTO PATIENT VALUES ('PAT020', N'Huỳnh Thanh Trúc', 'Female','2001-2-28','0928901234','998901237',N'HCM',N'Đau bụng kinh','EMP020');
INSERT INTO PATIENT VALUES ('PAT021', N'Trần Gia Huy', 'Male','1994-11-9','0929012345','999012348',N'HCM',N'Chấn thương phần mềm','EMP021');
INSERT INTO PATIENT VALUES ('PAT022', N'Ngô Thị Tường Vy', 'Female','1996-7-7','0930123456','900123459',N'HCM',N'Viêm đường tiết niệu','EMP022');
INSERT INTO PATIENT VALUES ('PAT023', N'Vũ Minh Tâm', 'Male','1982-1-2','0931234567','901234560',N'HCM',N'Tai nạn giao thông','EMP023');
INSERT INTO PATIENT VALUES ('PAT024', N'Phan Thị Mỹ Lệ', 'Female','2005-3-17','0932345678','902345671',N'HCM',N'Cảm cúm','EMP024');
INSERT INTO PATIENT VALUES ('PAT025', N'Đặng Thanh Phong', 'Male','1989-10-30','0933456789','903456782',N'HCM',N'Đau thắt lưng','EMP025');
INSERT INTO PATIENT VALUES ('PAT026', N'Trịnh Thu Ngân', 'Female','1993-6-12','0934567890','904567893',N'HCM',N'Khó ngủ','EMP025');
GO

INSERT INTO MEDICAL_RECORD VALUES ('MR001', 'PAT001', 'EMP001', '2025-1-10', N'Cảm cúm', N'Uống thuốc', N'Tình trạng nhẹ');
INSERT INTO MEDICAL_RECORD VALUES ('MR002', 'PAT002', 'EMP002', '2025-1-11', N'Viêm họng', N'Kháng sinh', N'Khó nuốt');
INSERT INTO MEDICAL_RECORD VALUES ('MR003', 'PAT003', 'EMP003', '2025-1-12', N'Sốt xuất huyết', N'Theo dõi tại nhà', N'Sốt cao');
INSERT INTO MEDICAL_RECORD VALUES ('MR004', 'PAT004', 'EMP001', '2025-1-13', N'Tiểu đường', N'Insulin', N'Kiểm tra đường');
INSERT INTO MEDICAL_RECORD VALUES ('MR005', 'PAT005', 'EMP002', '2025-1-14', N'Huyết áp cao', N'Uống thuốc', N'Theo dõi định kỳ');
INSERT INTO MEDICAL_RECORD VALUES ('MR006', 'PAT006', 'EMP003', '2025-1-15', N'Viêm phổi', N'Kháng sinh IV', N'Cần nghỉ ngơi');
INSERT INTO MEDICAL_RECORD VALUES ('MR007', 'PAT007', 'EMP001', '2025-1-16', N'Dị ứng', N'Thuốc kháng dị ứng', N'Ngứa ngáy');
INSERT INTO MEDICAL_RECORD VALUES ('MR008', 'PAT008', 'EMP002', '2025-1-17', N'Viêm gan B', N'Điều trị dài hạn', N'Ăn uống cẩn thận');
INSERT INTO MEDICAL_RECORD VALUES ('MR009', 'PAT009', 'EMP003', '2025-1-18', N'Loét dạ dày', N'Thuốc giảm axit', N'Kiêng đồ cay');
INSERT INTO MEDICAL_RECORD VALUES ('MR010', 'PAT010', 'EMP001', '2025-1-19', N'Mất ngủ', N'Thuốc ngủ nhẹ', N'Trạng thái căng thẳng');
GO

INSERT INTO MEDICAL_FORM VALUES ('MF001', 'PAT001', 'EMP001', '2025-1-10', N'Ho, sốt nhẹ', N'Cảm cúm');
INSERT INTO MEDICAL_FORM VALUES ('MF002', 'PAT002', 'EMP002', '2025-1-11', N'Đau họng, sốt', N'Viêm họng');
INSERT INTO MEDICAL_FORM VALUES ('MF003', 'PAT003', 'EMP003', '2025-1-12', N'Sốt cao, mệt mỏi', N'Sốt xuất huyết');
INSERT INTO MEDICAL_FORM VALUES ('MF004', 'PAT004', 'EMP001', '2025-1-13', N'Khát nước, mệt', N'Tiểu đường');
INSERT INTO MEDICAL_FORM VALUES ('MF005', 'PAT005', 'EMP002', '2025-1-14', N'Đau đầu, chóng mặt', N'Huyết áp cao');
INSERT INTO MEDICAL_FORM VALUES ('MF006', 'PAT006', 'EMP003', '2025-1-15', N'Ho nhiều, khó thở', N'Viêm phổi');
INSERT INTO MEDICAL_FORM VALUES ('MF007', 'PAT007', 'EMP001', '2025-1-16', N'Phát ban, ngứa', N'Dị ứng');
INSERT INTO MEDICAL_FORM VALUES ('MF008', 'PAT008', 'EMP002', '2025-1-17', N'Mệt mỏi, vàng da', N'Viêm gan B');
INSERT INTO MEDICAL_FORM VALUES ('MF009', 'PAT009', 'EMP003', '2025-1-18', N'Đau bụng, ợ chua', N'Loét dạ dày');
INSERT INTO MEDICAL_FORM VALUES ('MF010', 'PAT010', 'EMP001', '2025-1-19', N'Thức khuya, mệt', N'Mất ngủ');
GO

INSERT INTO BILL VALUES ('B001', 'PAT001', 'EMP001', '2026-1-17', 500000, N'Tiền mặt', N'Paid');
INSERT INTO BILL VALUES ('B002', 'PAT002', 'EMP002', '2026-1-18', 800000, N'Thẻ tín dụng', N'Unpaid');
INSERT INTO BILL VALUES ('B003', 'PAT003', 'EMP003', '2025-1-19', 300000, N'Chuyển khoản', N'Paid');
INSERT INTO BILL VALUES ('B004', 'PAT004', 'EMP001', '2026-1-20', 450000, N'Tiền mặt', N'Paid');
INSERT INTO BILL VALUES ('B005', 'PAT005', 'EMP002', '2025-1-21', 700000, N'Thẻ tín dụng', N'Unpaid');
INSERT INTO BILL VALUES ('B006', 'PAT006', 'EMP003', '2025-1-22', 600000, N'Chuyển khoản', N'Paid');
INSERT INTO BILL VALUES ('B007', 'PAT007', 'EMP001', '2025-1-23', 250000, N'Tiền mặt', N'Paid');
INSERT INTO BILL VALUES ('B008', 'PAT008', 'EMP002', '2025-1-24', 900000, N'Thẻ tín dụng', N'Unpaid');
INSERT INTO BILL VALUES ('B009', 'PAT009', 'EMP003', '2025-1-25', 400000, N'Chuyển khoản', N'Paid');
INSERT INTO BILL VALUES ('B010', 'PAT010', 'EMP001', '2025-1-26', 550000, N'Tiền mặt', N'Paid');
GO

INSERT INTO MONITORING_SHEET VALUES ('MS001', 'PAT001', 'EMP001', '2025-1-10','2025-1-17', N'Ổn định');
INSERT INTO MONITORING_SHEET VALUES ('MS002', 'PAT002', 'EMP002', '2025-1-11','2025-1-18', N'Đang theo dõi');
INSERT INTO MONITORING_SHEET VALUES ('MS003', 'PAT003', 'EMP003', '2025-1-12','2025-1-19', N'Cần nghỉ ngơi');
INSERT INTO MONITORING_SHEET VALUES ('MS004', 'PAT004', 'EMP001', '2025-1-13','2025-1-20', N'Ổn định');
INSERT INTO MONITORING_SHEET VALUES ('MS005', 'PAT005', 'EMP002', '2025-1-14','2025-1-21', N'Huyết áp giảm');
INSERT INTO MONITORING_SHEET VALUES ('MS006', 'PAT006', 'EMP003', '2025-1-15','2025-1-22', N'Ho giảm');
INSERT INTO MONITORING_SHEET VALUES ('MS007', 'PAT007', 'EMP001', '2025-1-16','2025-1-23', N'Dị ứng giảm');
INSERT INTO MONITORING_SHEET VALUES ('MS008', 'PAT008', 'EMP002', '2025-1-17','2025-1-24', N'Vàng da giảm');
INSERT INTO MONITORING_SHEET VALUES ('MS009', 'PAT009', 'EMP003', '2025-1-18','2025-1-25', N'Ổn định');
INSERT INTO MONITORING_SHEET VALUES ('MS010', 'PAT010', 'EMP001', '2025-1-19','2025-1-26', N'Ngủ tốt hơn');
GO

INSERT INTO DRUG VALUES ('D001', N'Paracetamol', N'Viên', 20000, 500);
INSERT INTO DRUG VALUES ('D002', N'Amoxicillin', N'Viên', 30000, 300);
INSERT INTO DRUG VALUES ('D003', N'Ibuprofen', N'Viên', 50000, 200);
INSERT INTO DRUG VALUES ('D004', N'Metformin', N'Viên', 30000, 150);
INSERT INTO DRUG VALUES ('D005', N'Lisinopril', N'Viên', 30000, 100);
INSERT INTO DRUG VALUES ('D006', N'Prednisone', N'Viên', 30000, 50);
INSERT INTO DRUG VALUES ('D007', N'Loratadine', N'Viên', 30000, 70);
INSERT INTO DRUG VALUES ('D008', N'Omeprazole', N'Viên', 30000, 90);
INSERT INTO DRUG VALUES ('D009', N'Atorvastatin', N'Viên', 30000, 80);
INSERT INTO DRUG VALUES ('D010', N'Aspirin', N'Viên', 30000, 120);
ALTER TABLE ACCOUNT ADD AvatarPath NVARCHAR(MAX);
GO
