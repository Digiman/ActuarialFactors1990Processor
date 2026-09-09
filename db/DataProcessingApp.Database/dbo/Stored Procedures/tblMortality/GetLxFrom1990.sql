CREATE PROCEDURE [dbo].[GetLxFrom1990]
	@Age int
AS
begin
	SELECT lx FROM tblMortality WHERE Year = 1990 AND Age = @Age;
end