<%@ Page Title="Front office" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
</asp:Content>
<asp:Content ID="MASContentContent" ContentPlaceHolderID="MASContent" runat="server">
 <ul><li>
 <%= Html.ActionLink("Catalog","Catalog" ) %>
 </li><li>
 <%= Html.ActionLink("Estimates","Estimates" ) %>
 </li></ul>
</asp:Content>
