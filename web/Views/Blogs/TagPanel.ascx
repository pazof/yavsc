<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<TagInfo>" %>
<h2><%=Html.Encode(Model.Name)%></h2>
<% foreach (var g in Model.Titles) { %>
<div class="panel">
<h3><%= Html.Markdown(g.Key)%></h3>
<% foreach (var p in g) { %>
<a href="<%= Url.RouteUrl("Titles", new { title = g.Key, postid = p.Id }) %>">
<% if (p.Photo != null) { %> <img src="<%=p.Photo%>" alt="placard" class="photo"> <% } %>
<div class="postpreview">
 <p><%=  Html.Markdown(p.Intro,"/bfiles/"+p.Id+"/") %></p>
</div> </a>
<% } %>
</div>
<% } %>