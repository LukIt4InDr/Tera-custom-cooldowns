﻿using System.Collections.Generic;
using TCC.TeraCommon.Game.Messages;
using TCC.TeraCommon.Game.Services;

namespace TCC.Parsing.Messages
{
    public class S_LEARN_EP_PERK : ParsedMessage
    {
        public Dictionary<uint, uint> Perks;

        public S_LEARN_EP_PERK(TeraMessageReader r) : base(r)
        {
            var perksCount = r.ReadInt16();
            var perksOffset = r.ReadInt16();

            var success = r.ReadBoolean();
            var usedPoints = r.ReadUInt32();

            Perks = new Dictionary<uint, uint>();
            if (perksCount == 0) return;

            for (var i = 0; i < perksCount; i++)
            {
                var curr = r.ReadUInt16();
                var next = r.ReadUInt16();
                var perkId = r.ReadUInt32();
                var perkLevel = r.ReadUInt32();
                Perks[perkId] = perkLevel;
                if (next == 0) break;
                r.RepositionAt(next);
            }

        }
    }
}