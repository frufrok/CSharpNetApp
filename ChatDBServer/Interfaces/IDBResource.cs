using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatDB.Models;
using ChatDBNet.Message;

namespace ChatDBServer.Interfaces
{
    public interface IDBResource
    {
        public Dictionary<int, string> GetUsers();
        public int GetUserID(string nickname);
        public void AddUser(string NickName, out int ID);
        public List<NetMessage> GetMessages(int userID, bool unsentOnly);
        public void AddMessage(NetMessage message, bool isSent, out int ID);
    }
}
