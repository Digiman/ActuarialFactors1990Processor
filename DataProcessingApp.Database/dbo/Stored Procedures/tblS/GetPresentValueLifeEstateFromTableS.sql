CREATE PROCEDURE [dbo].[GetPresentValueLifeEstateFromTableS]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT pvLifeEstate FROM tblS 
	WHERE MortalityTable = @Mortality AND Age = @Age AND InterestRate = @Rate;
end
