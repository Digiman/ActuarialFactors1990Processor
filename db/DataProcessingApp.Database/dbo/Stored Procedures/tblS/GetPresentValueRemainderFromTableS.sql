CREATE PROCEDURE [dbo].[GetPresentValueRemainderFromTableS]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT pvRemainderInterest FROM tblS 
	WHERE MortalityTable = @Mortality AND Age = @Age AND InterestRate = @Rate;
end