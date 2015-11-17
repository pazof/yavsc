<%@ Page Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<System.IO.FileInfo>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<%= Model.Name %><br/>
Création : 
<%= Model.CreationTime %><br/>
Dérnière modification : 
<%= Model.LastWriteTime %><br/>
Dernier accès : 
<%= Model.LastAccessTime %><br/>
Lien permanent : <a href="<%= ViewData["url"] %>"><%= ViewData["url"] %></a>
</asp:Content>
