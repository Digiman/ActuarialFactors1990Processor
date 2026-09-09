CREATE PROCEDURE [dbo].[GetDFactorFromTableC]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT dFactor FROM tblC 
	WHERE MortalityTable = @Mortality AND Age = @Age AND Rate = @Rate;
end
