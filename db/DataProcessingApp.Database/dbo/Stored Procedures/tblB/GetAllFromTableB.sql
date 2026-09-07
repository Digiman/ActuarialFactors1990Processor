CREATE PROCEDURE [dbo].[GetAllFromTableB]
	@Years float,
	@Rate float
as
begin
	SELECT pvAnnuity, pvIncomeInterest, pvRemainderInterest FROM tblB 
	WHERE Years = @Years AND Rate = @Rate;
end