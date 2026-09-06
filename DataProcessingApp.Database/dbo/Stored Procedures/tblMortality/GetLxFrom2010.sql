CREATE PROCEDURE [dbo].[GetLxFrom2010]
	@Age int
AS
begin
	SELECT lx FROM tblMortality WHERE Year = 2010 AND Age = @Age;
end