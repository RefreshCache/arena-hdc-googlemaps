/****** Object:  UserDefinedFunction [dbo].[cust_hdc_googlemaps_funct_distance_between]    Script Date: 03/18/2011 10:18:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[cust_hdc_googlemaps_funct_distance_between] (
	@LatFrom FLOAT,
	@LonFrom FLOAT,
	@LatTo FLOAT,
	@LonTo FLOAT)
	
	RETURNS FLOAT

AS
BEGIN

	DECLARE @EarthRadius FLOAT
	DECLARE @DeltaLat FLOAT
	DECLARE @DeltaLon FLOAT
	DECLARE @Value FLOAT
	DECLARE @Distance FLOAT

	IF @LatFrom <> 0 AND @LonFrom <> 0 AND @LatTo <> 0 AND @LonTo <> 0
	BEGIN
		SET @EarthRadius = 3958.7
		SET @DeltaLat = RADIANS(@LatTo - @LatFrom)
		SET @DeltaLon = RADIANS(@LonTo - @LonFrom)

		SET @Value = POWER(SIN(@DeltaLat / 2), 2) + COS(RADIANS(@LatFrom)) * COS(RADIANS(@LatTo)) * POWER(SIN(@DeltaLon / 2), 2)
		SET @Value = 2 * ATN2(SQRT(@Value), SQRT(1 - @Value))

		SET @Distance = (@Value * @EarthRadius)
	END
	ELSE
	BEGIN
		SET @Distance = NULL
	END
	
	RETURN @Distance

END

GO


