﻿<%@ Page Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<%= Html.ActionLink("Delete","FileSystem") %>
<%= Html.ActionLink("Rename","FileSystem") %>
</asp:Content>