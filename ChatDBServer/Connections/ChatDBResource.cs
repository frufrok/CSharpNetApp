using ChatDBNet.Message;
using ChatDBServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatDB.Models;

namespace ChatDBServer.Connections
{
    public class ChatDBResource : IDBResource
    {
        public void AddMessage(NetMessage message, bool isSent, out int ID)
        {
            using var context = new ChatDBContext();
            int userFromID = GetUserID(message.UserFrom);
            int userToID = GetUserID(message.UserTo);
            var dbMessage = new Message()
            {
                DateTimeSend = message.DateTime,
                UserFromID = userFromID,
                UserToID = userToID,
                Text = message.Text
            };
            context.Messages.Add(dbMessage);
            context.SaveChanges();
            ID = dbMessage.ID ?? -1;
        }

        public void AddUser(string nickName, out int ID)
        {
            using var context = new ChatDBContext();
            User user = new() { Nickname = nickName };
            context.Users.Add(user);
            context.SaveChanges();
            ID = user.ID ?? -1;
        }

        public List<NetMessage> GetMessages(int userID, bool unsentOnly)
        {
            using var context = new ChatDBContext();
            return context.Messages.Where(x => (x.UserToID.Equals(userID) && unsentOnly ? x.IsSent == false : true)).Select(x => new NetMessage()
            {
                DateTime = x.DateTimeSend ?? DateTime.UtcNow,
                UserFrom = x.UserFrom.Nickname,
                UserTo = x.UserTo.Nickname,
                Text = x.Text
            }).ToList();
        }

        public int GetUserID(string nickname)
        {
            using var context = new ChatDBContext();
            if (context.Users.Any(x => x.Nickname.ToLower().Equals(nickname.ToLower())))
            {
                var dict = context.Users.ToDictionary(x => x.Nickname.ToLower(), x => x);
                return dict[nickname.ToLower()].ID ?? -1;
            }
            else return -1;
        }

        public Dictionary<int, string> GetUsers()
        {
            using var context = new ChatDBContext();
            return context.Users.ToDictionary(x => x.ID ?? -1, x => x.Nickname);
        }
    }
}
