CREATE PROCEDURE [dbo].[GetNFactorFromTableZ]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT nFactor FROM tblZ WHERE MortalityTable = @Mortality AND Age = @Age AND InterestRate = @Rate;
end
