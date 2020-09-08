using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace VPMobileRuntime100_1_0.Model
{
    public static class Labels
    {
        public static LabelDefinition BuildLabelWithId(String idKey, String labelKey, String latKey, String longKey, bool displayLocation, int textSize, Color textColor, Color haloColor)
        {
            var textColorValues = new int[] { textColor.R, textColor.G, textColor.B, textColor.A };
            var haloColorValues = new int[] { haloColor.R, haloColor.G, haloColor.B, haloColor.A };

            var labelJson = @"{
      ""labelExpression"": ""[" + idKey + @"]"",
      ""labelExpressionInfo"": {
            ""expression"": ""return $feature[\""" + labelKey + @"\""]" + (displayLocation ? @" + \"" Lat: \"" + $feature[\""" + latKey + @"\""] + \"" Long: \"" + $feature[\""" + longKey + @"\""]" : String.Empty) + @";""
      },
      ""useCodedValues"": false,
      ""maxScale"": 0,
      ""minScale"": 0,
      ""deconflictionStrategy"": ""none"",
      ""labelPlacement"": ""esriServerPointLabelPlacementCenterRight"",
      ""symbol"": {
                    ""color"": [" + textColorValues[0] + @",
                                " + textColorValues[1] + @",
                                " + textColorValues[2] + @",
                                " + textColorValues[3] + @"],
                    ""type"": ""esriTS"",
                    ""haloSize"": 2,
                    ""haloColor"": [
                                " + haloColorValues[0] + @",
                                " + haloColorValues[1] + @",
                                " + haloColorValues[2] + @",
                                " + haloColorValues[3] + @"],
                    ""horizontalAlignment"": ""left"",
                    ""rightToLeft"": false,
                    ""angle"": 0,
                    ""xoffset"": 0,
                    ""yoffset"": 0,
                    ""text"": """",
                    ""rotated"": false,
                    ""kerning"": true,
                    ""font"": {
                        ""size"": " + textSize + @",
                        ""style"": ""normal"",
                        ""decoration"": ""none"",
                        ""weight"": ""bold"", 
                        ""family"": ""Arial""
                    }
          }
}";

            return LabelDefinition.FromJson(labelJson);
        }

        public static LabelDefinition BuildLabelWithLocation(String latKey, String longKey, int textSize, Color textColor, Color haloColor)
        {
            var textColorValues = new int[] { textColor.R, textColor.G, textColor.B, textColor.A };
            var haloColorValues = new int[] { haloColor.R, haloColor.G, haloColor.B, haloColor.A };

            var labelJson = @"{
      ""labelExpressionInfo"": {
                    ""expression"": ""return \"" Lat: \"" + $feature[\""" + latKey + @"\""] + \"" Long: \"" + $feature[\""" + longKey + @"\""];""
      },
      ""useCodedValues"": false,
      ""maxScale"": 0,
      ""minScale"": 0,
      ""deconflictionStrategy"": ""none"",
      ""labelPlacement"": ""esriServerPointLabelPlacementCenterRight"",
      ""symbol"": {
                    ""color"": [" + textColorValues[0] + @",
                                " + textColorValues[1] + @",
                                " + textColorValues[2] + @",
                                " + textColorValues[3] + @"],
                    ""type"": ""esriTS"",
                    ""haloSize"": 2,
                    ""haloColor"": [
                                " + haloColorValues[0] + @",
                                " + haloColorValues[1] + @",
                                " + haloColorValues[2] + @",
                                " + haloColorValues[3] + @"],
                    ""horizontalAlignment"": ""left"",
                    ""rightToLeft"": false,
                    ""angle"": 0,
                    ""xoffset"": 0,
                    ""yoffset"": 0,
                    ""text"": """",
                    ""rotated"": false,
                    ""kerning"": true,
                    ""font"": {
                        ""size"": " + textSize + @",
                        ""style"": ""normal"",
                        ""decoration"": ""none"",
                        ""weight"": ""bold"", 
                        ""family"": ""Arial""
                    }
          }
}";

            return LabelDefinition.FromJson(labelJson);
        }
    }
}
