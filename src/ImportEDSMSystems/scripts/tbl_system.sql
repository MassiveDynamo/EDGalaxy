CREATE TABLE tbl_system (
    id INT PRIMARY KEY,
    id64 bigint NULL,
    name CHAR(43) NOT NULL COLLATE 'utf8mb4_general_ci',
    x DOUBLE NOT NULL,
    y DOUBLE NOT NULL,
    z DOUBLE NOT NULL,
    coords GEOMETRY NOT NULL,
    SPATIAL INDEX(coords),
    UNIQUE INDEX `Name` (`Name`) USING BTREE
);