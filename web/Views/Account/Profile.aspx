<%@ Page Title="Profile_edition" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<Profile>" %>

<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = ViewData["UserName"] + " : " +Html.Translate("Profile_edition"); %>
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

Avatar : <img src="<%=Html.AvatarUrl(HttpContext.Current.User.Identity.Name)%>" alt="avatar" class="iconsmall" />

<input type="file" id="AvatarFile" name="AvatarFile"/>
<%= Html.ValidationMessage("AvatarFile", "*") %>

</fieldset>

    <fieldset><legend>Blog</legend>
    <div class="spanel">
<%= Html.LabelFor(model => model.BlogVisible) %> :
<%= Html.CheckBox("BlogVisible") %>
<%= Html.ValidationMessage("BlogVisible", "*") %>
</div><div class="spanel">

<%= Html.LabelFor(model => model.BlogTitle) %> :
<%= Html.TextBox("BlogTitle") %>
<%= Html.ValidationMessage("BlogTitle", "*") %>
</div>
   </fieldset>

    <fieldset><legend>Contact</legend>
<div class="spanel">
<%= Html.LabelFor(model => model.Phone) %>
<%= Html.TextBox("Phone") %>
<%= Html.ValidationMessage("Phone", "*") %>
</div><div class="spanel">
<%= Html.LabelFor(model => model.Mobile) %>
<%= Html.TextBox("Mobile") %>
<%= Html.ValidationMessage("Mobile", "*") %>
</div><div class="spanel">
<%= Html.LabelFor(model => model.Address) %>
<%= Html.TextBox("Address") %>
<%= Html.ValidationMessage("Address", "*") %>
</div><div class="spanel">
<%= Html.LabelFor(model => model.CityAndState) %>
<%= Html.TextBox("CityAndState") %>
<%= Html.ValidationMessage("CityAndState", "*") %>
</div><div class="spanel">
<%= Html.LabelFor(model => model.ZipCode) %>
<%= Html.TextBox("ZipCode") %>
<%= Html.ValidationMessage("ZipCode", "*") %>
</div><div class="spanel">
<%= Html.LabelFor(model => model.Country) %>
<%= Html.TextBox("Country") %>
<%= Html.ValidationMessage("Country", "*") %>
</div>
</fieldset>
<fieldset><legend>Disponibilité</legend>
<div class="spanel">
   <%= Html.LabelFor(model => model.GoogleCalendar) %> :
   
    <%= Html.Encode(Model.GoogleCalendar) %>
   <%= Html.ActionLink("Choisir l'agenda","ChooseCalendar","Google",new { returnUrl= Request.Url.AbsolutePath }, new { @class="actionlink" }) %>
</div></fieldset>
<fieldset><legend>Informations de facturation</legend>

<div class="spanel">
<%= Html.LabelFor(model => model.BankCode) %> :
<%= Html.TextBox("BankCode") %>
<%= Html.ValidationMessage("BankCode", "*") %>
</div><div class="spanel">

<%= Html.LabelFor(model => model.WicketCode) %> :
<%= Html.TextBox("WicketCode") %>
<%= Html.ValidationMessage("WicketCode", "*") %>
</div><div class="spanel">
   
<%= Html.LabelFor(model => model.AccountNumber) %> :
<%= Html.TextBox("AccountNumber") %>
<%= Html.ValidationMessage("AccountNumber", "*") %>
</div><div class="spanel">
<%= Html.LabelFor(model => model.BankedKey) %> :  
<%= Html.TextBox("BankedKey") %>
<%= Html.ValidationMessage("BankedKey", "*") %>
</div><div class="spanel">
<%= Html.LabelFor(model => model.BIC) %> :
<%= Html.TextBox("BIC") %>
<%= Html.ValidationMessage("BIC", "*") %>
</div><div class="spanel">
<%= Html.LabelFor(model => model.IBAN) %> :
<%= Html.TextBox("IBAN") %>
<%= Html.ValidationMessage("IBAN", "*") %>
</div>
</fieldset>

<input type="submit"/>
<% } %>
   <aside>
   <%= Html.ActionLink("Changer de mot de passe","ChangePassword", "Account",null, new { @class="actionlink" })%>
   <%= Html.ActionLink("Désincription","Unregister", "Account",null, new { @class="actionlink" })%>
   </aside>
   <aside>
   <% 	if (Roles.IsUserInRole((string)ViewData ["UserName"],"Admin")) { %>
   This user is Admin.
   <% } %>
   <code>HasBankAccount:<%= Model.HasBankAccount %></code>
   <% if (!Model.HasBankAccount) { %><span class="hint">
   IBAN+BIC ou Codes banque, guichet, compte et clé RIB</span>
   <% } %>, <code>IsBillable:<%=Model.IsBillable%></code>
    <% if (!Model.IsBillable) { %>
    <span class="hint">un nom et au choix, une adresse postale valide,
   ou un téléphone, ou un email, ou un Mobile</span>   <% } %>
   </aside>

</asp:Content>

