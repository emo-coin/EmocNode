//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey.stoyan@hotmail.com
//        stoyan@cliversoft.com
//        http://www.cliversoft.com
//********************************************************************************************

using System;
using NBitcoin;
using Newtonsoft.Json;

namespace Emocoin.Bitcoin.Utilities.JsonConverters
{
    /// <summary>
    /// Converter used to convert a <see cref="Emoji"/> object to and from JSON.
    /// Uses hex string representation for serialization.
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    public class EmojiJsonConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return typeof(Emoji) == objectType;
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            try
            {
                return new Emoji((string)reader.Value);
            }
            catch
            {
                throw new JsonObjectException("Invalid bitcoin object of type " + objectType.Name, reader);
            }
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
