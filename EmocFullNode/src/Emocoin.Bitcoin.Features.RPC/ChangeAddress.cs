﻿using NBitcoin;

namespace Emocoin.Bitcoin.Features.RPC
{
    public class ChangeAddress
    {
        public Money Amount { get; set; }
        public BitcoinAddress Address { get; set; }
    }
}