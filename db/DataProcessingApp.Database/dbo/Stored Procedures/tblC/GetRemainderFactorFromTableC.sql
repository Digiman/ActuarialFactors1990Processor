CREATE PROCEDURE [dbo].[GetRemainderFactorFromTableC]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT remainderFactor FROM tblC 
	WHERE MortalityTable = @Mortality AND Age = @Age AND Rate = @Rate;
end
