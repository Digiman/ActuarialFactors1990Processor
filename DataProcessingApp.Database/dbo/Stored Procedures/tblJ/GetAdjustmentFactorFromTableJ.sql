CREATE PROCEDURE [dbo].[GetAdjustmentFactorFromTableJ]
	@Rate float,
	@Frequency nvarchar(50)
AS
begin
	SELECT adjustmentFactor FROM tblJ WHERE InterestRate = @Rate AND Frequency = @Frequency;
end
