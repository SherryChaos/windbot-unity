using System;
using System.IO;
using System.Text;
using System.Threading;
using WindBot.Game;
using WindBot.Game.AI;
using YGOSharp.OCGWrapper;

namespace WindBot.Unity
{
    /// <summary>
    /// In-process WindBot driver. Runs the bot loop on a background thread so Unity stays responsive.
    /// Usage (host):
    ///   WindBotRunner.Start(new WindBotInfo { Host="127.0.0.1", Port=serverPort, Name="BotA", Deck="Blue-Eyes" }, cards);
    ///   ... later WindBotRunner.Stop();
    /// </summary>
    public static class WindBotRunner
    {
        private static Thread _thread;
        private static GameClient _client;
        private static volatile bool _stop;

        public static bool IsRunning
        {
            get
            {
                Thread t = _thread;
                return t != null && t.IsAlive;
            }
        }

        public static GameClient Client
        {
            get { return _client; }
        }

        public static void Start(WindBotInfo info, ICardDataProvider cards)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            if (IsRunning)
                throw new InvalidOperationException("WindBot is already running.");

            NamedCardsManager.Init(cards);
            DecksManager.Init();

            WindBotIO.Provider = (directory, filename, extension) =>
            {
                string text = WindBotResources.Get(directory, filename, extension);
                if (text == null)
                    return null;
                return new MemoryStream(Encoding.UTF8.GetBytes(text));
            };

            _stop = false;
            _client = new GameClient(info);
            _thread = new Thread(Loop);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public static void Stop()
        {
            _stop = true;
            try
            {
                GameClient client = _client;
                if (client != null && client.Connection != null)
                    client.Connection.Close();
            }
            catch
            {
                // ignore shutdown races
            }

            Thread thread = _thread;
            if (thread != null)
            {
                try { thread.Join(2000); } catch { }
            }
            _thread = null;
            _client = null;
        }

        private static void Loop()
        {
            try
            {
                _client.Start();
                while (!_stop && _client.Connection.IsConnected)
                {
                    _client.Tick();
                    Thread.Sleep(30);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLine("WindBot runner error", ex);
            }
        }
    }
}
