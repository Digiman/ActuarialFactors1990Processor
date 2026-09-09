CREATE PROCEDURE [dbo].[GetDFactorFromTableZ]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT dFactor FROM tblZ WHERE MortalityTable = @Mortality AND Age = @Age AND InterestRate = @Rate;
end
