using Cirrious.MvvmCross.Community.Plugins.Sqlite;
using Cirrious.MvvmCross.Test.Core;
using MWF.Mobile.Core.Converters;
using Newtonsoft.Json;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MWF.Mobile.Tests.Converters
{
    public class SingleObjectToListConverterTests
        : MvxIoCSupportingTest
    {
        private IFixture _fixture;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

        }


        [Fact]
        public void SingleObjectToListConverter_SingleObject()
        {
            string jsonString = @"{""barcodes"":{""barcode"":""506010A032001855533201""}}";

            var deserializeJson = JsonConvert.DeserializeObject<TestItem>(jsonString);
        }

        [Fact]
        public void SingleObjectToListConverter_ListObject()
        {
            string jsonString = @"{""barcodes"": { ""barcode"": [   ""506012A076001852453401"",   ""506012A076001852453402"", ""506012A076001852453403""]}}";

            var deserializeJson = JsonConvert.DeserializeObject<TestItem>(jsonString);
        }

        [Fact]
        public void SingleObjectToListConverter_EmptyListObject()
        {
            string jsonString = @"{""barcodes"": { ""barcode"": [ ]}}";

            var deserializeJson = JsonConvert.DeserializeObject<TestItem>(jsonString);
        }

        [Fact]
        public void SingleObjectToListConverter_2LevelObject()
        {
            string jsonString = @"{""instructions"": { ""instruction"": { ""line"":""NEXT DAY\r\n      **CUSTOMERS PAPERWORK MUST BE USED***\r\n"" }}}";

            var deserializeJson = JsonConvert.DeserializeObject<TestInstructionsContainer>(jsonString);
            Assert.Equal(1, deserializeJson.Instructions.Count());
            Assert.Equal(1, deserializeJson.Instructions.First().LinesList.Count());
            Assert.Equal("NEXT DAY\r\n      **CUSTOMERS PAPERWORK MUST BE USED***\r\n", deserializeJson.Instructions.First().LinesList.First());
        }


        [Fact]
        public void SingleObjectToListConverter_NullInstruction()
        {
            string jsonString = @"{""instructions"": { ""instruction"": null}}";

            var deserializeJson = JsonConvert.DeserializeObject<TestInstructionsContainer>(jsonString);
            Assert.Equal(0, deserializeJson.Instructions.Count());

        }

        public class TestInstructionsContainer
        {
            [JsonProperty("instructions")]
            [JsonConverter(typeof(JsonWrappedListConverter<Core.Models.Instruction.Instruction>))]
            public List<Core.Models.Instruction.Instruction> Instructions { get; set; }
        }

        public class TestItem
        {

            [JsonProperty("barcodes")]
            [JsonConverter(typeof(JsonWrappedListConverter<string>))]
            public List<string> Barcodes { get; set; }
        }
            
    }
}
