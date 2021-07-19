using Emocoin.Bitcoin.Configuration;
using Emocoin.Bitcoin.Consensus;

namespace Emocoin.Bitcoin
{
    public static class NodeHelpers
    {
        public static DbType GetDbType(this NodeSettings nodeSettings)
        {
            var dbTypeString = nodeSettings.ConfigReader.GetOrDefault("dbtype", "leveldb");

            DbType dbType = DbType.Leveldb;
            return dbType;
        }
    }
}
