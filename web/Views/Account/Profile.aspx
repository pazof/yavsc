<%@ Page Title="Profile" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<Profile>" %>

<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = ViewData["UserName"]+" at "+ YavscHelpers.SiteName +" - profile edition" ; %>
</asp:Content>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
  <style>
table.layout { border-width: 0;  }
table.layout TR TD { max-width:40%; }
   </style>
   <%= Html.ValidationSummary() %>
<% using(Html.BeginForm("Profile", "Account", FormMethod.Post, new { enctype = "multipart/form-data" })) %>
<% { %>
  
 <%= Html.Hidden("UserName",ViewData["ProfileUserName"]) %>

   <fieldset><legend>Informations publiques</legend>


<%= Html.LabelFor(model => model.Name) %> :
<%= Html.TextBox("Name") %> 
<%= Html.ValidationMessage("Name", "*") %>
<br>

<%= Html.LabelFor(model => model.WebSite) %> : 
<%= Html.TextBox("WebSite") %>
<%= Html.ValidationMessage("WebSite", "*") %>
<br>

Avatar : <img class="avatar" src="<%=Model.avatar%>?version=<%=Html.Encode(DateTime.Now.ToString())%>" alt=""/>
<input type="file" id="AvatarFile" name="AvatarFile"/>
<%= Html.ValidationMessage("AvatarFile", "*") %>

</fieldset>

    <fieldset><legend>Blog</legend>

<%= Html.LabelFor(model => model.BlogVisible) %> :
<%= Html.CheckBox("BlogVisible") %>
<%= Html.ValidationMessage("BlogVisible", "*") %>
<br>

<%= Html.LabelFor(model => model.BlogTitle) %> :
<%= Html.TextBox("BlogTitle") %>
<%= Html.ValidationMessage("BlogTitle", "*") %>

   </fieldset>

    <fieldset><legend>Contact</legend>
  
<%= Html.LabelFor(model => model.Phone) %>
<%= Html.TextBox("Phone") %>
<%= Html.ValidationMessage("Phone", "*") %>

<%= Html.LabelFor(model => model.Mobile) %>
<%= Html.TextBox("Mobile") %>
<%= Html.ValidationMessage("Mobile", "*") %>

<%= Html.LabelFor(model => model.Address) %>
<%= Html.TextBox("Address") %>
<%= Html.ValidationMessage("Address", "*") %>

<%= Html.LabelFor(model => model.CityAndState) %>
<%= Html.TextBox("CityAndState") %>
<%= Html.ValidationMessage("CityAndState", "*") %>

<%= Html.LabelFor(model => model.ZipCode) %>
<%= Html.TextBox("ZipCode") %>
<%= Html.ValidationMessage("ZipCode", "*") %>

<%= Html.LabelFor(model => model.Country) %>
<%= Html.TextBox("Country") %>
<%= Html.ValidationMessage("Country", "*") %>
</fieldset>
<fieldset><legend>Disponibilité</legend>
   <%= Html.LabelFor(model => model.GoogleCalendar) %> :
   
    <%= Html.Encode(Model.GoogleCalendar) %>
   <%= Html.ActionLink("Choisir l'agenda","ChooseCalendar","Google",new { returnUrl= Request.Url.AbsolutePath }, new { @class="actionlink" }) %>
</fieldset>
<fieldset><legend>Informations de facturation</legend>
   
<%= Html.LabelFor(model => model.BankCode) %> :
<%= Html.TextBox("BankCode") %>
<%= Html.ValidationMessage("BankCode", "*") %>
<br>

<%= Html.LabelFor(model => model.WicketCode) %> :
<%= Html.TextBox("WicketCode") %>
<%= Html.ValidationMessage("WicketCode", "*") %>
<br>
   
<%= Html.LabelFor(model => model.AccountNumber) %> :
<%= Html.TextBox("AccountNumber") %>
<%= Html.ValidationMessage("AccountNumber", "*") %>
<br>
<%= Html.LabelFor(model => model.BankedKey) %> :  
<%= Html.TextBox("BankedKey") %>
<%= Html.ValidationMessage("BankedKey", "*") %>
<br>
<%= Html.LabelFor(model => model.BIC) %> :
<%= Html.TextBox("BIC") %>
<%= Html.ValidationMessage("BIC", "*") %>
<br>
<%= Html.LabelFor(model => model.IBAN) %> :
<%= Html.TextBox("IBAN") %>
<%= Html.ValidationMessage("IBAN", "*") %>
</fieldset>

<input type="submit"/>
<% } %>
   <aside>
   <%= Html.ActionLink("Changer de mot de passe","ChangePassword", "Account",null, new { @class="actionlink" })%>
   <%= Html.ActionLink("Désincription","Unregister", "Account",  new { id=ViewData["UserName"] } , new { @class="actionlink" })%>
   </aside>
   <aside>
   <% 	if (Roles.IsUserInRole((string)ViewData ["UserName"],"Admin")) { %>
   This user is Admin.
   <% } %>
   HasBankAccount:<%= Model.HasBankAccount %> 
   <% if (!Model.HasBankAccount) { %>
   (IBAN+BIC ou Codes banque, guichet, compte et clé RIB)
   <% } %>, IsBillable:<%=Model.IsBillable%>
    <% if (!Model.IsBillable) { %>
     (un nom et au choix, une adresse postale valide,
   ou un téléphone, ou un email, ou un Mobile)   <% } %>
   </aside>

</asp:Content>

