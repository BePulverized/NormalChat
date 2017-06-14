using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace NormalChat
{
    public class ChatHub : Hub
    {
        private const string initVector = "pemgail9uzpgzl88";
        // This constant is used to determine the keysize of the encryption algorithm
        private const int keysize = 256;

        public void Send(string name, string message, string encrypted)
        {

            foreach (var userConnection in UserHandler.ConnectedIds)
            {
                if (userConnection.Username == "Hacker")
                {
                    if (encrypted == "True")
                    {
                        Clients.Client(userConnection.ConnectionID)
                            .addNewMessageToPage(userConnection.Username, EncryptString(message, "lazydog"), name);
                    }
                    else
                    {
                        Clients.Client(userConnection.ConnectionID)
                            .addNewMessageToPage(userConnection.Username, message, name);
                    }
                }
                else
                {
                    Clients.Client(userConnection.ConnectionID).addNewMessageToPage(userConnection.Username, message, name);
                }
            }
                
            
        }

        public override Task OnConnected()
        {
            var us = new UserConnection();
            us.Username = Context.QueryString["username"];
            us.ConnectionID = Context.ConnectionId;
            UserHandler.ConnectedIds.Add(us);
            return base.OnConnected();
        }

        public static string EncryptString(string plainText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }
    }
}