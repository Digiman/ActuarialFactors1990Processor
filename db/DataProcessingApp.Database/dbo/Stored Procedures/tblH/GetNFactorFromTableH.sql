CREATE PROCEDURE [dbo].[GetNFactorFromTableH]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT nFactor FROM tblH WHERE MortalityTable = @Mortality AND Age = @Age AND InterestRate = @Rate;
end
