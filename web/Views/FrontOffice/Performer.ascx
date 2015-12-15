<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PerformerProfile>" %>
<div class="performer">
<h2>
<img src="<%= Model.Avatar %>" alt="" class="bigavatar"> 
<span class="username" data-type="user" 
data-roles="<%=string.Join (" ",Roles.GetRolesForUser (Model.UserName)) %>" 
 data-blog-counter="<%=BlogManager.GetPostCounter(Model.UserName)%>" 
 data-circles="<%=string.Join (" ",CircleManager.Circles(Model.UserName))%>" >
 <%=Html.Encode(Model.UserName)%></span>
<%=Html.Partial("RateUserSkillControl", Model) %>
</h2>
<address>
 <% if (Membership.GetUser()!=null) { %>
<a href="mailto:<%=Model.EMail%>">
<i class="fa fa-envelope"></i>
&lt;<%=Html.Encode(Model.EMail)%>&gt;
</a>
 <% } else { %>
 <i class="fa fa-envelope"></i>
 <%=Html.LabelFor(m => m.EMail)%>:
 <%= Html.Translate("AuthenticatedOnly") %>
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
<% if (Model.HasCalendar()) { %>
<i class="fa fa-calendar-check" ><%= Html.Translate("Google_calendar") %> : <%= Html.Translate("available") %>.</i><br>
<% } %>
<% if (BlogManager.GetPostCounter(Model.UserName)>0) { %>
<a href="<%=Url.RouteUrl("Blogs",new { user = Model.UserName } )%>">
  <i class="fa fa-folder"><%=Model.BlogTitle %></i>
</a>
<% } %>



</div>