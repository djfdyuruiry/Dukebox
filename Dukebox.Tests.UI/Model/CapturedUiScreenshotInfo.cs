using Newtonsoft.Json;

namespace Dukebox.Tests.UI.Model
{
    public class CapturedUiScreenshotInfo
    {
        [JsonProperty("metodo")]
        public string Method { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("classe")]
        public string Class { get; set; }
        [JsonProperty("descricao")]
        public string DisplayTestMethod
        {
            get
            {
                return Method;
            }
        }

        public void SetStatus(bool testPassed)
        {
            SetStatus(testPassed, false);
        }

        public void SetStatus(bool testPassed, bool testWasSkipped)
        {
            if (testWasSkipped)
            {
                Status = "skiped";
                return;
            }

            Status = testPassed ? "sucesso" : "falha";
        }
    }
}
