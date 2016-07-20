using Xunit;
using Dukebox.Configuration.Helper;

namespace Dukebox.Tests.Unit
{
    public class Base64DecoderTests
    {
        private const string expectedResult = "This is a test string.";

        [Fact]
        public void Base64Decode_Should_Decode_Base64_Strings()
        {
            var result = Base64Decoder.Base64Decode("VGhpcyBpcyBhIHRlc3Qgc3RyaW5nLg==");

            var resultCorrect = result.Equals(expectedResult);

            Assert.True(resultCorrect, "Base64Decoder did not decode a Base64 string correctly");
        }

        [Fact]
        public void DoubleBase64Decode_Should_Decode_Doubly_Encoded_Base64_String()
        {
            var result = Base64Decoder.DoubleBase64Decode("VkdocGN5QnBjeUJoSUhSbGMzUWdjM1J5YVc1bkxnPT0=");

            var resultCorrect = result.Equals(expectedResult);

            Assert.True(resultCorrect, "Base64Decoder did not decode a doubly encoded Base64 string correctly");
        }
    }
}
