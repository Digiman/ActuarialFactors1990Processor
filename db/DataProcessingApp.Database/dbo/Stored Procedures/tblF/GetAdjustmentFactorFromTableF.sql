CREATE PROCEDURE [dbo].[GetAdjustmentFactorFromTableF]
	@Rate float,
	@Frequency nvarchar(50),
	@Months int
AS
begin
	SELECT adjustmentFactor FROM tblF 
	WHERE InterestRate = @Rate AND Frequency = @Frequency AND Months = @Months;
end