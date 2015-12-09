<%@ Page Title="Successfully changed your password" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div>
<%= Html.TranslatedActionLink("Register","Register")%></div>
<div><%= Html.TranslatedActionLink("ChangePassword","ChangePassword")%></div>
</asp:Content>
