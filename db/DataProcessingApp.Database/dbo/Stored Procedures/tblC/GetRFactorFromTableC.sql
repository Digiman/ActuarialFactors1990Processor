CREATE PROCEDURE [dbo].[GetRFactorFromTableC]
	@Mortality int,
	@Age int,
	@Rate float
AS
begin
	SELECT rFactor FROM tblC 
	WHERE MortalityTable = @Mortality AND Age = @Age AND Rate = @Rate;
end
