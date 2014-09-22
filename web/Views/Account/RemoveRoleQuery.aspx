﻿<%@ Page Title="User removal" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="header" ID="headerContent" runat="server">
<h2>Suppression d'un rôle</h2>
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div>
<%= Html.ValidationSummary() %>
<% using ( Html.BeginForm("RemoveRole") ) {  %>
Supprimer le rôle 
<%= Html.Encode( ViewData["roletoremove"] ) %> ?
<br/>
<input type="hidden" name="rolename" value="<%=ViewData["roletoremove"]%>"/>
<input class="actionlink" type="submit" name="submitbutton" value="Supprimer"/>
<input class="actionlink" type="submit" name="submitbutton" value="Annuler" />
<% } %>
</div>

</asp:Content>

