// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("ScrEy/tJysHJScrKy0sXl1vVd2nobE+9FuKXFD2/tfBzyKwEOWa72eF6InOkqnl0Jfkewx0IG91VuoC1DPbVNT1BefiP7QDD9Z9i37gyDJoKE7iYECMzBgm58R1YbFtcoLlPOm2OM8u4W9aNR6l+9rDu0laNMgotCOrxjQyJshacPA8nRVRabo96PbE/160D+Y86GYGchCqOXKTfmUc1XlnyAH3UzieHEgiUO8RCi50/EVadSZ4dOx+YgWdHEBp5I9cFeK0IbmTgbShoXcz6sZbVH7f7CgDOC6ktL3Nth1stpBNRmupbAR5qIgb9s5/R+0nK6fvGzcLhTYNNPMbKysrOy8i2jOVQTjWhEtX85uVmXcQBQGUSEyTh/s72/iY/EMnIysvK");
        private static int[] order = new int[] { 1,12,7,13,11,11,7,8,8,9,13,13,12,13,14 };
        private static int key = 203;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
