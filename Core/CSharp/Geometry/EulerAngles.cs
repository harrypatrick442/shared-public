using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Core.Exceptions;
namespace Core.Geometry
{
	[DataContract]
    [KnownType(typeof(EulerAngles))]
    public class EulerAngles {

        private float _YawRadians;
        [JsonPropertyName("yawRadians")]
        [JsonInclude]
        [DataMember(Name = "yawRadians")]
        public float YawRadians { get { return _YawRadians; } protected set { _YawRadians = value; } }

        private float _PitchRadians;
        [JsonPropertyName("pitchRadians")]
        [JsonInclude]
        [DataMember(Name = "pitchRadians")]
        public float PitchRadians { get { return _PitchRadians; } protected set { _PitchRadians = value; } }
        private float _RollRadians;
        [JsonPropertyName("rollRadians")]
        [JsonInclude]
        [DataMember(Name = "rollRadians")]
        public float RollRadians { get { return _RollRadians; } protected set { _RollRadians = value; } }
        public EulerAngles(float yawRadians, float pitchRadians, float rollRadians) {
            _YawRadians = yawRadians;
            _PitchRadians = pitchRadians;
            _RollRadians = rollRadians;
        }
        protected EulerAngles() { }
    }
}
