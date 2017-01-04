CREATE PROCEDURE [dbo].[GetAdjustmentFactorFromTableK]
	@Rate float,
	@Frequency nvarchar(50)
AS
begin
	SELECT adjustmentFactor FROM tblK WHERE InterestRate = @Rate AND Frequency = @Frequency;
end