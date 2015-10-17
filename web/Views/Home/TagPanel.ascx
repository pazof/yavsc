<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<TagInfo>" %>
<div class="panel">
<i class="fa fa-tag"><%=Model.Name%></i>
<ul>
<% foreach (BasePostInfo be in Model.Titles) { %>
<li><%= be.Title %>
<span><%= be.Intro %></span>
</li>
<% } %></ul>
</div>