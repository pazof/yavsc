<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<PerformerProfile>" %>

<img src="<%= Model.Avatar %>" alt="" class="avatar"> 
<div class="performer">
<h2><%=Html.Encode(Model.UserName)%></h2>
<%=Html.Partial("RateUserSkillControl", Model) %>
</div><br>
E-mail: &lt;<%=Html.Encode(Model.EMail)%>&gt;
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
<span class="fa-stack fa-lg">
  <i class="fa fa-square-o fa-stack-2x"></i>
  <i class="fa fa-folder fa-stack-1x"></i>
</span></a>
<% } %>