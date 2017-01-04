CREATE PROCEDURE [dbo].[GetDFactorFromTableH]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT dFactor FROM tblH WHERE MortalityTable = @Mortality AND Age = @Age AND InterestRate = @Rate;
end
