﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BookAStar {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///   Une classe de ressource fortement typée destinée, entre autres, à la consultation des chaînes localisées.
    /// </summary>
    // Cette classe a été générée automatiquement par la classe StronglyTypedResourceBuilder
    // à l'aide d'un outil, tel que ResGen ou Visual Studio.
    // Pour ajouter ou supprimer un membre, modifiez votre fichier .ResX, puis réexécutez ResGen
    // avec l'option /str ou régénérez votre projet VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Retourne l'instance ResourceManager mise en cache utilisée par cette classe.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BookAStar.Strings", typeof(Strings).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Remplace la propriété CurrentUICulture du thread actuel pour toutes
        ///   les recherches de ressources à l'aide de cette classe de ressource fortement typée.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Star.
        /// </summary>
        public static string FiveStars {
            get {
                return ResourceManager.GetString("FiveStars", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Célébrité.
        /// </summary>
        public static string ForStars {
            get {
                return ResourceManager.GetString("ForStars", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Un artiste.
        /// </summary>
        public static string NoStar {
            get {
                return ResourceManager.GetString("NoStar", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Étoile montante.
        /// </summary>
        public static string OneStar {
            get {
                return ResourceManager.GetString("OneStar", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à sc.
        /// </summary>
        public static string String1 {
            get {
                return ResourceManager.GetString("String1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à Incontournable.
        /// </summary>
        public static string ThreeStars {
            get {
                return ResourceManager.GetString("ThreeStars", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à À ne manquer sous aucun prétexte.
        /// </summary>
        public static string TwoStars {
            get {
                return ResourceManager.GetString("TwoStars", resourceCulture);
            }
        }
    }
}