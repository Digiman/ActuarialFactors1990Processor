CREATE PROCEDURE [dbo].[GetAllFromTableC]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT remainderFactor, rFactor, dFactor FROM tblC 
	WHERE MortalityTable = @Mortality AND Age = @Age AND Rate = @Rate;
end
