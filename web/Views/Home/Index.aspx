<%@ Page Title="Home" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%= Html.Partial("TagPanel",ViewData["Accueil"]) %>
<%= Html.Partial("TagPanel",ViewData["Actualités"]) %>
<%= Html.Partial("TagPanel",ViewData["Artistes"]) %>
<%= Html.Partial("TagPanel",ViewData["Mentions légales"]) %>

</asp:Content>
<asp:Content ContentPlaceHolderID="MASContent" ID="MASContentContent" runat="server">
<div>
<%= Html.ActionLink("Les articles", "Index", "Blogs", null, new { @class="actionlink" }) %>
<%= Html.ActionLink("Contact", "Contact", "Home", null, new { @class="actionlink" }) %>
<%= Html.ActionLink("Credits", "Credits", "Home", null, new { @class="actionlink" }) %>
<%= Html.ActionLink("Version des librairies", "AssemblyInfo", "Home", null, new { @class="actionlink" }) %>
</div>
 </asp:Content>

