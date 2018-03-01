using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CkMp.Data.Objects;

namespace CkMp.Data.Objects
{
    public static class ScriptObjectExtensions
    {
        private const string flagRegex = "CWCcivFlag_(Large|Small)";
        private const string fuelDepotRegex = "CWCcivFuel_(Large|Medium|Small)_..*";

        public static bool IsFlag(this ScriptObject scriptObject)
        {
            return Regex.IsMatch(scriptObject.Type, flagRegex);
        }

        public static bool IsFuelDepot(this ScriptObject scriptObject)
        {
            return Regex.IsMatch(scriptObject.Type, fuelDepotRegex);
        }

        public static bool IsResource(this ScriptObject scriptObject)
        {
            return scriptObject.IsFlag() || scriptObject.IsFuelDepot();
        }
    }
}
