<%@ Page Title="Comptes utilisateur - Erreur" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div>
<%= Html.Encode(ViewData["Message"]) %></div>
Your UID : 
<%= Html.Encode(ViewData ["UID"]) %> </br>
<a class="actionlink" href="<%=ViewData["returnUrl"]%>">Retour</a> 
</asp:Content>
