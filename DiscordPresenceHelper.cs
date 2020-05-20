using System;
using DiscordRPC;

namespace Ets2AtsCustomRichPresence
{
    class DiscordPresenceHelper
    {
        // maintain and update the dicord rich presence
        DiscordRpcClient client;

        public DiscordPresenceHelper(string clientId)
        {
            client = new DiscordRpcClient(clientId);
        }

        public void InitClient() 
        {
            client.Initialize();
        }

        public void PresenceUpdate(string details, string state) 
        {
            client.SetPresence(new RichPresence()
            {
                Details = details,
                State = state
/*                Assets = new Assets()
                {
                    LargeImageKey = "image_large",
                    LargeImageText = "Lachee's Discord IPC Library",
                    SmallImageKey = "image_small"
                }*/
            });
        }

        public void DeInitClient() 
        {
            client.Dispose();
            client.Deinitialize();
        }
    }
}
