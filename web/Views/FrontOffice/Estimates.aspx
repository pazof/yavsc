<%@ Page Title="My estimates" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Estimate>>" %>
<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<% foreach (Estimate estim in Model) { %>
	<%= Html.ActionLink(estim.Id.ToString(),"Estimate",new {Id=estim.Id}) %>
<% } %>
</asp:Content>
<asp:Content ID="MASContentContent" ContentPlaceHolderID="MASContent" runat="server">
</asp:Content>
