<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PerformerProfile>" %>
<div class="performer">
<h2>
<img src="<%= Model.Avatar %>" alt="" class="bigavatar"> 
<%=Html.Encode(Model.UserName)%> 
<%=Html.Partial("RateUserSkillControl", Model) %>
</h2>
<p>
<i class="fa fa-envelope"> 
 <% if (Membership.GetUser()!=null) { %>
&lt;<%=Html.Encode(Model.EMail)%>&gt;
 <% } else { %><%=Html.LabelFor(m => m.EMail)%>:
 <i><%= Html.Translate("AuthenticatedOnly") %></i>
 <% }%>
 </i>
 </p>

<% if (Model.Skills==null) { %>
<%= Html.Translate("") %>
  <% } else 
foreach (var userskill in Model.Skills) { %>
<li class="skillname" data-sid="<%= userskill.Id %>">
<%= userskill.SkillName %> 
<div data-type="comment">&quot;<%= userskill.Comment %>&quot;</div>
 <%=Html.Partial("RateUserSkillControl", userskill) %>
</li>
<% } %>
<% if (Model.HasCalendar()) { %>
<i class="fa fa-calendar-check" ><%= Html.Translate("Google_calendar") %> : <%= Html.Translate("available") %>.</i><br>
<% } %>
<% if (BlogManager.GetPostCounter(Model.UserName)>0) { %>
<a href="<%=Url.RouteUrl("Blogs",new { user = Model.UserName } )%>">
  <i class="fa fa-folder"><%=Model.BlogTitle %></i>
</a></div>
<% } %>