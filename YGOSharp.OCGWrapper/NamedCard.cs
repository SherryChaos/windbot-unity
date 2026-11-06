using System;
#if !UNITY_5_3_OR_NEWER
using System.Data;
#endif

namespace YGOSharp.OCGWrapper
{
    public class NamedCard : Card
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

#if UNITY_5_3_OR_NEWER
        internal NamedCard(CardRow row) : base(row)
        {
            Name = row.Name;
            Description = row.Description;
        }
#endif

#if !UNITY_5_3_OR_NEWER
        internal NamedCard(IDataRecord reader) : base(reader)
        {
            Name = reader.GetString(10);
            Description = reader.GetString(11);
        }
#endif

        public static new NamedCard Get(int id)
        {
            return NamedCardsManager.GetCard(id);
        }

        /// <summary>
        /// Returns whether a card ID and its alias represent alternate artwork.
        /// Aliases with an ID difference of 20 or more are rule-name aliases, not alternate artwork.
        /// </summary>
        public static bool IsAltartAlias(int cardId, int alias)
        {
            // YGOPro treats this normal-monster printing as a rule-name alias of the ritual monster.
            if (cardId == 5405695)
                return false;
            return cardId > 0 && alias > 0 && Math.Abs(cardId - alias) < 20;
        }
    }
}
