<%@ Page Title="Ligne de devis" Language="C#" Inherits="System.Web.Mvc.ViewPage<Writting>" MasterPageFile="~/Models/App.master" %>

<asp:Content ContentPlaceHolderID="MainContent" ID="MainContentContent" runat="server">
<%= Html.ValidationSummary("Ligne de devis") %>
<% using  (Html.BeginForm("Write","WorkFlow")) { %>
<%= Html.LabelFor(model => model.Id) %>:<%=Model.Id%>
<%= Html.Hidden( "Id" ) %>
<%= Html.Hidden( "EstimateId", (long) ViewData["EstimateId"]) %>

<%= Html.LabelFor(model => model.UnitaryCost) %>:<%= Html.TextBox( "UnitaryCost" ) %>
<%= Html.ValidationMessage("UnitaryCost", "*") %>

<% } %>
</asp:Content>
