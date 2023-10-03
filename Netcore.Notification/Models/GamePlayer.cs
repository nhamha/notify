namespace Netcore.Notification.Models
{
    public class GamePlayer
    {
        public Account Account { get; private set; }
        public GamePlayer(Account acc)
        {
            Account = acc;
        }
    }
}