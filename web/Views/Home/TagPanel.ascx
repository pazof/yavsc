<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<TagInfo>" %>
<div class="panel">
<i class="fa fa-tag"><%=Html.Encode(Model.Name)%></i>
<% foreach (var t in Model.Titles) { %>
<a href="<%= Url.RouteUrl("Titles", new { title = t.Title }) %>">
<img src="<%=Url.Encode(t.Photo)%>" alt="placard" class="photo">
<%= Html.Markdown(t.Title,"/bfiles/"+t.Id+"/")%></a>
<div class="postpreview">
 <p><%=  Html.Markdown(t.Intro,"/bfiles/"+t.Id+"/") %></p>
</div> 
<% } %>
</div>