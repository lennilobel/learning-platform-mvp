CREATE TABLE dbo.Role (
	RoleId bigint NOT NULL,
	RoleName varchar(100) NOT NULL,

	CONSTRAINT PK_Role PRIMARY KEY CLUSTERED (RoleId),
	CONSTRAINT UQ_Role_RoleName UNIQUE (RoleName),
)
