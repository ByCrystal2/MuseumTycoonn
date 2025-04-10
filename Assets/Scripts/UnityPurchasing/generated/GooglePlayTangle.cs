// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("Rrt22cSGno3QMYBmEOqi4NXElXfamLPqK4kRkoKNNFhg8MkPhxFgvR5w3/sa3SIL4PjAdqJn443kDKcIsUeYVmr9WwVb68X/qoQ1/MklEPiZ1fGBWK7s7zVgWRiUm+J+eMxKdiHyxZ2T0LNgeCwnT38bYVglQVPtXB4xxu5k9Ot+0oeUO54C/01pQYQ0K7MvOqiEO0Z98dfv3cwlyW+rQd9t7s3f4unmxWmnaRji7u7u6u/sixMV8uEEMcOp7Tr9cKRko2sRqI0ZC9+ez8P8oQWLY3B51foGznCjl4yCU26qMuGE+TjsO0ViYgK/JzV6be7g799t7uXtbe7u72kSQJyTWFd6lU+IDaPaoIFQoWeSA/lHlUtCfRojMDzAHsFZtO3s7u/u");
        private static int[] order = new int[] { 7,6,9,11,10,8,12,10,10,9,13,13,12,13,14 };
        private static int key = 239;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
