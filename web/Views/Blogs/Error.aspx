<%@ Page Title="Comptes utilisateur - Index" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">	
<h2>Comptes utilisteur</h2>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">

<div>
<%= Html.Encode(ViewData["Message"]) %></div>
Your UID : 
<%= Html.Encode(ViewData ["UID"]) %> </br>
<a class="actionlink" href="<%=ViewData["returnUrl"]%>">Retour</a> 
</asp:Content>
