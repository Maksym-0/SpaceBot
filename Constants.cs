using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceBot
{
    internal class Constants
    {
        public static string BotID = Environment.GetEnvironmentVariable("BOT_TOKEN");
        public static string ApiAdress = "https://spaceapi-spaceapi.up.railway.app";
        public static string ApiHost = "/api/SpacePhoto";
    }
}
