<%@ Page Title="Login" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%= Html.ValidationSummary("Ouverture de session avec OpenID") %>
<% using(Html.BeginForm("OpenIDLogOn", "Account")) %>
<% { %>
<label for="loginIdentifier"/>
<%= Html.TextBox( "loginIdentifier" ) %>
<%= Html.ValidationMessage("loginIdentifier", "*") %>
<% } %>
</asp:Content>
