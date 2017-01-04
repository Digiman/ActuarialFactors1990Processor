CREATE PROCEDURE [dbo].[GetRemainderInterestFromTableD]
	@Years int,
	@Rate float
AS
begin
	SELECT remainderInterest FROM tblD WHERE Years = @Years AND PayoutRate = @Rate;

end
