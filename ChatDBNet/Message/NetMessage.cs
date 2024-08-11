using System.Text.Json;

namespace ChatDBNet.Message
{
    public enum MessageType
    {
        SIMPLE,
        LIST,
        REGISTRATION,
        AUTHENTIFICATION,
        CONFIRMATION
    }
    public class NetMessage()
    {
        public MessageType MessageType { get; set; } = MessageType.SIMPLE;

        public DateTime DateTime { get; set; } = DateTime.UtcNow;

        public string UserFrom { get; set; } = string.Empty;

        public string UserTo { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{MessageType} {DateTime} From: \"{UserFrom}\". To: \"{UserTo}\". Text: \"{Text}\".";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.MessageType, this.DateTime, this.UserFrom, this.UserTo, this.Text);
        }

        public override bool Equals(object? obj)
        {
            if (obj?.GetType() == typeof(NetMessage))
            {
                var that = (NetMessage)obj;
                return DateTime.Equals(this.DateTime, that.DateTime)
                && String.Equals(this.UserFrom, that.UserFrom)
                && String.Equals(this.UserTo, that.UserTo)
                && String.Equals(this.Text, that.Text);
            }
            else return false;
        }

        public string SerializeToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static NetMessage? DeserializeFromJson(string json)
        {
            return JsonSerializer.Deserialize<NetMessage>(json);
        }

        public static NetMessage CreateListMessage(IEnumerable<NetMessage> messages, string sender, string receiver)
        {
            string text = JsonSerializer.Serialize<List<NetMessage>>(messages.ToList());
            NetMessage result = new()
            {
                UserFrom = sender,
                UserTo = receiver,
                Text = text,
                MessageType = MessageType.LIST
            };
            return result;
        }

        public static List<NetMessage>? ExtractMessages(NetMessage listMessage)
        {
            if (listMessage.MessageType == MessageType.LIST)
            {
                return JsonSerializer.Deserialize<List<NetMessage>>(listMessage.Text);
            }
            else throw new Exception("Message is not list message.");
        }
    }
}