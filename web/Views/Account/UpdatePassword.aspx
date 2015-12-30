<%@ Page Title="Reset your Password" Language="C#" Inherits="System.Web.Mvc.ViewPage<LostPasswordModel>" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%= Html.ValidationSummary("Modification de mot de passe") %>

<% using(Html.BeginForm("UpdatePassword", "Account")) { %>
<%= Html.ValidationSummary() %>
Modifiez votre mot de passe (<%= Html.Encode(ViewData["UserName"]) %>):
<ul><li>
<label for="Password">Saisissez ci après votre nouveau mot de passe:</label>
<%= Html.Password( "Password" ) %>
<%= Html.ValidationMessage("Password", "*") %></li>
<li>
<label for="ConfirmPassword">Confirmez votre nouveau mot de passe:</label>
 <%= Html.Password( "ConfirmPassword" ) %>
<%= Html.ValidationMessage("ConfirmPassword", "*") %>
</li>
</ul>
Puis, actionnez ce bouton: 
<input type="submit" value="Modifiez mon mot de passe!"/>
<%= Html.Hidden("UserKey") %>
<%= Html.Hidden("UserName") %>
<% } %>
</asp:Content>


