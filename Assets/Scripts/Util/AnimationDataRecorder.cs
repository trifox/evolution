using UnityEngine;
using Keiwando.JSON;
using System.IO;
using System.Collections.Generic;

namespace Keiwando.Evolution { 

    public class AnimationDataRecorder: MonoBehaviour {

        private const string SAVE_FODER = "AnimationData";
        private string RESOURCE_PATH;

        public Creature Creature;

        private AnimationData animationData;

        public bool Recording { get { return recording; } }
        private bool recording = false;

        void FixedUpdate() {
            // Collect the data
            if (recording) {
                for (int i = 0; i < Creature.joints.Count; i++) {
                    var joint = Creature.joints[i];
                    var positions = animationData.jointPositions;
                    var id = joint.JointData.id;
                    positions[id].Add(joint.center.x);
                    positions[id].Add(joint.center.y);
                }
            }
        }

        public void StartRecording(CreatureDesign design) {

            RESOURCE_PATH = Path.Combine(Application.persistentDataPath, SAVE_FODER);

            animationData = new AnimationData(
                design, (int)System.Math.Round(1f / Time.fixedDeltaTime)
            );
            foreach (var joint in Creature.joints) {
                animationData.jointPositions[joint.JointData.id] = new List<float>();
            }
            recording = true;
        }

        public void EndRecording() {
            recording = false;
            WriteDataToFile();
        }

        public void WriteDataToFile() {

            if (animationData == null) return;

            string encoded = animationData.Encode().ToString();
            string filename = GetAvailableName(GetSuggestedName(animationData.design));
            Directory.CreateDirectory(RESOURCE_PATH);
            File.WriteAllText(PathToAnimationDataSave(filename), encoded);
        }

        private List<string> GetAllFilenames() {
            return FileUtil.GetFilenamesInDirectory(RESOURCE_PATH, ".json");
        }

        private string GetAvailableName(string suggestedName) {

            var existingNames = GetAllFilenames();
            int counter = 2;
            var finalName = suggestedName;
            while (existingNames.Contains(finalName)) {
                finalName = string.Format("{0} ({1})", suggestedName, counter);
                counter++;
            }
            return finalName;
        }

        private string GetSuggestedName(CreatureDesign design) {
            return string.Format("{0}", design.Name);
        }

        private string PathToAnimationDataSave(string name) {
            return Path.Combine(RESOURCE_PATH, name + ".json");
        }

        // MARK: - AnimationData

        private class AnimationData: IJsonConvertible {
            public readonly CreatureDesign design;
            private int fps;
            public Dictionary<int, List<float>> jointPositions = new Dictionary<int, List<float>>();

            public AnimationData(CreatureDesign design, int fps) {
                this.design = design;
                this.fps = fps;
            }

            public JObject Encode() {

                var json = new JObject();
                json["design"] = design.Encode();
                json["fps"] = fps;
                var positionsJSON = new JObject();
                foreach (KeyValuePair<int, List<float>> entry in jointPositions) {
                    positionsJSON[entry.Key.ToString()] = entry.Value;
                }
                json["jointPositions"] = positionsJSON;

                return json;
            }
        }
    }   
}