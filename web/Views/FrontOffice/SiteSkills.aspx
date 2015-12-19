﻿<%@ Page Title="ManagedSiteSkills" Language="C#" MasterPageFile="~/Models/App.master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<SkillEntity>>" %>

<asp:Content ID="headContent" ContentPlaceHolderID="head" runat="server">
<script src="<%=Url.Content("~/Scripts/yavsc.skills.js")%>"></script>
<script>
 $(document).ready(function(){ 
  $('[data-type="rate-site-skill"]').rate({webTarget: 'Skill/RateSkill'});
 });
 </script>
</asp:Content>

<asp:Content ID="MainContentContent" ContentPlaceHolderID="MainContent" runat="server">
<aside class="control">
<form method="post" action="DeclareSkill">
<fieldset>
<label for="MEACode"><%= Html.Translate("MEACode") %> :</label>
<%= Html.DropDownList("MEACode") %>

<div id="Err_skillName" class="field-validation-error"></div>
<input type="text" name="SkillName" id="SkillName" >
<input type="button" value="Créer la compétence" id="btncreate" >
</fieldset>
</form>
<script type="text/javascript">
 $(document).ready(function () {
 $('#btncreate').click( function() {
 var $sname = $('#SkillName').val();
 var $meacode = $('#MEACode').val();
 Skills.createSkill({Name: $sname, MEACode: $meacode }, function(sid) {
 $('<li>'+$sname+'</li>').data('sid',sid).addClass('skillname').appendTo('#skills');
 $('#SkillName').val('');
 }); } ); });
</script>
</aside>

<ul id="skills">
<% foreach (var skill in Model) { %>
<li class="skillname" data-sid="<%= skill.Id %>">
<%= skill.Name %> <%=Html.Partial("RateSkillControl", skill) %>
</li>
<% } %>
</ul>

</asp:Content>