<%@ Page Title="Ajout d'un role" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<%= Html.ValidationSummary() %>
<% using(Html.BeginForm("DoAddRole", "Account")) %>
<% { %>
Nom du rôle : 
<%= Html.TextBox( "RoleName" ) %>
<%= Html.ValidationMessage("RoleName", "*") %><br/>
<input class="actionlink" type="submit"/>
<% } %>
</asp:Content>


