﻿<%@ Page Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<DataAccess>" %>
<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="headerContent" ContentPlaceHolderID="header" runat="server">
</asp:Content>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<%= Html.ActionLink("Backups","Backups","BackOffice") %>
</asp:Content>