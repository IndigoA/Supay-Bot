using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supay.Bot.Properties;
using Supay.Irc;
using Supay.Irc.Messages;

namespace Supay.Bot
{
    internal class CommandContext
    {
        private readonly Channel _channel;
        private readonly Client _irc;
        private readonly UserCollection _users;

        private string _message;
        private string[] _messageTokens;
        private bool _replyNotice;

        public CommandContext(Client irc, UserCollection users, User from, Channel channel, string message)
        {
            this._irc = irc;
            this._users = users;

            this.From = from;
            this._channel = channel;

            if (message[0] == '!' || message[0] == '.')
            {
                this._replyNotice = true;
            }
            else
            {
                this._replyNotice = false;
            }

            if (message[0] == '!' || message[0] == '.' || message[0] == '@')
            {
                this.Message = message.Substring(1);
            }
            else
            {
                this.Message = message;
            }
        }

        public UserCollection Users
        {
            get
            {
                return this._users;
            }
        }

        public string Message
        {
            get
            {
                return this._message;
            }
            set
            {
                if (this._message != value)
                {
                    this._message = value;
                    this._messageTokens = this._message.Split(' ');
                }
            }
        }

        public string[] MessageTokens
        {
            get
            {
                return this._messageTokens;
            }
        }

        public User From
        {
            get;
            private set;
        }

        public bool IsAdmin
        {
            get
            {
                if (this.From.Nickname.EqualsI("_aLfa_") || this.From.Nickname.EqualsI("_aLfa_|laptop") || this.From.Nickname.EqualsI("P_Gertrude"))
                {
                    return true;
                }
                return Settings.Default.Administrators.Split(';').Any(admin => this.From.Nickname.EqualsI(admin));
            }
        }

        public bool ReplyNotice
        {
            get
            {
                return this._replyNotice;
            }
            set
            {
                this._replyNotice = value;
            }
        }

        public string Channel
        {
            get
            {
                if (this._channel == null)
                {
                    return null;
                }
                return this._channel.Name;
            }
        }

        public async Task<string> GetPlayerName(string query)
        {
            // remove leading and trailing whitespace
            query = query.Trim();

            // fix player name
            if (query.StartsWithI("&") || query.EndsWithI("&") || query.StartsWithI("*") || query.EndsWithI("*"))
            {
                return query.Trim(new[] { '&', '*' }).ValidatePlayerName();
            }

            // lookup player in users collection and return his name from database
            // or return a validated player name from query
            User peer;
            return this._users.TryGetValue(query, out peer)
                ? await Database.Lookup("rsn", "users", "fingerprint=@fp", new[] { new MySqlParameter("@fp", peer.FingerPrint) }, query.ValidatePlayerName())
                : query.ValidatePlayerName();
        }

        public async Task SendReply(string message, params object[] args)
        {
            message = string.Format(CultureInfo.InvariantCulture, message, args);

            if (message.Length > 512)
            {
                message = message.Substring(0, 512);
            }

            if (this._replyNotice)
            {
                await this._irc.Send(new NoticeMessage(message, this.From.Nickname));
            }
            else
            {
                if (this._channel == null)
                {
                    await this._irc.SendChat(message, this.From.Nickname);
                }
                else
                {
                    await this._irc.SendChat(message, this._channel.Name);
                }
            }
        }

        public async Task SendReply(StringBuilder stringBuilder)
        {
            if (stringBuilder == null)
            {
                return;
            }

            await SendReply(stringBuilder.ToString());
        }

        public async Task SendReply(IEnumerable<StringBuilder> stringBuilders)
        {
            foreach (var stringBuilder in stringBuilders)
            {
                await SendReply(stringBuilder);
            }
        }
    }
}
