CREATE PROCEDURE [dbo].[GetMFactorFromTableZ]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT mFactor FROM tblZ WHERE MortalityTable = @Mortality AND Age = @Age AND InterestRate = @Rate;
end
