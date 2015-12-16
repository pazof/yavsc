<%@ Page Title="Profile_edition" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<ProfileEdition>" %>

<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = ViewData["UserName"] + " : " +Html.Translate("Profile_edition"); %>
</asp:Content>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
   <% if (Roles.IsUserInRole((string)ViewData ["UserName"],"Admin")) {
   // TODO View all roles
     %><aside>This user is Admin.</aside>
   <% } %>  
   <aside>
   <%= Html.TranslatedActionLink("Changer de mot de passe","ChangePassword", "Account",null, new { @class="actionlink" })%>
   <%= Html.TranslatedActionLink("Désincription", "Unregister", "Account",  new { id = ViewData["UserName"] } , new { @class="actionlink" })%>
   </aside>
   <aside>
   <code>Compte bancaire:<%= Model.HasBankAccount %></code>
   <% if (!Model.HasBankAccount) { %><span class="hint">
   IBAN+BIC ou Codes banque, guichet, compte et clé RIB</span>
   <% } %>, <code>Adressable:<%=Model.IsBillable%></code>
    <% if (!Model.IsBillable) { %>
    <span class="hint">un nom et au choix, une adresse postale valide,
   ou un téléphone, ou un email, ou un Mobile</span>   <% } %>
   </aside>

   <%= Html.ValidationSummary() %>
<% using(Html.BeginForm("Profile", "Account", FormMethod.Post, new { enctype = "multipart/form-data" })) %>
<% { %>
  <%= Html.ValidationSummary() %>
 <%= Html.Hidden("UserName",ViewData["ProfileUserName"]) %>

   <fieldset class="mayhide"><legend>Informations publiques 
  <i>
   <img src="<%=Url.AvatarUrl(HttpContext.Current.User.Identity.Name)%>" alt="avatar" class="avatar" />
   <%=Html.Encode(Model.UserName)%> APE:<%=Model.MEACode%> <%=Model.WebSite%> </i></legend>
   <span>
<%= Html.LabelFor(model => model.MEACode) %> :
<%= Html.DropDownList("MEACode") %>
<%= Html.ValidationMessage("MEACode", "*") %>
<br>

<%= Html.LabelFor(model => model.NewUserName) %> :
<%= Html.TextBox("NewUserName") %> 
<%= Html.ValidationMessage("NewUserName", "*") %>
<br>

<%= Html.LabelFor(model => model.WebSite) %> : 
<%= Html.TextBox("WebSite") %>
<%= Html.ValidationMessage("WebSite", "*") %>
<br>

Avatar : 
<input type="file" id="AvatarFile" name="AvatarFile"/>
<%= Html.ValidationMessage("AvatarFile", "*") %>

</span>
</fieldset>

<fieldset class="mayhide"><legend>Informations administratives
<i><%= string.IsNullOrWhiteSpace(Model.Name)?"KO":Html.Encode(Model.Name) %>
</i>
</legend>
<span>
<%= Html.LabelFor(model => model.Name) %> :
<%= Html.TextBox("Name") %> 
<%= Html.ValidationMessage("Name", "*") %></span>
</fieldset>

    <fieldset class="mayhide"><legend>Blog <i><%=Html.Encode(Model.BlogTitle)%>
    <%= Model.BlogVisible?null:Html.Translate("hidden") %>
    </i></legend>
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

    <fieldset class="mayhide"><legend>Contact
    <i><%=Html.Encode(Model.Phone)%> <%=Html.Encode(Model.Mobile)%> 
    <%=Html.Encode(Model.HasPostalAddress?"adresse OK":"adresse KO")%></i>
    </legend>
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
<fieldset class="mayhide"><legend>Profile préstataire</legend>

<fieldset class="mayhide"><legend>Disponibilité
<i><%=Html.Encode(
    string.IsNullOrWhiteSpace(Model.GoogleCalendar)?"KO":"OK")%></i></legend>
<div class="spanel">
   <%= Html.LabelFor(model => model.GoogleCalendar) %> :
   
    <%= Html.Encode(Model.GoogleCalendar) %>
   <%= Html.TranslatedActionLink("Choisir l'agenda","ChooseCalendar","Google",new { returnUrl= Request.Url.AbsolutePath }, new { @class="actionlink" }) %>
</div></fieldset>
<fieldset class="mayhide"><legend>Informations de facturation
<i> <%=Html.Encode(Model.HasBankAccount?"OK":"KO")%> </i>
</legend>

<p>Saisissez ici vos informations de facturation.</p>

<fieldset class="mayhide">
<legend>Par le numéro de compte</legend>
<div class="spanel">
<%= Html.LabelFor(model => model.BankCode) %> :
<%= Html.TextBox("BankCode") %>
<%= Html.ValidationMessage("BankCode", "*") %>
</div>
<div class="spanel">
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
</div>
</fieldset>
<fieldset class="mayhide">
<legend>Par codes BIC et IBAN</legend>
<div class="spanel">
<%= Html.LabelFor(model => model.BIC) %> :
<%= Html.TextBox("BIC") %>
<%= Html.ValidationMessage("BIC", "*") %>
</div><div class="spanel">
<%= Html.LabelFor(model => model.IBAN) %> :
<%= Html.TextBox("IBAN") %>
<%= Html.ValidationMessage("IBAN", "*") %>
</div>
</fieldset>
</fieldset>

</fieldset>

<fieldset class="mayhide"><legend>Interface utilisateur
<i> <%=Html.Encode(Model.UITheme)%> </i>
</legend>
<span>
<%= Html.LabelFor(model => model.UITheme) %> :
<%= Html.DropDownList("UITheme") %>
<%= Html.ValidationMessage("UITheme", "*") %></span>

</fieldset>
<input type="submit" id="submit" value="<%=Html.Translate("SubmitChanges")%>" />
<% } %>
<script>
$(document).ready(function(){
$('input').on('change',function(){$(this).addClass('dirty'); $('#submit').addClass('clickme');});
});
</script>
</asp:Content>

