CREATE PROCEDURE [dbo].[GetPresentValueAnnuityFromTableS]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT pvAnnuity FROM tblS 
	WHERE MortalityTable = @Mortality AND Age = @Age AND InterestRate = @Rate;
end