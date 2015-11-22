<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PerformerProfile>" %>
<img src="<%=Url.AvatarUrl(HttpContext.Current.User.Identity.Name)%>" alt="avatar" class="iconsmall" />
<%=Html.Encode(Model.UserName)%> <%= Html.Partial("RateControl",Model) %>
<ul>
<% foreach (var skill in Model.Skills) { %>
<li>
<%=Html.Encode(skill.SkillName)%> <%= Html.Partial("RateControl",skill) %> 
<%=Html.Encode(skill.Comment)%> 
</li>
<% } %>
</ul>
<% if (Model.HasCalendar()) { %>
<i class="fa fa-check">Calendrier Google Disponible</i>
<% } %>
