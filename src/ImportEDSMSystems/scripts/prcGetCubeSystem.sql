CREATE DEFINER=`root`@`localhost` PROCEDURE `prcGetCubeSystems`(
	IN `p_SystemName` CHAR(43),
	IN `p_Size` INT
)
LANGUAGE SQL
NOT DETERMINISTIC
CONTAINS SQL
SQL SECURITY DEFINER
COMMENT ''
BEGIN
    DECLARE v_Name VARCHAR(43);
    DECLARE v_X FLOAT;
    DECLARE v_Y FLOAT;
    DECLARE v_Z FLOAT;
    DECLARE v_Id INT;
    DECLARE v_Id64 BIGINT;
    

    -- Get the values from the first record meeting the criteria
    SELECT 
        Id, Id64, Name, X, Y, Z
    INTO 
        v_Id, v_Id64, v_Name, v_X, v_Y, v_Z
    FROM 
        system
    WHERE 
        Name = p_SystemName
    LIMIT 1;

    -- Select records based on the specified cube size and coordinates
    SELECT Id, Id64, Name, X, Y, Z,
        SQRT(POW(v_X - X, 2) + POW(v_Y - Y, 2) + POW(v_Z - Z, 2)) AS Distance
    FROM system
    WHERE 
        X > v_X - (p_Size / 2) AND X < v_X + (p_Size / 2) AND
        Y > v_Y - (p_Size / 2) AND Y < v_Y + (p_Size / 2) AND
        Z > v_Z - (p_Size / 2) AND Z < v_Z + (p_Size / 2)
   ORDER BY Distance;

END