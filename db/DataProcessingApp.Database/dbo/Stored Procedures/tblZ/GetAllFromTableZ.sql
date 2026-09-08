CREATE PROCEDURE [dbo].[GetAllFromTableZ]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT dFactor, nFactor, mFactor FROM tblZ
	WHERE MortalityTable = @Mortality AND Age = @Age AND InterestRate = @Rate;
end
