// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.42000
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Yavsc.Models.Relationship {
    using System;
    using System.Reflection;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class HyperLink {
        
        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager(("Yavsc.Server.Resources." + "Yavsc.Models.Relationship.HyperLink"), typeof(HyperLink).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        public static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        public static string MethodDisplayName {
            get {
                return ResourceManager.GetString("MethodDisplayName", resourceCulture);
            }
        }
        
        public static string ContentTypeDisplayName {
            get {
                return ResourceManager.GetString("ContentTypeDisplayName", resourceCulture);
            }
        }
        
        public static string RelDisplayName {
            get {
                return ResourceManager.GetString("RelDisplayName", resourceCulture);
            }
        }
        
        public static string HRefDisplayName {
            get {
                return ResourceManager.GetString("HRefDisplayName", resourceCulture);
            }
        }
    }
}
