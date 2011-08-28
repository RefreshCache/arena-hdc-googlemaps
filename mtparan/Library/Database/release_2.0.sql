/*********************************** DATA ****************************************/

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


/*********************************** DATA ****************************************/

/* GoogleMaps_ProfileSource Organization Setting */
IF NOT EXISTS (SELECT * FROM orgn_organization_setting WHERE [Key]='GoogleMaps_ProfileSource')
BEGIN
	INSERT INTO orgn_organization_setting
		( organization_id, [Key], Value
		, date_created, date_modified, created_by, modified_by
		, Descr
		, system_flag, category_luid, [read_only])
		VALUES
		( 1, 'GoogleMaps_ProfileSource', ''
		, GETDATE(), GETDATE(), 'Google Maps Install', 'Google Maps Install'
		, 'The source to use when adding a person to a new tag from a Google Map view.'
		, 0, NULL, 0)
END
GO

/* GoogleMaps_ProfileStatus Organization Setting */
IF NOT EXISTS (SELECT * FROM orgn_organization_setting WHERE [Key]='GoogleMaps_ProfileStatus')
BEGIN
	INSERT INTO orgn_organization_setting
		( organization_id, [Key], Value
		, date_created, date_modified, created_by, modified_by
		, Descr
		, system_flag, category_luid, [read_only])
		VALUES
		( 1, 'GoogleMaps_ProfileStatus', ''
		, GETDATE(), GETDATE(), 'Google Maps Install', 'Google Maps Install'
		, 'New tag members from the Google Map view will have this member status in a tag.'
		, 0, NULL, 0)
END
GO
