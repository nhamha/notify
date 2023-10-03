using System;

namespace ServerCore.Utilities.Models
{
    public class UpdatedProfile
    {
        public UpdatedProfile(long accountID, string accountName, long amount, byte moneyType, byte gameId, string characterName)
        {
            this.AccountId = accountID;
            this.AccountName = accountName;
            this.Amount = amount;
            this.MoneyType = moneyType;
            this.GameId = gameId;
            this.CharacterName = characterName;
        }

        public UpdatedProfile()
        {
        }

        public long AccountId { get; set; }

        public string AccountName { get; set; }

        public string CharacterName { get; set; }

        public long Amount { get; set; }

        public byte MoneyType { get; set; }

        public byte GameId { get; set; }
    }
}