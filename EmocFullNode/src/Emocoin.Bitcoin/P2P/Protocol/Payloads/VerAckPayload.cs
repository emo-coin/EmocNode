﻿using NBitcoin;

namespace Emocoin.Bitcoin.P2P.Protocol.Payloads
{
    [Payload("verack")]
    public class VerAckPayload : Payload, IBitcoinSerializable
    {
        public override void ReadWriteCore(BitcoinStream stream)
        {
        }
    }
}