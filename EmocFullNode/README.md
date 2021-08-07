Emocoin Node 
===============

Emoji NFT Blockchain Implementation in C#
----------------------------

Emocoin is an implementation of the Stratis protocol in C# on the [.NET Core](https://dotnet.github.io/) platform.  
[.NET Core](https://dotnet.github.io/) is an open source cross platform framework and enables the development of applications and services on Windows, macOS and Linux.  

Join our community on [discord](https://discord.gg/GkJbrgAPYf).

Future plans
----------

We plan to add many more features on top of the Emocoin blockchain:
Wallet Emoji trading, Smart Contracts, Mobile wallet and more!

Running an EmocNode
------------------

Download premade binaries:
```
git clone https://github.com/emo-coin/EmocNode.git
https://github.com/emo-coin/EmocNode/releases/tag/latest

```


or source and build:

```
git clone https://github.com/emo-coin/EmocNode.git
cd EmocoinFullNode\src\Emocoin.NodeD

dotnet build

```

To run on the EmocTest network:
```
cd Emocoin.NodeD
dotnet run -testnet
```  

