<%@ Page Title="Home" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%= Html.Partial("TagPanel",ViewData["Accueil"]) %>
<%= Html.Partial("TagPanel",ViewData["Événements"]) %>
<%= Html.Partial("TagPanel",ViewData["Mentions légales"]) %>

</asp:Content>
<asp:Content ContentPlaceHolderID="MASContent" ID="MASContentContent" runat="server">
<div class="panel">
<%= Html.ActionLink("Les articles", "Index", "Blogs") %>
<%= Html.ActionLink("Contact", "Contact", "Home", null, new { @class="actionlink" }) %>
<%= Html.ActionLink("Credits", "Credits", "Home", null, new { @class="actionlink" }) %>
<%= Html.ActionLink("Version des librairies", "AssemblyInfo", "Home", null, new { @class="actionlink" }) %>
</div>
 </asp:Content>
