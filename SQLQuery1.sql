CREATE TABLE Employee (
	EmployeeId nvarchar(128) NOT NULL PRIMARY KEY,
	FirstLastName nvarchar(128) NOT NULL,
	Email nvarchar(128) NOT NULL
);
CREATE TABLE Project (
	ProjectKey nvarchar(10) NOT NULL PRIMARY KEY,
	ProjectName nvarchar(60)
)

CREATE TABLE Task(
	TaskId nvarchar(15) NOT NULL PRIMARY KEY,
	TaskName nvarchar(255) NOT NULL,
	ReporterId nvarchar(128) NOT NULL,
	AsigneeId nvarchar(128) NOT NULL ,
	FOREIGN KEY (ReporterId) REFERENCES Employee(EmployeeId),
	FOREIGN KEY (AsigneeId) REFERENCES Employee(EmployeeId)
);

ALTER TABLE Task 
	ADD ProjectKey nvarchar(10),
	FOREIGN KEY (ProjectKey) REFERENCES Project(ProjectKey)
;

ALTER TABLE Task
	ADD EstimatedTime time

ALTER TABLE Task
	ADD ElapsedTime time

ALTER TABLE Task
	ADD Done bit

ALTER TABLE Task
	ADD DoneDate date

CREATE TABLE Worklog (
	WorklogId int IDENTITY(1,1) PRIMARY KEY,
	EmployeeId nvarchar(128) NOT NULL,
	TaskId nvarchar(15) NOT NULL,
	WorklogDate date NOT NULL,
	TimeSpent time,
	FOREIGN KEY (EmployeeId) REFERENCES Employee(EmployeeId),
	FOREIGN KEY (TaskId) REFERENCES Task(TaskId)
)
