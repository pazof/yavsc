<%@ Page Title="Successfully changed your password" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>
<asp:Content ContentPlaceHolderID="head" ID="headContent" runat="server">
</asp:Content>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div>
<%= Html.ActionLink("Register","Register")%></div>
<div><%= Html.ActionLink("ChangePassword","ChangePassword")%></div>
</asp:Content>
