<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%= Html.ValidationSummary() %>
<form action="AddUserToRole" method="POST" id="frmAUTR">
 <fieldset>
 <% if (ViewData ["UserName"] != null) { %>
<%= Html.Hidden("RoleName",ViewData ["RoleName"]) %>
<% } else { %>
<div>
<label for="UserName" >Utilisateur : </label>
<div id="Err_model_UserName"></div>
<input type="text" name="UserName" id="UserName">
<%= Html.ValidationMessage("UserName", "*") %>
</div>
<% } %>
<% if (ViewData ["RoleName"] != null) { %>
<%= Html.Hidden("RoleName",ViewData ["RoleName"]) %>
<% } else { %>
<div>
<label for="RoleName" >Nom du rôle : </label>
<input type="text" name="RoleName" id="RoleName">
<%= Html.ValidationMessage("RoleName", "*") %>
</div>
<% } %>
<script>
$("form").on ('submit', function (e) { return e.preventDefault(); } );
function ajaxSubmit() {
var user = $('#UserName').val();
var role = $('#RoleName').val();
Admin.addUserToRole(user, role,
function () { $('#UserName').val('');
$('[data-type="userlist"]').filter('[data-role='+role+']').each(function() {
 $('<li>'+user+'</li>').decharger({roleName: <%= YavscAjaxHelper.QuoteJavascriptString((string)ViewData["RoleName"]) %>}).appendTo(this);
});
} );
return false;
}
</script>
<input type="submit" value="Ajouter l'utilisateur au rôle <%=ViewData ["RoleName"]%>" onclick="return ajaxSubmit();" >
 </fieldset>
</form>
