﻿<%@ Page Title="User removal" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div>
<%= Html.ValidationSummary() %>
<% using ( Html.BeginForm("RemoveUser","Admin") ) {  %>
Supprimer l'utilisateur 
<%= Html.Encode( ViewData["usertoremove"] ) %> ?
<br/>
<input type="hidden" name="username" value="<%=ViewData["usertoremove"]%>"/>
<input class="actionlink" type="submit" name="submitbutton" value="Supprimer"/>
<input class="actionlink" type="submit" name="submitbutton" value="Annuler" />
<% } %>
</div>

</asp:Content>

