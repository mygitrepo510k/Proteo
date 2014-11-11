﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MWF.Mobile.Core.Converters
{

    public class JsonWrappedListConverter<T> : JsonConverter
        where T: new()
    {

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var target = new List<T>();
            var wrappedObject = jsonObject.Children().First().Children().First();

            if (!wrappedObject.HasValues)
                return target;

            if (wrappedObject.Type == JTokenType.Array)
                serializer.Populate(wrappedObject.CreateReader(), target);
            else
            {
                var targetItem = new T();
                serializer.Populate(wrappedObject.CreateReader(), targetItem);
                target.Add(targetItem);
            }

            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

    }

}
