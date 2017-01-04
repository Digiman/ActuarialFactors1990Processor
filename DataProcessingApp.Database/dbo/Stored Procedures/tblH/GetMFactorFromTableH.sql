CREATE PROCEDURE [dbo].[GetMFactorFromTableH]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT mFactor FROM tblH WHERE MortalityTable = @Mortality AND Age = @Age AND InterestRate = @Rate;
end