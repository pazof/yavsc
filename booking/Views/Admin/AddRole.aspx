<%@ Page Title="Ajout d'un role" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/AppAdmin.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<%= Html.ValidationSummary() %>
<% using(Html.BeginForm()) 
 { %>
Nom du rôle : 
<%= Html.TextBox( "RoleName" ) %>
<%= Html.ValidationMessage("RoleName", "*") %><br/>
<input class="actionlink" type="submit"/>
<% } %>
<%= Html.Partial("AddMemberToRole")%>
</asp:Content>


