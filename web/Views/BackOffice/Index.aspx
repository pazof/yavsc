<%@ Page Title="Back office" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
 <ul>
 <li><%= Html.ActionLink("Notifier des cercles d'un évennement","NotifyEvent","BackOffice" ) %></li>
 </ul>
</asp:Content>
