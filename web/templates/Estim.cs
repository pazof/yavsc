﻿// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.17020
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Yavsc.templates {
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using Yavsc.Model.WorkFlow;
    using Yavsc.Model.RolesAndMembers;
    using System;
    
    
    public partial class Estim : EstimBase {
        
        private Estimate _estimField;
        public Estimate estim {
            get {
                return this._estimField;
            }
        }
        private Profile _fromField;
        public Profile from {
            get {
                return this._fromField;
            }
        }
        private Profile _toField;
        public Profile to {
            get {
                return this._toField;
            }
        }

        
        public virtual string TransformText() {
            this.GenerationEnvironment = null;
            
            #line 10 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\n");
            
            #line default
            #line hidden
            
            #line 14 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\n\n\\documentclass[french,11pt]{article}\n\\usepackage{babel}\n\\usepackage[T1]{fontenc}\n\\usepackage[utf8]{inputenc}\n\\usepackage[a4paper]{geometry}\n\\usepackage{units}\n\\usepackage{bera}\n\\usepackage{graphicx}\n\\usepackage{fancyhdr}\n\\usepackage{fp}\n\n\\def\\TVA{20}    % Taux de la TVA\n\n\\def\\TotalHT{0}\n\\def\\TotalTVA{0}\n\n\n\\newcommand{\\AjouterService}[3]{%    Arguments : Désignation, quantité, prix\n    \\FPround{\\prix}{#3}{2}\n    \\FPeval{\\montant}{#2 * #3}\n    \\FPround{\\montant}{\\montant}{2}\n    \\FPadd{\\TotalHT}{\\TotalHT}{\\montant}\n   \n    \\eaddto\\ListeProduits{#1    &    \\prix    &    #2    &    \\montant    \\cr}\n}\n\n\n\\newcommand{\\AfficheResultat}{%\n    \\ListeProduits\n   \n    \\FPeval{\\TotalTVA}{\\TotalHT * \\TVA / 100}\n    \\FPadd{\\TotalTTC}{\\TotalHT}{\\TotalTVA}\n    \\FPround{\\TotalHT}{\\TotalHT}{2}\n    \\FPround{\\TotalTVA}{\\TotalTVA}{2}\n    \\FPround{\\TotalTTC}{\\TotalTTC}{2}\n    \\global\\let\\TotalHT\\TotalHT\n    \\global\\let\\TotalTVA\\TotalTVA\n    \\global\\let\\TotalTTC\\TotalTTC\n   \n\n    \\cr \n    \\hline\n    \\textbf{Total}    & & &    \\TotalHT\n}\n\n\\newcommand*\\eaddto[2]{% version développée de \\addto\n   \\edef\\tmp{#2}%\n   \\expandafter\\addto\n   \\expandafter#1%\n   \\expandafter{\\tmp}%\n}\n\n\\newcommand{\\ListeProduits}{}\n\n\n\n\n%%%%%%%%%%%%%%%%%%%%% A MODIFIER DANS LA FACTURE %%%%%%%%%%%%%%%%%%%%%\n\n\\def\\FactureNum            {");
            
            #line default
            #line hidden
            
            #line 75 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( estim.Id.ToString() ));
            
            #line default
            #line hidden
            
            #line 75 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("}    % Numéro de facture\n\\def\\FactureAcquittee    {non}        % Facture acquittée : oui/non\n\\def\\FactureLieu    {");
            
            #line default
            #line hidden
            
            #line 77 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( from.CityAndState ));
            
            #line default
            #line hidden
            
            #line 77 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("}    % Lieu de l'édition de la facture\n\\def\\FactureObjet    {Facture : ");
            
            #line default
            #line hidden
            
            #line 78 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( estim.Title ));
            
            #line default
            #line hidden
            
            #line 78 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("}    % Objet du document\n% Description de la facture\n\\def\\FactureDescr    {%\n   ");
            
            #line default
            #line hidden
            
            #line 81 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( estim.Description ));
            
            #line default
            #line hidden
            
            #line 81 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\n}\n\n% Infos Client\n\\def\\ClientNom{");
            
            #line default
            #line hidden
            
            #line 85 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( to.Name ));
            
            #line default
            #line hidden
            
            #line 85 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("}    % Nom du client\n\\def\\ClientAdresse{%                    % Adresse du client\n   ");
            
            #line default
            #line hidden
            
            #line 87 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( to.Address ));
            
            #line default
            #line hidden
            
            #line 87 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\\\\\n   ");
            
            #line default
            #line hidden
            
            #line 88 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( to.ZipCode ));
            
            #line default
            #line hidden
            
            #line 88 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(" ");
            
            #line default
            #line hidden
            
            #line 88 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( to.CityAndState ));
            
            #line default
            #line hidden
            
            #line 88 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\\\\\n");
            
            #line default
            #line hidden
            
            #line 89 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
 if (!string.IsNullOrWhiteSpace(to.Phone)) { 
            
            #line default
            #line hidden
            
            #line 90 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("   Téléphone fixe: ");
            
            #line default
            #line hidden
            
            #line 90 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( to.Phone ));
            
            #line default
            #line hidden
            
            #line 90 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\\\\\n");
            
            #line default
            #line hidden
            
            #line 91 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
 } 
            
            #line default
            #line hidden
            
            #line 92 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
 if (!string.IsNullOrWhiteSpace(to.Mobile)) { 
            
            #line default
            #line hidden
            
            #line 93 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("   Mobile: ");
            
            #line default
            #line hidden
            
            #line 93 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( to.Mobile ));
            
            #line default
            #line hidden
            
            #line 93 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\\\\\n");
            
            #line default
            #line hidden
            
            #line 94 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
 } 
            
            #line default
            #line hidden
            
            #line 95 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("   E-mail: ");
            
            #line default
            #line hidden
            
            #line 95 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( to.Email ));
            
            #line default
            #line hidden
            
            #line 95 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\n}\n\n% Liste des produits facturés : Désignation, prix\n\n   ");
            
            #line default
            #line hidden
            
            #line 100 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
 foreach (Writting wr in estim.Lines) { 
            
            #line default
            #line hidden
            
            #line 101 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\\AjouterService    {");
            
            #line default
            #line hidden
            
            #line 101 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(wr.Description));
            
            #line default
            #line hidden
            
            #line 101 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(" ");
            
            #line default
            #line hidden
            
            #line 101 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
 if (!string.IsNullOrWhiteSpace(wr.ProductReference)) { 
            
            #line default
            #line hidden
            
            #line 102 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("   (");
            
            #line default
            #line hidden
            
            #line 102 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(wr.ProductReference));
            
            #line default
            #line hidden
            
            #line 102 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(")");
            
            #line default
            #line hidden
            
            #line 102 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
 } 
            
            #line default
            #line hidden
            
            #line 103 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("} {");
            
            #line default
            #line hidden
            
            #line 103 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(wr.Count));
            
            #line default
            #line hidden
            
            #line 103 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("} {");
            
            #line default
            #line hidden
            
            #line 103 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(wr.UnitaryCost));
            
            #line default
            #line hidden
            
            #line 103 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("} \n   ");
            
            #line default
            #line hidden
            
            #line 104 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
 } 
            
            #line default
            #line hidden
            
            #line 105 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\n%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%\n\n\\geometry{verbose,tmargin=4em,bmargin=8em,lmargin=6em,rmargin=6em}\n\\setlength{\\parindent}{0pt}\n\\setlength{\\parskip}{1ex plus 0.5ex minus 0.2ex}\n\n\\thispagestyle{fancy}\n\\pagestyle{fancy}\n\\setlength{\\parindent}{0pt}\n\n\\renewcommand{\\headrulewidth}{0pt}\n\\cfoot{\n    ");
            
            #line default
            #line hidden
            
            #line 118 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(from.Name));
            
            #line default
            #line hidden
            
            #line 118 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(" - ");
            
            #line default
            #line hidden
            
            #line 118 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(from.Address ));
            
            #line default
            #line hidden
            
            #line 118 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(" - ");
            
            #line default
            #line hidden
            
            #line 118 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( from.CityAndState ));
            
            #line default
            #line hidden
            
            #line 118 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(" \\newline\n    \\small{\n    E-mail: ");
            
            #line default
            #line hidden
            
            #line 120 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( from.Email ));
            
            #line default
            #line hidden
            
            #line 120 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\n    ");
            
            #line default
            #line hidden
            
            #line 121 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
 if (!string.IsNullOrWhiteSpace(from.Mobile)) { 
            
            #line default
            #line hidden
            
            #line 122 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(" - Téléphone mobile: ");
            
            #line default
            #line hidden
            
            #line 122 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( from.Mobile ));
            
            #line default
            #line hidden
            
            #line 122 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
 } 
            
            #line default
            #line hidden
            
            #line 123 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("    ");
            
            #line default
            #line hidden
            
            #line 123 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
 if (!string.IsNullOrWhiteSpace(from.Phone)) { 
            
            #line default
            #line hidden
            
            #line 124 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(" - Téléphone fixe: ");
            
            #line default
            #line hidden
            
            #line 124 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( from.Phone ));
            
            #line default
            #line hidden
            
            #line 124 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
 } 
            
            #line default
            #line hidden
            
            #line 125 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("    }\n}\n\n\\begin{document}\n\n% Logo de la société\n%\\includegraphics{logo.jpg}\n\n% Nom et adresse de la société\n");
            
            #line default
            #line hidden
            
            #line 134 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( from.Name ));
            
            #line default
            #line hidden
            
            #line 134 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\\\\\n");
            
            #line default
            #line hidden
            
            #line 135 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( from.Address ));
            
            #line default
            #line hidden
            
            #line 135 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\\\\\n");
            
            #line default
            #line hidden
            
            #line 136 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(from.ZipCode ));
            
            #line default
            #line hidden
            
            #line 136 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(" ");
            
            #line default
            #line hidden
            
            #line 136 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(from.CityAndState));
            
            #line default
            #line hidden
            
            #line 136 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("\\\\\n\nFacture n°\\FactureNum\n\n\n{\\addtolength{\\leftskip}{10.5cm} %in ERT\n    \\textbf{\\ClientNom}    \\\\\n    \\ClientAdresse        \\\\\n\n} %in ERT\n\n\n\\hspace*{10.5cm}\n\\FactureLieu, le \\today\n\n~\\\\~\\\\\n\n\\textbf{Objet : \\FactureObjet \\\\}\n\n\\textnormal{\\FactureDescr}\n\n~\\\\\n\n\\begin{center}\n    \\begin{tabular}{lrrr}\n        \\textbf{Désignation ~~~~~~}  & \\textbf{Prix unitaire} & \\textbf{Quantité} & \\textbf{Montant (EUR)}    \\\\\n        \\hline\n        \\AfficheResultat{}\n    \\end{tabular}\n\\end{center}\n\n\\begin{flushright}\n\\textit{Auto entreprise en franchise de TVA}\\\\\n\n\\end{flushright}\n~\\\\\n\n\\ifthenelse{\\equal{\\FactureAcquittee}{oui}}{\n    Facture acquittée.\n}{\n\n    À régler par chèque ou par virement bancaire :\n\n    \\begin{center}\n        \\begin{tabular}{|c c c c|}\n            \\hline     \\textbf{Code banque}    & \\textbf{Code guichet}    & \\textbf{N° de Compte}        & \\textbf{Clé RIB}    \\\\\n                    ");
            
            #line default
            #line hidden
            
            #line 182 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( from.BankCode ));
            
            #line default
            #line hidden
            
            #line 182 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("                    & ");
            
            #line default
            #line hidden
            
            #line 182 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( from.WicketCode ));
            
            #line default
            #line hidden
            
            #line 182 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("                     & ");
            
            #line default
            #line hidden
            
            #line 182 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(from.AccountNumber ));
            
            #line default
            #line hidden
            
            #line 182 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("                & ");
            
            #line default
            #line hidden
            
            #line 182 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture(from.BankedKey));
            
            #line default
            #line hidden
            
            #line 182 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write("               \\\\\n            \\hline     \\textbf{IBAN N°}        & \\multicolumn{3}{|l|}{ ");
            
            #line default
            #line hidden
            
            #line 183 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( from.IBAN ));
            
            #line default
            #line hidden
            
            #line 183 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(" }         \\\\\n            \\hline     \\textbf{Code BIC}        & \\multicolumn{3}{|l|}{ ");
            
            #line default
            #line hidden
            
            #line 184 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(this.ToStringHelper.ToStringWithCulture( from.BIC ));
            
            #line default
            #line hidden
            
            #line 184 "/home/paul/workspace/yavsc/web/templates/Estim.tt"
            this.Write(" }         \\\\\n            \\hline\n        \\end{tabular}\n    \\end{center}\n}\n\\end{document}\n");
            
            #line default
            #line hidden
            return this.GenerationEnvironment.ToString();
        }
        
        protected virtual void Initialize() {
            if ((this.Errors.HasErrors == false)) {
                bool _estimAcquired = false;
                if (((this.Session != null) && this.Session.ContainsKey("estim"))) {
                    object data = this.Session["estim"];
                    if (typeof(Estimate).IsAssignableFrom(data.GetType())) {
                        this._estimField = ((Estimate)(data));
                        _estimAcquired = true;
                    }
                    else {
                        this.Error("The type 'Estimate' of the parameter 'estim' did not match the type passed to the template");
                    }
                }
                if ((_estimAcquired == false)) {
                    object data = System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("estim");
                    if ((data != null)) {
                        if (typeof(Estimate).IsAssignableFrom(data.GetType())) {
                            this._estimField = ((Estimate)(data));
                            _estimAcquired = true;
                        }
                        else {
                            this.Error("The type 'Estimate' of the parameter 'estim' did not match the type passed to the template");
                        }
                    }
                }
                bool _fromAcquired = false;
                if (((this.Session != null) && this.Session.ContainsKey("from"))) {
                    object data = this.Session["from"];
                    if (typeof(Profile).IsAssignableFrom(data.GetType())) {
                        this._fromField = ((Profile)(data));
                        _fromAcquired = true;
                    }
                    else {
                        this.Error("The type 'Profile' of the parameter 'from' did not match the type passed to the template");
                    }
                }
                if ((_fromAcquired == false)) {
                    object data = System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("from");
                    if ((data != null)) {
                        if (typeof(Profile).IsAssignableFrom(data.GetType())) {
                            this._fromField = ((Profile)(data));
                            _fromAcquired = true;
                        }
                        else {
                            this.Error("The type 'Profile' of the parameter 'from' did not match the type passed to the template");
                        }
                    }
                }
                bool _toAcquired = false;
                if (((this.Session != null) && this.Session.ContainsKey("to"))) {
                    object data = this.Session["to"];
                    if (typeof(Profile).IsAssignableFrom(data.GetType())) {
                        this._toField = ((Profile)(data));
                        _toAcquired = true;
                    }
                    else {
                        this.Error("The type 'Profile' of the parameter 'to' did not match the type passed to the template");
                    }
                }
                if ((_toAcquired == false)) {
                    object data = System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("to");
                    if ((data != null)) {
                        if (typeof(Profile).IsAssignableFrom(data.GetType())) {
                            this._toField = ((Profile)(data));
                            _toAcquired = true;
                        }
                        else {
                            this.Error("The type 'Profile' of the parameter 'to' did not match the type passed to the template");
                        }
                    }
                }
            }

        }
    }
    
    public class EstimBase {
        
        private global::System.Text.StringBuilder builder;
        
        private global::System.Collections.Generic.IDictionary<string, object> session;
        
        private global::System.CodeDom.Compiler.CompilerErrorCollection errors;
        
        private string currentIndent = string.Empty;
        
        private global::System.Collections.Generic.Stack<int> indents;
        
        private ToStringInstanceHelper _toStringHelper = new ToStringInstanceHelper();
        
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session {
            get {
                return this.session;
            }
            set {
                this.session = value;
            }
        }
        
        public global::System.Text.StringBuilder GenerationEnvironment {
            get {
                if ((this.builder == null)) {
                    this.builder = new global::System.Text.StringBuilder();
                }
                return this.builder;
            }
            set {
                this.builder = value;
            }
        }
        
        protected global::System.CodeDom.Compiler.CompilerErrorCollection Errors {
            get {
                if ((this.errors == null)) {
                    this.errors = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errors;
            }
        }
        
        public string CurrentIndent {
            get {
                return this.currentIndent;
            }
        }
        
        private global::System.Collections.Generic.Stack<int> Indents {
            get {
                if ((this.indents == null)) {
                    this.indents = new global::System.Collections.Generic.Stack<int>();
                }
                return this.indents;
            }
        }
        
        public ToStringInstanceHelper ToStringHelper {
            get {
                return this._toStringHelper;
            }
        }
        
        public void Error(string message) {
            this.Errors.Add(new global::System.CodeDom.Compiler.CompilerError(null, -1, -1, null, message));
        }
        
        public void Warning(string message) {
            global::System.CodeDom.Compiler.CompilerError val = new global::System.CodeDom.Compiler.CompilerError(null, -1, -1, null, message);
            val.IsWarning = true;
            this.Errors.Add(val);
        }
        
        public string PopIndent() {
            if ((this.Indents.Count == 0)) {
                return string.Empty;
            }
            int lastPos = (this.currentIndent.Length - this.Indents.Pop());
            string last = this.currentIndent.Substring(lastPos);
            this.currentIndent = this.currentIndent.Substring(0, lastPos);
            return last;
        }
        
        public void PushIndent(string indent) {
            this.Indents.Push(indent.Length);
            this.currentIndent = (this.currentIndent + indent);
        }
        
        public void ClearIndent() {
            this.currentIndent = string.Empty;
            this.Indents.Clear();
        }
        
        public void Write(string textToAppend) {
            this.GenerationEnvironment.Append(textToAppend);
        }
        
        public void Write(string format, params object[] args) {
            this.GenerationEnvironment.AppendFormat(format, args);
        }
        
        public void WriteLine(string textToAppend) {
            this.GenerationEnvironment.Append(this.currentIndent);
            this.GenerationEnvironment.AppendLine(textToAppend);
        }
        
        public void WriteLine(string format, params object[] args) {
            this.GenerationEnvironment.Append(this.currentIndent);
            this.GenerationEnvironment.AppendFormat(format, args);
            this.GenerationEnvironment.AppendLine();
        }
        
        public class ToStringInstanceHelper {
            
            private global::System.IFormatProvider formatProvider = global::System.Globalization.CultureInfo.InvariantCulture;
            
            public global::System.IFormatProvider FormatProvider {
                get {
                    return this.formatProvider;
                }
                set {
                    if ((this.formatProvider == null)) {
                        throw new global::System.ArgumentNullException("formatProvider");
                    }
                    this.formatProvider = value;
                }
            }
            
            public string ToStringWithCulture(object objectToConvert) {
                if ((objectToConvert == null)) {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                global::System.Type type = objectToConvert.GetType();
                global::System.Type iConvertibleType = typeof(global::System.IConvertible);
                if (iConvertibleType.IsAssignableFrom(type)) {
                    return ((global::System.IConvertible)(objectToConvert)).ToString(this.formatProvider);
                }
                global::System.Reflection.MethodInfo methInfo = type.GetMethod("ToString", new global::System.Type[] {
                            iConvertibleType});
                if ((methInfo != null)) {
                    return ((string)(methInfo.Invoke(objectToConvert, new object[] {
                                this.formatProvider})));
                }
                return objectToConvert.ToString();
            }
        }
    }
}
