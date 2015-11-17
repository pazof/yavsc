<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<RegisterClientModel>" %>
<%= Html.ValidationSummary() %>
<% using(Html.BeginForm("Register")) %>
<% { %>
<h1>Nouvel utilisateur</h1>
<span class="field-validation-valid error" data-valmsg-replace="false" id="Err_ur_IsApprouved">*</span>
<table class="layout">

<tr><td align="right">
<%= Html.LabelFor(model => model.Name) %>
</td><td>
<%= Html.TextBox( "Name" ) %>
<%= Html.ValidationMessage("Name", "*", new { @id="Err_ur_Name", @class="error" }) %></td></tr>

<tr><td align="right">
<%= Html.LabelFor(model => model.UserName) %>
</td><td>
<%= Html.TextBox( "UserName" ) %>
<%= Html.ValidationMessage("UserName", "*", new { @id="Err_ur_UserName", @class="error" }) %></td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.Password) %>
</td><td>
<%= Html.Password( "Password" ) %>
<%= Html.ValidationMessage("Password", "*", new { @id="Err_ur_Password", @class="error" }) %>
</td></tr>
<tr><td align="right">
<%= Html.LabelFor(model => model.Email) %>
</td><td>
<%= Html.TextBox( "Email" ) %>
<%= Html.ValidationMessage("Email", "*", new { @id="Err_ur_Email", @class="error" }) %>
</td></tr>

<tr><td align="right">
<%= Html.LabelFor(model => model.Address) %>
</td><td>
<%= Html.TextBox( "Address" ) %>
<%= Html.ValidationMessage("Address", "*", new { @id="Err_ur_Address", @class="error" }) %></td></tr>

<tr><td align="right">
<%= Html.LabelFor(model => model.CityAndState) %>
</td><td>
<%= Html.TextBox( "CityAndState" ) %>
<%= Html.ValidationMessage("CityAndState", "*", new { @id="Err_ur_CityAndState", @class="error" }) %>


</td></tr>

<tr><td align="right">
<%= Html.LabelFor(model => model.ZipCode) %>
</td><td>
<%= Html.TextBox( "ZipCode" ) %>
<%= Html.ValidationMessage("ZipCode", "*", new { @id="Err_ur_ZipCode", @class="error" }) %></td></tr>


<tr><td align="right">
<%= Html.LabelFor(model => model.Phone) %>
</td><td>
<%= Html.TextBox( "Phone" ) %>
<%= Html.ValidationMessage("Phone", "*", new { @id="Err_ur_Phone", @class="error" }) %></td></tr>

<tr><td align="right">
<%= Html.LabelFor(model => model.Mobile) %>
</td><td>
<%= Html.TextBox( "Mobile" ) %>
<%= Html.ValidationMessage("Mobile", "*", new { @id="Err_ur_Mobile", @class="error" }) %></td></tr>

</table>
<input type="button" id="btnnewuser" class="actionlink" value="Enregistrer">
<% } %>




