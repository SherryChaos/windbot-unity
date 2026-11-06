#if !UNITY_5_3_OR_NEWER
using System.Data;
using Mono.Data.Sqlite;
#endif
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace YGOSharp.OCGWrapper
{
    public static class NamedCardsManager
    {
        private static IDictionary<int, NamedCard> _cards;

#if UNITY_5_3_OR_NEWER
        /// <summary>Unity: the host feeds the card table (no sqlite on device).</summary>
        public static void Init(ICardDataProvider provider)
        {
            _cards = new Dictionary<int, NamedCard>();
            foreach (CardRow row in provider.GetAll())
            {
                if (!_cards.ContainsKey(row.Id))
                    _cards.Add(row.Id, new NamedCard(row));
            }
        }
#else
        public static void Init(string databaseFullPath)
        {
            try
            {
                if (!File.Exists(databaseFullPath))
                {
                    throw new Exception("Could not find the cards database.");
                }

                _cards = new Dictionary<int, NamedCard>();

                using (SqliteConnection connection = new SqliteConnection("Data Source=" + databaseFullPath))
                {
                    connection.Open();

                    using (IDbCommand command = new SqliteCommand(
                        "SELECT datas.id, ot, alias, setcode, type, level, race, attribute, atk, def, texts.name, texts.desc"
                        + " FROM datas INNER JOIN texts ON datas.id = texts.id",
                        connection))
                    {
                        using (IDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                LoadCard(reader);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Could not initialize the cards database. Check the inner exception for more details.", ex);
            }
        }
#endif

        internal static NamedCard GetCard(int id)
        {
            if (_cards.ContainsKey(id))
                return _cards[id];
            return null;
        }

        public static IList<NamedCard> GetAllCards()
        {
            return _cards.Values.ToList();
        }

#if !UNITY_5_3_OR_NEWER
        private static void LoadCard(IDataRecord reader)
        {
            NamedCard card = new NamedCard(reader);
            _cards.Add(card.Id, card);
        }
#endif
    }
}
