CREATE PROCEDURE [dbo].[GetRemainderFactorFromTableU1]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT remainderFactor FROM tblU1 
	WHERE MortalityTable = @Mortality AND Age = @Age AND AdjustedPayoutRate = @Rate;
end