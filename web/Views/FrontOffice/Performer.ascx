<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PerformerProfile>" %>

<% Profile userProfile = new Profile(Model.UserName);
userProfile.Populate(); %>
<div class="performer">
<h2>
<span class="username" data-type="user" data-user="<%=Model.UserName%>"
data-roles="<%=string.Join (" ",Roles.GetRolesForUser (Model.UserName)) %>" 
 data-blog-counter="<%=BlogManager.GetPostCounter(Model.UserName)%>" 
 data-circles="<%=string.Join (" ",ViewData["Circles"])%>" >
 <img src="<%= userProfile.avatar %>" alt="" class="bigavatar"> 
 <%=Html.Encode(Model.UserName)%> </span>
<%=Html.Partial("RateUserSkillControl", Model) %>
</h2>
<address>
 <% if (Membership.GetUser()!=null) { %>
<a class="actionlink" href="<%=Url.RouteUrl("Performance",new { action="Contact", Performer = Model.UserName })%>">
<i class="fa fa-envelope"></i>
</a>
 <% } else { %>
 <i class="fa fa-envelope"></i>
 <%=Html.LabelFor(m => m.EMail)%>:
 <%= Html.Translate("OnlyAuthorizedMayContact") %>
 <% }%>
</address>

<% if (Model.Skills==null) { %>
Cet utilisateur n'a pas saisi de compétence particulière ...
  <% } else if (Model.Skills.Count()>0) { %>

  <ul style="padding:0; margin:0;"> <% foreach (var userskill in Model.Skills) { %>
<li class="skillname" data-sid="<%= userskill.Id %>">
<%= userskill.SkillName %> 
<% if (!string.IsNullOrWhiteSpace(userskill.Comment)) { %>
<div data-type="comment">
&quot;<%= userskill.Comment %>&quot;
</div><% } %>
 <%=Html.Partial("RateUserSkillControl", userskill) %>
 </li>
<% } %>
</ul>

<% } %>
<% if (userProfile.GoogleCalendar!=null) { %>
<i class="fa fa-calendar-check" ><%= Html.Translate("Google_calendar") %> : <%= Html.Translate("available") %>.</i><br>
<% } %>




</div>