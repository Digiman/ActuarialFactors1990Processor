CREATE PROCEDURE [dbo].[GetPresentValueAnnuityFromTableB]
	@Years float,
	@Rate float
AS
begin
	SELECT pvAnnuity FROM tblB WHERE Years = @Years AND Rate = @Rate;
end
