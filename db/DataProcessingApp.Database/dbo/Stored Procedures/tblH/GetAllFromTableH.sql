CREATE PROCEDURE [dbo].[GetAllFromTableH]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT dFactor, nFactor, mFactor FROM tblH 
	WHERE MortalityTable = @Mortality AND Age = @Age AND InterestRate = @Rate;
end
