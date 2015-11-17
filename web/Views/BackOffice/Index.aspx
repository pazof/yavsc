<%@ Page Title="Back office" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
 <ul><li>
 <%= Html.ActionLink("Catalog","Catalog","FrontOffice" ) %>
 </li>
 <li><%= Html.ActionLink(LocalizedText.Skill,"Skills","FrontOffice" ) %></li>
 </ul>
</asp:Content>
