CREATE PROCEDURE [dbo].[GetLxFrom1980]
	@Age int
AS
begin	
	SELECT lx FROM tblMortality WHERE Year = 1980 AND Age = @Age;
end
