<%@ Page Title="Profile" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<Profile>" %>

<asp:Content ContentPlaceHolderID="init" ID="init1" runat="server">
<% Title = ViewData["UserName"]+" at "+ YavscHelpers.SiteName +" - profile edition" ; %>
</asp:Content>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
  
   <%= Html.ValidationSummary() %>
<% using(Html.BeginForm("Profile", "Account", FormMethod.Post, new { enctype = "multipart/form-data" })) %>
<% { %>
   <h2>Informations générales</h2>
<table class="layout">
<tr><td align="right">
<%= Html.LabelFor(model => model.Name) %></td><td>
<%= Html.TextBox("Name") %>
<%= Html.ValidationMessage("Name", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.Phone) %></td><td>
<%= Html.TextBox("Phone") %>
<%= Html.ValidationMessage("Phone", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.Mobile) %></td><td>
<%= Html.TextBox("Mobile") %>
<%= Html.ValidationMessage("Mobile", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.Address) %></td><td>
<%= Html.TextBox("Address") %>
<%= Html.ValidationMessage("Address", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.CityAndState) %></td><td>
<%= Html.TextBox("CityAndState") %>
<%= Html.ValidationMessage("CityAndState", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.ZipCode) %></td><td>
<%= Html.TextBox("ZipCode") %>
<%= Html.ValidationMessage("ZipCode", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.Country) %></td><td>
<%= Html.TextBox("Country") %>
<%= Html.ValidationMessage("Country", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.WebSite) %></td><td>
<%= Html.TextBox("WebSite") %>
<%= Html.ValidationMessage("WebSite", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.BlogVisible) %></td><td>
<%= Html.CheckBox("BlogVisible") %>
<%= Html.ValidationMessage("BlogVisible", "*") %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.BlogTitle) %></td><td>
<%= Html.TextBox("BlogTitle") %>
<%= Html.ValidationMessage("BlogTitle", "*") %></td></tr>
<tr><td align="right">
Avatar : "<%=Model.avatar%>"  </td><td> <img class="avatar" src="<%=Model.avatar%>" alt=""/>
<input type="file" id="AvatarFile" name="AvatarFile"/>
<%= Html.ValidationMessage("AvatarFile", "*") %></td></tr>
<tr><td align="right">
   <%= Html.LabelFor(model => model.GoogleCalendar) %>:
   </td>
   <td> <%= Html.Encode(Model.GoogleCalendar) %>
   <%= Html.ActionLink("Choisir l'agenda","ChooseCalendar","Google",new { returnUrl= Request.Url.AbsolutePath }, new { @class="actionlink" }) %>
   </td>
   </tr>
</table>

   <h2>Informations de facturation</h2>
<table class="layout">
<tr>
   <td align="right">
<%= Html.LabelFor(model => model.BankCode) %>
   </td>
   <td>
<%= Html.TextBox("BankCode") %>
<%= Html.ValidationMessage("BankCode", "*") %>
   </td>
</tr>
   <tr>
   <td align="right">
<%= Html.LabelFor(model => model.WicketCode) %></td>
   <td>
<%= Html.TextBox("WicketCode") %>
<%= Html.ValidationMessage("WicketCode", "*") %>
   </td>
   </tr>

   <tr>
   <td align="right">
<%= Html.LabelFor(model => model.AccountNumber) %></td>
   <td>
<%= Html.TextBox("AccountNumber") %>
<%= Html.ValidationMessage("AccountNumber", "*") %>
   </td>
   </tr>


   <tr>
   <td align="right">
<%= Html.LabelFor(model => model.BankedKey) %></td>
   <td>
<%= Html.TextBox("BankedKey") %>
<%= Html.ValidationMessage("BankedKey", "*") %>
   </td>
   </tr>

   <tr>
   <td align="right">
<%= Html.LabelFor(model => model.BIC) %></td>
   <td>
<%= Html.TextBox("BIC") %>
<%= Html.ValidationMessage("BIC", "*") %>
   </td>
   </tr>

   <tr>
    <td align="right">
<%= Html.LabelFor(model => model.IBAN) %></td>
   <td>
<%= Html.TextBox("IBAN") %>
<%= Html.ValidationMessage("IBAN", "*") %>
   </td>
   </tr>


</table>


<input type="submit"/>
<% } %>


</asp:Content>


<asp:Content ID="MASC1" ContentPlaceHolderID="MASContent" runat="server">
   <% 	if (Roles.IsUserInRole("Admin")) { %>
   Admin,
   Bankable:<%= Model.IsBankable %>, Billable:<%=Model.IsBillable%>
<% } %>
		
   <%= Html.ActionLink("Changer de mot de passe","ChangePassword", "Account",null, new { @class="actionlink" })%>
   <%= Html.ActionLink("Désincription","Unregister", "Account",null, new { @class="actionlink" })%>
</asp:Content>
