CREATE PROCEDURE [dbo].[GetAllFromTableS]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT pvAnnuity, pvLifeEstate, pvRemainderInterest FROM tblS 
	WHERE MortalityTable = @Mortality AND Age = @Age AND InterestRate = @Rate;
end