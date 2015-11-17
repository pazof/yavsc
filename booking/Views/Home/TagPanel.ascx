<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<TagInfo>" %>
<div class="panel">
<h2><%=Html.Encode(Model.Name)%></h2>
<% foreach (var t in Model.Titles) { %>
<div class="postpreview">
<a href="<%= Url.RouteUrl("Titles", new { title = t.Title }) %>">
<% if (t.Photo != null ) { %>
<img src="<%=t.Photo%>" alt="placard" class="photo"><% } %>
<%= Html.Markdown(t.Title,"/bfiles/"+t.Id+"/")%></a>
 <%=  Html.Markdown(t.Intro,"/bfiles/"+t.Id+"/") %>
</div> 
<% } %>
</div>
