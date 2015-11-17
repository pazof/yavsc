<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Writting>" %>

<%= Html.ValidationSummary("Ligne de devis") %>
<% using  (Html.BeginForm("Write","WorkFlow")) { %>
<div>
<%= Html.Hidden( "Id" ) %>

<%= Html.LabelFor(model => model.Description) %>:<%= Html.TextArea( "Description" ) %>
<%= Html.ValidationMessage("Description", "*", new { @id="Err_wr_Description", @class="error" }) %>
<br/>
<%= Html.LabelFor(model => model.ProductReference) %>:<%= Html.TextBox( "ProductReference" ) %>
<%= Html.ValidationMessage("ProductReference", "*", new { @id="Err_wr_ProductReference", @class="error" }) %>
<br/>
<%= Html.LabelFor(model => model.UnitaryCost) %>:<%= Html.TextBox( "UnitaryCost" ) %>
<%= Html.ValidationMessage("UnitaryCost", "", new { @id="Err_wr_UnitaryCost", @class="error" }) %>
<br/>
<%= Html.LabelFor(model => model.Count) %>:<%= Html.TextBox( "Count" ) %>
<%= Html.ValidationMessage("Count", "", new { @id="Err_wr_Count" , @class="error"}) %>

</div>
<% } %>




