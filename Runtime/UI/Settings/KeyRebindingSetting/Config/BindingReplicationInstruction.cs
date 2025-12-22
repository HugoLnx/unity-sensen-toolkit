using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SensenToolkit
{
    [System.Serializable]
    public class BindingReplicationInstruction
    {
        public DynamicInputActionReference TargetActionReference;
        public List<string> BlacklistRegex = new();
        public List<string> WhitelistRegex = new();

        public bool IsReplicationAllowed(string bindingPath)
        {
            bool isBlacklisted = BlacklistRegex.Any((pattern) => Regex.IsMatch(bindingPath, pattern));
            if (isBlacklisted) return false;

            if (WhitelistRegex.Count == 0) return true;

            bool isWhitelisted = WhitelistRegex.Any((pattern) => Regex.IsMatch(bindingPath, pattern));
            return isWhitelisted;
        }
    }
}
