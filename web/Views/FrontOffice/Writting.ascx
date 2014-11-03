<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Writting>" %>

<%= Html.ValidationSummary("Ligne de devis") %>
<% using  (Html.BeginForm("Write","WorkFlow")) { %>
<%= Html.Hidden( "Id" ) %>
<%= Html.Hidden( "EstimateId", (long) ViewData["EstimateId"]) %>

<%= Html.LabelFor(model => model.Description) %>:<%= Html.TextBox( "Description" ) %>
<%= Html.ValidationMessage("Description", "*") %>
<br/>
<%= Html.LabelFor(model => model.ProductReference) %>:<%= Html.TextBox( "ProductReference" ) %>
<%= Html.ValidationMessage("ProductReference", "*") %>
<br/>
<%= Html.LabelFor(model => model.UnitaryCost) %>:<%= Html.TextBox( "UnitaryCost" ) %>
<%= Html.ValidationMessage("UnitaryCost", "*") %>
<br/>
<%= Html.LabelFor(model => model.Count) %>:<%= Html.TextBox( "Count" ) %>
<%= Html.ValidationMessage("Count", "*") %><br/>
<% } %>




