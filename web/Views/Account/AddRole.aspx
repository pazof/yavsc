<%@ Page Title="" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">
<h2>Ajout d'un role</h2>
</asp:Content>
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


