CREATE PROCEDURE [dbo].[GetPresentValueIncomeFromTableB]
	@Years float,
	@Rate float
AS
begin
	SELECT pvIncomeInterest FROM tblB 
	WHERE Years = @Years AND Rate = @Rate;
end
