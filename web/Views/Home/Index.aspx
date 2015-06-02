<%@ Page Title="Home" Language="C#" Inherits="System.Web.Mvc.ViewPage" MasterPageFile="~/Models/App.master"%>
<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<div>
<%= Html.ActionLink("Les blogs", "Index", "Blogs",null, new { @class="actionlink" }) %>
<%= Html.ActionLink("Contact", "Contact", "Home", null, new { @class="actionlink" }) %>
<%= Html.ActionLink("Version des librairies", "AssemblyInfo", "Home", null, new { @class="actionlink" }) %>
</div>
 </asp:Content>

