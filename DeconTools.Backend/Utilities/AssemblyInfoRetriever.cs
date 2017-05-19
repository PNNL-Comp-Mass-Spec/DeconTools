using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DeconTools.Backend.Utilities
{
    public static class AssemblyInfoRetriever
    {

        public static string GetVersion(Type classType, bool showExtendedInfo)
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetAssembly(classType);
                var match = Regex.Match(assembly.ToString(), "Version=(?<versionNum>.*), Culture");
                if (match.Success)
                {
                    var versionString = match.Groups["versionNum"].Value;

                    if (showExtendedInfo)
                    {
                        return ("v" + versionString);
                    }
                    else
                    {
                        var posOfLastDot = versionString.LastIndexOf(".");
                        if (posOfLastDot > 1)
                        {
                            return ("v" + versionString.Substring(0, posOfLastDot));
                        }
                        else
                        {
                            return ("v" + versionString);
                        }
                    }
                }
                else
                {
                    return "";
                }

            }
            catch (Exception)
            {

                return "";
            }

        }

        
        
        public static string GetVersion(Type classType)
        {
            return GetVersion(classType, true);

 

        }

    }
}
