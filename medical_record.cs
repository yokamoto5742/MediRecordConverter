using Newtonsoft.Json;

namespace MediRecordConverter
{
    public class MedicalRecord
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string timestamp { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string department { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string subject { get; set; }

        [JsonProperty("object", NullValueHandling = NullValueHandling.Ignore)]
        public string objectData { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string assessment { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string plan { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string comment { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string summary { get; set; }

        [JsonIgnore]
        public string currentSoapSection { get; set; } = "";

        public bool ShouldSerializesubject()
        {
            return !string.IsNullOrEmpty(subject);
        }

        public bool ShouldSerializeobjectData()
        {
            return !string.IsNullOrEmpty(objectData);
        }

        public bool ShouldSerializeassessment()
        {
            return !string.IsNullOrEmpty(assessment);
        }

        public bool ShouldSerializeplan()
        {
            return !string.IsNullOrEmpty(plan);
        }

        public bool ShouldSerializecomment()
        {
            return !string.IsNullOrEmpty(comment);
        }

        public bool ShouldSerializesummary()
        {
            return !string.IsNullOrEmpty(summary);
        }
    }
}