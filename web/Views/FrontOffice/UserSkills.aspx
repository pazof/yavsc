<%@ Page Title="Skills" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<PerformerProfile>" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
<script src="<%=Url.Content("~/Scripts/yavsc.skills.js")%>"></script>
<script>
 $(document).ready(function(){ 
  $('[data-type="rate-user-skill"]').rate({target: 'Skill/RateUserSkill'});
 });
 </script>
</asp:Content>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<aside class="control">
<form method="post" action="DeclareUserSkill">
<fieldset>
<legend>Declarer une compétence supplémentaire</legend>
<div id="Err_skillId" class="field-validation-error"></div>
<h2>
<select id="SkillId" >
<% foreach (var skill in (SkillEntity[]) ViewData["SiteSkills"]) { 
if (Model.HasSkill(skill.Id)) {%>
<option value="<%=skill.Id%>" disabled>
<%=skill.Name%></option>
<% } else { %>
<option value="<%=skill.Id%>"><%=skill.Name%></option>
<% } } %>
</select></h2>
<label for="Comment">Si vous le désirez, faites un commentaire sur cette compétence</label>
<textarea id="Comment"></textarea>
<input type="button" value="Ajouter la compétence à votre profile" id="btndeclare" >
</fieldset>
</form>
<script type="text/javascript">
 $(document).ready(function () {
 $('#btndeclare').click( function() {
 var sid = $('#SkillId').val();
 var cmnt = $('#Comment').val();
 var sname = $('#SkillId').text();
 Skills.declareUserSkill({ username: <%=YavscAjaxHelper.QuoteJavascriptString(Model.UserName) %> , 
 skillid: sid, rate: 50, comment: cmnt }, function(usid) {
 console.log('User skill created, id : '+usid);
 $('<li>'+sname+'</li>').data('sid',sid).data('id',usid).addClass('skillname').appendTo('#userskills');
  $('#SkillId').val(0);
 }); }); });
</script>
</aside>

<ul id="userskills">
<% foreach (var userskill in Model.Skills) { %>
<li class="skillname" data-sid="<%= userskill.Id %>">
<h2><%= userskill.SkillName %></h2><br> <div data-type="comment"><%= userskill.Comment %></div> <%=Html.Partial("RateUserSkillControl", userskill) %>
</li>
<% } %>
</ul>
</asp:Content>
