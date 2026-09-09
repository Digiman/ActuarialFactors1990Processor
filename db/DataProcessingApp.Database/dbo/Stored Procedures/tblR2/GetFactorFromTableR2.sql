CREATE PROCEDURE [dbo].[GetFactorFromTableR2]
	@Mortality int,
	@Age1 int,
	@Age2 int,
	@Rate float
AS
begin
	SELECT remainderFactor FROM tblR2 
	WHERE MortalityTable = @Mortality AND Age1 = @Age1 AND Age2 = @Age2 AND AdjustedPayoutRate = @Rate;
end