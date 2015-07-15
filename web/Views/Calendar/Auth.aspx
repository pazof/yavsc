<%@ Page Title="Catalog" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
AccessToken : <%= Session["AccessToken"] %>
Target in error : <%= ViewData["TargetNameError"] %>
</asp:Content>
