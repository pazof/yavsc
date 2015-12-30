<%@ Page Title="Front office" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">

<%= Html.Translate("YourMessageHasBeenSent") %>
Votre message a été envoyé

</asp:Content>
