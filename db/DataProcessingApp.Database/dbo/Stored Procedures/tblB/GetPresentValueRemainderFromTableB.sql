CREATE PROCEDURE [dbo].[GetPresentValueRemainderFromTableB]
	@Years float,
	@Rate float
AS
begin
	SELECT pvRemainderInterest FROM tblB 
	WHERE Years = @Years AND Rate = @Rate;
end