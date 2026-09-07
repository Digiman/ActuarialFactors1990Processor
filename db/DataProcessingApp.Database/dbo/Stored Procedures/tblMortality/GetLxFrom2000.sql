CREATE PROCEDURE [dbo].[GetLxFrom2000]
	@Age int
AS
begin
	SELECT lx FROM tblMortality WHERE Year = 2000 AND Age = @Age;
end