﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BridgeVs.VsPackage.Helper {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BridgeVs.VsPackage.Helper.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Software\LINQBridgeVs\{0}\Solutions\{1}.
        /// </summary>
        internal static string EnabledProjectsRegistryKey {
            get {
                return ResourceManager.GetString("EnabledProjectsRegistryKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Software\LINQBridgeVs\{0}.
        /// </summary>
        internal static string ProductRegistryKey {
            get {
                return ResourceManager.GetString("ProductRegistryKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Software\LINQBridgeVs\{0}.
        /// </summary>
        internal static string ProductVersion {
            get {
                return ResourceManager.GetString("ProductVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to If you skip the configuration you won&apos;t be able to use LINQBridgeVs. Are you sure?.
        /// </summary>
        internal static string SkipMessage {
            get {
                return ResourceManager.GetString("SkipMessage", resourceCulture);
            }
        }
    }
}
