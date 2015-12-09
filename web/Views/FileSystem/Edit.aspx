<%@ Page Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<%= Html.TranslatedActionLink("Delete","FileSystem") %>
<%= Html.TranslatedActionLink("Rename","FileSystem") %>
</asp:Content>
