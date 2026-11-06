using System.Collections.Generic;

namespace YGOSharp.OCGWrapper
{
    /// <summary>One cdb `datas`/`texts` row, host-fed (no sqlite on device).</summary>
    public struct CardRow
    {
        public int Id;
        public int Ot;
        public int Alias;
        public long Setcode;
        public int Type;
        /// <summary>Packed level info: low 8 bits = level, bits 16..23 = rscale, bits 24..31 = lscale.</summary>
        public int LevelInfo;
        public int Race;
        public int Attribute;
        public int Attack;
        public int Defense;
        public string Name;
        public string Description;
    }

    /// <summary>Implemented by the Unity host (OcgCardDatabase) to feed WindBot's card table.</summary>
    public interface ICardDataProvider
    {
        IEnumerable<CardRow> GetAll();
    }
}
